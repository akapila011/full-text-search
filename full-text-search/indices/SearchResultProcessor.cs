
using System;
using System.Collections.Generic;
using System.Linq;
using full_text_search.models;

namespace full_text_search.indices {
    
    public class SearchResultProcessor {

        public static IList<SearchResult> ProcessResults(IList<IndexResult> indexResults) {
            var searchResults = new List<SearchResult>();
            var mappedResults = new Dictionary<string, ISet<string>>();  // mapping path/id to tokens
            var mappedResultsOrder = new List<string>();  // list of id's/paths in mappedResults used in order

            foreach (var indexResult in indexResults) {
                foreach (var id in indexResult.ids) {
                    if (!mappedResults.ContainsKey(id)) {
                        mappedResults.Add(id, new HashSet<string>());
                        mappedResultsOrder.Add(id);
                    }
                    mappedResults[id].Add(indexResult.token);
                }
            }

            // TODO: order by mappedResults.values.count
            foreach (var id in mappedResultsOrder) {
                var filename = System.IO.Path.GetFileName(id);
                var searchResult = new SearchResult(filename, mappedResults[id].ToList(), id);
                searchResults.Add(searchResult);
            }

            return searchResults;
        }   
    }
}