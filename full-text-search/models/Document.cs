using System;

namespace full_text_search.models {
    
    [Serializable()]
    public class Document {
        
        [System.Xml.Serialization.XmlElement("title")]
        public string Title { get; set; }

        [System.Xml.Serialization.XmlElement("url")]
        public string URL { get; set; }

        [System.Xml.Serialization.XmlElement("abstract")]
        public string Text { get; set; }
        
        public int ID { get; set; }

        public Document(string title, string url, string text) {
            Title = title;
            URL = url;
            Text = text;
        }
    }
}