using System;
using System.Collections.Generic;

namespace full_text_search.models {
    public class SearchResult {
        public string DisplayName { get; private set; }
        public List<string> MatchingTokens { get; private set; }
        public string MatchingTokensCsv { get; private set; }
        public string ID { get; private set; }

        public SearchResult(string displayName, List<string> matchingTokens, string id) {
            this.DisplayName = displayName;
            this.MatchingTokens = matchingTokens;
            this.MatchingTokensCsv = String.Join(",", this.MatchingTokens);
            this.ID = id;
        }

        public override string ToString() {
            return $"{this.DisplayName} ({this.MatchingTokensCsv}) @ {this.ID}";
        }
    }
}