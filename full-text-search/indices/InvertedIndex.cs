using System.Collections.Generic;

namespace full_text_search.indices {
    
    public class InvertedIndex<K, V> {

        public string Path { get; private set; }
        public string MD5 { get; private set; }
        public string Alias { get; private set; }
        private Dictionary<K, V> index;

        public InvertedIndex(string path, string md5, string alias = null) {
            this.Path = path;
            this.MD5 = md5;
            this.index = new Dictionary<K, V>();
            if (!string.IsNullOrWhiteSpace(alias)) {
                this.Alias = alias;
            }
        }

        public uint BuildIndex() {
            return 0;
        }

        public void search(K searchTerm) {
            
        }
    }
}