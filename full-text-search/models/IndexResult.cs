using System.Collections.Generic;

namespace full_text_search.models {
    public class IndexResult {

        private string token;
        private List<string> ids;

        public IndexResult(string token, List<string> ids) {
            this.token = token;
            this.ids = ids;
        }

        public IList<string> getStringResult() {
            var stringResults = new List<string>(); // filename (token) @ path
            foreach (var id in this.ids) {
                var filename = System.IO.Path.GetFileName(id);
                stringResults.Add($"{filename} ({this.token}) @ {id}");
            }
            return stringResults;
        }

        public override string ToString() {
            var result = new System.Text.StringBuilder("------------------------------------");
            result.Append($"Token {this.token} found {this.ids.Count} in files");
            foreach (var id in this.ids) {
                result.Append($" - {id}");
            }

            result.Append("------------------------------------");
            return result.ToString();
        }
    }
}