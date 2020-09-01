using System;
using System.Collections.Generic;
using full_text_search.dataloader;
using full_text_search.models;
using full_text_search.search;

namespace full_text_search 
{
    class Program {
        private const int NoOfArgs = 2;
        
        static void Main(string[] args) {
            if (args.Length != NoOfArgs) {
                throw new ArgumentException($"Program must be started with {NoOfArgs} arguments.");
            }

            string searchTerm = args[0];
            string searchPath = args[1];
            Console.WriteLine($"SearchTerm={searchTerm}, SearchPath={searchPath}");
            
            List<Document> documents = XmlLoader.LoadDocumentsFromXmlList(searchPath, "doc");
            
            TextSearch textSearch = new TextSearch(searchTerm, documents);
            textSearch.search();
        }
    }
}