using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;

namespace full_text_search.models {

    public class BasicFileDocument : IIndexDocument {
        
        public string Title { get; set; }

        public string Path { get; set; }

        public string ID { get; set; }

        public BasicFileDocument(string id, string title, string path) {
            ID = id;
            Title = title;
            Path = path;
        }

        public IEnumerable<string> LoadFileLine() {
            foreach(string line in File.ReadLines(this.Path))
            {
                yield return line;
            }
            yield break;
        }
    }
}