using System;
using System.Collections.Generic;
using System.Text;
using full_text_search.cache;
using full_text_search.exceptions;
using full_text_search.indices;
using full_text_search.utilities;

namespace full_text_search.repl {
    public class ReplParser {

        public MemoryCache<string, InvertedIndex> invertedIndexCache;

        public ReplParser(MemoryCache<string, InvertedIndex> invertedIndexCache) {
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
        private void handleLoadCommand(string[] tokens) {  // TODO: tokens for force refresh (currently on), walking dir to child files
            var watch = System.Diagnostics.Stopwatch.StartNew();
            if (tokens.Length < 2) {
                throw new ArgumentException("No path provided for directory indexing.");
            }

            var directoryPath = tokens[1];
            var directoryPathHash = HashUtilities.CreateMD5Hash(directoryPath);
            Console.WriteLine($"directoryPathHash = {directoryPathHash}");
            // if (!System.IO.Directory.Exists(directoryPath)) {
            //     throw new ArgumentException($"The path '{directoryPath}' does not exist.");
            // }
            Console.WriteLine($"Indexing directory '{directoryPath}' started at {DateTime.Now}");
            var invertedIndex = new InvertedIndex(directoryPath, directoryPathHash);
            invertedIndex.BuildIndex();

            this.invertedIndexCache.set(directoryPathHash, invertedIndex);  // save to memory, force overwrite
            
            watch.Stop();
            var elapsedMs = watch.ElapsedMilliseconds;
            Console.WriteLine($"Indexed directory '{directoryPath}' in {elapsedMs} ms");
        }
        
        private void handleSearchCommand(string[] tokens) {
            var watch = System.Diagnostics.Stopwatch.StartNew();
            // TODO: currently expecting search a directory, should be possible to recursively index, save paths indexed and do a global search
            if (tokens.Length < 2) {
                throw new ArgumentException("No search term provided while searching");
            }

            var searchTerm = tokens[1];  // TODO: will not work for multiple word searches
            if (tokens.Length < 3) {
                throw new ArgumentException("No directory path provided for search");
            }
            var directoryPath = tokens[2];
            var directoryPathHash = HashUtilities.CreateMD5Hash(directoryPath);

            var invertedIndex = this.invertedIndexCache.get(directoryPathHash);
            Console.WriteLine($"directoryPathHash = {directoryPathHash}, invertedIndex != null {invertedIndex!=null} ");
            if (invertedIndex != null) {
                invertedIndex?.search(searchTerm);
            }
            // TODO: load if not indexed (with option)
            watch.Stop();
            var elapsedMs = watch.ElapsedMilliseconds;
            Console.WriteLine($"Search '{directoryPath}' completed in {elapsedMs} ms");
        }
    }
}