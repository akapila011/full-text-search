using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using full_text_search.dataloader;
using full_text_search.models;
using full_text_search.utilities;

namespace full_text_search.indices {
    
    [Serializable]
    public class InvertedIndex { // TODO: use generics later with interfaces

        public string Path { get; private set; }  // directory path for now, allow file path and extensions parsing options later
        public string MD5 { get; private set; }
        public string Alias { get; private set; }
        private Dictionary<string, HashSet<string>> index; // token: [document id's]
        private IList<string> allowedExtensions = new List<string> { "txt", "csv", "cs", "xml"};

        public InvertedIndex(string path, string md5, string alias = null) {
            this.Path = path;
            this.MD5 = md5;
            this.index = new Dictionary<string, HashSet<string>>();
            if (!string.IsNullOrWhiteSpace(alias)) {
                this.Alias = alias;
            }
        }

        public override string ToString() {
            return $"InvertedIndex(Path={this.Path},MD5={this.MD5},Alias={this.Alias}," +
                   $"index={this.index.Count},allowedExtensions={this.allowedExtensions.Count})";
        }

        public int BuildIndex() {
            var watch = System.Diagnostics.Stopwatch.StartNew();
            IList<string> validFilePaths = new List<string>();
            FileAttributes attr = File.GetAttributes(this.Path);
            if (attr.HasFlag(FileAttributes.Directory)) {
                validFilePaths = FileUtilities.GetValidFilePaths(this.Path, allowedExtensions); // TODO: Use DI for file work
            }
            else {
                for (int i=0; i<allowedExtensions.Count; i++) {
                    var extension = allowedExtensions[i];
                    if (this.Path.EndsWith(extension)) {
                        validFilePaths.Add(this.Path);
                        i = allowedExtensions.Count; // don't continue checking other extensions for a path
                    }
                }
            }

            if (validFilePaths.Count == 0) {
                Console.WriteLine($"No index built for '{this.Path}', no valid files found.");
            }

            foreach (var filepath in validFilePaths) {
                var filename = System.IO.Path.GetFileName(filepath);
                var document = new BasicFileDocument(filepath, filename, filepath);
                foreach (var line in document.LoadFileLine()) {
                    var tokens = TextUtilities.tokenize(line);
                    tokens = TextUtilities.lowerCaseTokens(tokens);
                    foreach (var token in tokens) {  // TODO: O(n^n), need to find a better way to index
                        if (!this.index.ContainsKey(token)) {
                            this.index[token] = new HashSet<string>();
                        }
                        this.index[token].Add(document.ID);
                    }
                }
            }
            watch.Stop();
            var elapsedMs = watch.ElapsedMilliseconds;
            Console.WriteLine($"Built index for '{this.Path}', {this.index.Count} indexed words in {elapsedMs} ms");
            return this.index.Count;
        }

        public List<IndexResult> search(string searchTerm) {
            var watch = System.Diagnostics.Stopwatch.StartNew();
            var results = new List<IndexResult>();
            
            HashSet<string> resultDocumentIds = new HashSet<string>();
            var tokens = TextUtilities.tokenize(searchTerm);
            tokens = TextUtilities.lowerCaseTokens(tokens);
            foreach (var token in tokens) {
                if (this.index.TryGetValue(token, out var documentValues)) {
                    resultDocumentIds.UnionWith(documentValues);
                    results.Add(new IndexResult(token, documentValues.ToList()));
                }
            }
            watch.Stop();
            var elapsedMs = watch.ElapsedMilliseconds;
            Console.WriteLine($"Search for '{searchTerm}' returned {resultDocumentIds.Count} results {elapsedMs} ms");
            return results; // TODO: need to do post processing so docs that have both words or so get high priority
        }
    }
}