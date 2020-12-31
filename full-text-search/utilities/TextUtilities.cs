
using System;
using System.Collections.Generic;
using System.Linq;

namespace full_text_search.utilities {
    
    public static class TextUtilities {

        public static string[] tokenize(string text) {  // TODO: need to remvoe any char that is not letter or number to get actual words
            var tokens = text.Trim('\n', '\r').Split(' ');
                //.Select(x => new string(x.Where(Char.IsLetterOrDigit).ToArray()));
            return tokens;
        }

        public static string[] lowerCaseTokens(string[] tokens) {
            return tokens.Select(s => s.ToLowerInvariant()).ToArray();
        }
        
        public static string[] cleanTokensOfSpecialChars(string[] tokens) {
            for (int i = 0; i < tokens.Length; i++) {
                tokens[i] = tokens[i]
                    .Replace("'", String.Empty)
                    .Replace("\"", String.Empty)
                    .Replace(".", String.Empty)
                    .Replace(",", String.Empty)
                    .Replace("!", String.Empty)
                    .Replace("?", String.Empty)
                    .Replace(";", String.Empty);
            }
            return tokens;
        }
        
        public static string[] removeTokens(string[] tokens, ISet<string> tokensToRemove) {
            return tokens.Except(tokensToRemove).ToArray();
        }
    }
}