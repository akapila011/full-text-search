﻿
using System;
using System.Collections.Generic;
using full_text_search.dataloader;
using full_text_search.models;
using System.Text.RegularExpressions;

namespace full_text_search.search {
    
    public class TextSearch {

        private String searchTerm;
        private List<Document> documents;

        public TextSearch(string searchTerm, List<Document> documents) {
            this.searchTerm = searchTerm;
            this.documents = documents;
        }

        public void search() {
            var watch = System.Diagnostics.Stopwatch.StartNew();
            
            List<Document> results = new List<Document>();
            foreach (var document in this.documents) {
                // Find matches.
                var match = Regex.IsMatch(document.Text, @"(?i)\b" + this.searchTerm + @"\b(?-i)");
                if (match) {
                    results.Add(document);
                }
            }

            watch.Stop();
            var elapsedMs = watch.ElapsedMilliseconds;
            Console.WriteLine($"Found {results.Count} results in {elapsedMs} ms");
        }
        
    }
}