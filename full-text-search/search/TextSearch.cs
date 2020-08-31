
using System;
using System.Collections.Generic;
using full_text_search.dataloader;
using full_text_search.models;

namespace full_text_search.search {
    
    public class TextSearch {

        private String searchTerm;
        private String searchPath;

        public TextSearch(string searchTerm, string searchPath) {
            this.searchTerm = searchTerm;
            this.searchPath = searchPath;
        }

        public void search() {
            var watch = System.Diagnostics.Stopwatch.StartNew();
            
            List<Document> documents = XmlLoader.LoadDocumentsFromXmlList(searchPath, "doc");
            List<Document> results = new List<Document>();

            foreach (var document in documents) {
                if (document.Text.Contains(this.searchTerm)) {
                    results.Add(document);
                }
            }

            watch.Stop();
            var elapsedMs = watch.ElapsedMilliseconds;
            Console.WriteLine($"Found {results.Count} results in {elapsedMs} ms");
        }
        
    }
}