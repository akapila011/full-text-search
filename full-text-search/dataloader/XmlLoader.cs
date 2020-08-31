
using System;
using System.Collections.Generic;
using System.Linq;
using System.Xml;
using System.Xml.Linq;
using full_text_search.models;

namespace full_text_search.dataloader {
    public static class XmlLoader {

        public static List<Document> LoadDocumentsFromXmlList(string path, string nodeSelector) {
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
            Console.WriteLine($"Loaded {documents.Count} documents");
            return documents;
        }
    }
}