using System;
using System.Collections.Generic;
using full_text_search.dataloader;
using full_text_search.models;

namespace full_text_search.indices {
    
    public class InvertedIndex { // TODO: use generics later with interfaces

        public string Path { get; private set; }  // directory path for now, allow file path and extensions parsing options later
        public string MD5 { get; private set; }
        public string Alias { get; private set; }
        private Dictionary<string, HashSet<string>> index;  // TODO: figure how this can be serialized and saved

        public InvertedIndex(string path, string md5, string alias = null) {
            this.Path = path;
            this.MD5 = md5;
            this.index = new Dictionary<string, HashSet<string>>();
            if (!string.IsNullOrWhiteSpace(alias)) {
                this.Alias = alias;
            }
        }

        public int BuildIndex() {
            var watch = System.Diagnostics.Stopwatch.StartNew();
            List<Document> documents = XmlLoader.LoadDocumentsFromXmlList(this.Path, "doc");
            // TODO: support for json, txt - should have a general reader to parse any document
            foreach (var document in documents) {
                var words = Array.ConvertAll(document.Text.Split(" "), d => d.ToLower());
                // TODO: need to normalize: lowercase, remove stop words, stemming util func etc
                foreach (var word in words) {  // TODO: O(n^n), need to find a better way to index
                    if (!this.index.ContainsKey(word)) {
                        this.index[word] = new HashSet<string>();
                    }
                    this.index[word].Add(document.ID.ToString());
                }
            }
            watch.Stop();
            var elapsedMs = watch.ElapsedMilliseconds;
            Console.WriteLine($"Built index for '{this.Path}', {this.index.Count} indexed words in {elapsedMs} ms");
            return this.index.Count;
        }

        public void search(string searchTerm) {  // TODO: will figure away to return results to be used otherwise this function is useless other than prints
            var watch = System.Diagnostics.Stopwatch.StartNew();
            
            HashSet<string> resultDocumentIds = new HashSet<string>();
            var words = Array.ConvertAll(searchTerm.Split(" "), d => d.ToLower());
            foreach (var word in words) {
                if (this.index.TryGetValue(word, out var documentValues)) {
                    resultDocumentIds.UnionWith(documentValues);
                }
            }

            watch.Stop();
            var elapsedMs = watch.ElapsedMilliseconds;
            Console.WriteLine($"Search for '{searchTerm}' returned {resultDocumentIds.Count} results {elapsedMs} ms"); 
        }
    }
}