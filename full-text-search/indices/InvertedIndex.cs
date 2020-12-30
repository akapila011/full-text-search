using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using full_text_search.dataloader;
using full_text_search.models;
using full_text_search.utilities;

namespace full_text_search.indices {
    
    [Serializable]
    public class InvertedIndex {

        private const uint serializeVersion = 1;
        
        public string Path { get; private set; }  // directory path for now, allow file path and extensions parsing options later
        public string MD5 { get; private set; }
        public string Alias { get; private set; }
        private Dictionary<string, HashSet<string>> index; // token: [document id's]
        private Dictionary<string, string> idMapping;  // document id: less space consuming value like hash

        public InvertedIndex(string path, string md5, string alias = null) {
            this.Path = path;
            this.MD5 = md5;
            this.index = new Dictionary<string, HashSet<string>>();
            this.idMapping = new Dictionary<string, string>();
            if (!string.IsNullOrWhiteSpace(alias)) {
                this.Alias = alias;
            }
        }

        public override string ToString() {
            return $"InvertedIndex(Path={this.Path},MD5={this.MD5},Alias={this.Alias}," +
                   $"index={this.index.Count})";
        }

        public int BuildIndex(IList<string> validFilePaths) {
            var watch = System.Diagnostics.Stopwatch.StartNew();

            if (validFilePaths.Count == 0) {
                Console.WriteLine($"No index built for '{this.Path}', no valid files found.");
            }

            foreach (var filepath in validFilePaths) {
                var filename = System.IO.Path.GetFileName(filepath);
                var document = new BasicFileDocument(filepath, filename, filepath);
                foreach (var line in document.LoadFileLine()) {
                    var tokens = TextUtilities.tokenize(line);
                    tokens = TextUtilities.lowerCaseTokens(tokens);
                    foreach (var token in tokens) {
                        if (!this.index.ContainsKey(token)) {
                            this.index[token] = new HashSet<string>();
                        }

                        var idHash = HashUtilities.CreateMD5Hash(document.ID); 
                        if (!this.idMapping.ContainsKey(idHash)) {
                            this.idMapping[idHash] = document.ID;
                        }
                        this.index[token].Add(idHash);
                    }
                }
            }
            watch.Stop();
            var elapsedMs = watch.ElapsedMilliseconds;
            Console.WriteLine($"Built index for '{this.Path}', {this.index.Count} indexed words in {elapsedMs} ms");
            return this.index.Count;
        }

        public IList<SearchResult> Search(string searchTerm) {
            var watch = System.Diagnostics.Stopwatch.StartNew();
            var results = new List<IndexResult>();
            
            HashSet<string> resultDocumentIds = new HashSet<string>();
            var tokens = TextUtilities.tokenize(searchTerm);
            tokens = TextUtilities.lowerCaseTokens(tokens);
            foreach (var token in tokens) {
                if (this.index.TryGetValue(token, out var documentCompressedIds)) {
                    var documentIds = new HashSet<string>();
                    foreach (var documentCompressedId in documentCompressedIds) {
                        var documentId = this.idMapping[documentCompressedId];
                        documentIds.Add(documentId);
                    }
                    resultDocumentIds.UnionWith(documentIds);
                    results.Add(new IndexResult(token, documentIds.ToList()));
                }
            }
            watch.Stop();
            var elapsedMs = watch.ElapsedMilliseconds;
            Console.WriteLine($"Search for '{searchTerm}' returned {resultDocumentIds.Count} results {elapsedMs} ms");
            return SearchResultProcessor.ProcessResults(results);
        }
    }
}