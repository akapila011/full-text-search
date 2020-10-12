
using System;
using System.Collections.Generic;
using System.Linq;
using System.Xml;
using System.Xml.Linq;
using full_text_search.models;

namespace full_text_search.dataloader {
    public static class XmlLoader {

        public static List<Document> LoadDocumentsFromXmlList(string path, string nodeSelector) { // TODO: use generators
            var watch = System.Diagnostics.Stopwatch.StartNew();

            List<Document>  documents = XDocument
                .Load(path)
                .Root
                .Elements(nodeSelector)
                .Select(p => new Document(
                    (string) p.Element("title"),
                    (string) p.Element("url"),
                    (string) p.Element("abstract")
                ))
                .ToList();

            // set ids
            for (int i = 0; i < documents.Count; i++) {
                documents[i].ID = i;
            }
            
            watch.Stop();
            var elapsedMs = watch.ElapsedMilliseconds;
            Console.WriteLine($"Loaded {documents.Count} documents in {elapsedMs} ms");
            return documents;
        }
    }
}