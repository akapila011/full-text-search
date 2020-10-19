using System;
using System.Text;
using full_text_search.cache;
using full_text_search.exceptions;
using full_text_search.indices;

namespace full_text_search.repl {
    public class ReplParser {

        public MemoryCache<string, InvertedIndex<string, string>> invertedIndexCache;

        public ReplParser(MemoryCache<string, InvertedIndex<string, string>> invertedIndexCache) {
            this.invertedIndexCache = invertedIndexCache;
        }

        public void execute(string line) {
            if (string.IsNullOrWhiteSpace(line)) {
                return;
            }

            var tokens = line.Split(" ");
            if (tokens.Length < 1) {
                throw new ArgumentException("No command provided");
            }

            var command = tokens[0].ToLower();
            switch (command) {
                case ReplCommandConstants.EXIT:
                    throw new ReplExitException();
                case ReplCommandConstants.HELP:
                    this.handleHelpCommand(tokens);
                    break;
                case ReplCommandConstants.LOAD:
                    this.handleLoadCommand(tokens);
                    break;
                case ReplCommandConstants.SEARCH:
                    this.handleSearchCommand(tokens);
                    break;
                
                default:
                    throw new ArgumentException($"Unknown command '{command}'");
            }
        }

        private void handleHelpCommand(string[] tokens) {
            var helpText = new StringBuilder("Full text search allows for indexing of directories and searching through " +
                                             "text documents within those directories. Usage:\n");
            helpText.Append(
                "\n-Load - allows for loading a directory with text files and indexes contents for search. " +
                "Use: '>> load path/to/dir'");
            helpText.Append(
                "\n-Search - searches a previously loaded & indexed directory for the search term. " +
                "Use: '>> search search_word path/to/dir'");
            helpText.Append(
                "\n-Exit - Exit this program. " +
                "Use: '>> exit'");
            Console.WriteLine(helpText);
        }
        private void handleLoadCommand(string[] tokens) {
            
        }
        
        private void handleSearchCommand(string[] tokens) {
            
        }
    }
}