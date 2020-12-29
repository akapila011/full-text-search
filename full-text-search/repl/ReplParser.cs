using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using full_text_search.cache;
using full_text_search.exceptions;
using full_text_search.indices;
using full_text_search.utilities;

namespace full_text_search.repl {
    public class ReplParser {

        public MemoryCache<string, InvertedIndex> invertedIndexCache;
        private string lastIndexedPath;

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
                case ReplCommandConstants.INDEX:
                    this.handleIndexCommand(tokens);
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
                "\n-Index - allows for loading a directory/file with files in order to index contents for searching. " +
                "Use: '>> load path/to/dir'");
            helpText.Append(
                "\n-Load - Load an already built index for searching. " +
                "Use: '>> load path/to/index.txt'");
            helpText.Append(
                "\n-Search - searches a previously loaded & indexed directory for the search term. " +
                "Uses last loaded/indexed directory/file. " +
                "Use: '>> search search_word");
            helpText.Append(
                "\n-Exit - Exit this program. " +
                "Use: '>> exit'");
            Console.WriteLine(helpText);
        }
        
        private void handleIndexCommand(string[] tokens) {
            var watch = System.Diagnostics.Stopwatch.StartNew();
            if (tokens.Length < 2) {
                throw new ArgumentException("No path provided for indexing.");
            }

            var path = tokens[1];
            var pathHash = HashUtilities.CreateMD5Hash(path);
            Console.WriteLine($"pathHash = {pathHash}");
            // if (!System.IO.Directory.Exists(directoryPath)) {
            //     throw new ArgumentException($"The path '{directoryPath}' does not exist.");
            // }
            Console.WriteLine($"Indexing path '{path}' started at {DateTime.Now}");
            var invertedIndex = new InvertedIndex(path, pathHash);
            invertedIndex.BuildIndex();

            this.invertedIndexCache.set(invertedIndex.MD5, invertedIndex);  // save to memory, force overwrite
            this.lastIndexedPath = invertedIndex.Path;

            var savePath = $"{pathHash}.txt";
            FileUtilities.SerializeInvertedIndex(savePath, invertedIndex);
            Console.WriteLine($"Index written to '{savePath}'");
            
            watch.Stop();
            var elapsedMs = watch.ElapsedMilliseconds;
            Console.WriteLine($"Indexed Path '{path}' in {elapsedMs} ms");
        }
        
        private void handleLoadCommand(string[] tokens) {  // TODO: tokens for force refresh (currently on), walking dir to child files
            var watch = System.Diagnostics.Stopwatch.StartNew();
            if (tokens.Length < 2) {
                throw new ArgumentException("No path provided for loading index.");
            }

            var indexPath = tokens[1].Replace("'", String.Empty).Replace("\"", String.Empty);
            if (!File.Exists(indexPath)) {
                throw new ArgumentException($"Index '{indexPath}' does not exist.");
            }

            var invertedIndex = FileUtilities.DeserializeInvertedIndex(indexPath);
            this.invertedIndexCache.set(invertedIndex.MD5, invertedIndex);  // save to memory, force overwrite
            this.lastIndexedPath = invertedIndex.Path;
            
            
            watch.Stop();
            var elapsedMs = watch.ElapsedMilliseconds;
            Console.WriteLine($"Loaded index '{indexPath}' in {elapsedMs} ms");
        }
        
        private void handleSearchCommand(string[] tokens) {
            var watch = System.Diagnostics.Stopwatch.StartNew();
            // TODO: currently expecting search a directory, should be possible to recursively index, save paths indexed and do a global search
            if (tokens.Length < 2) {
                throw new ArgumentException("No search term provided while searching");
            }

            var searchTerm = String.Join(" ", tokens, 1, tokens.Length - 1);
            Console.WriteLine($"Searching '{searchTerm}' in {this.lastIndexedPath}");
            // if (tokens.Length < 3) {
            //     throw new ArgumentException("No directory path provided for search");
            // }
            // var directoryPath = tokens[2];
            var directoryPathHash = HashUtilities.CreateMD5Hash(this.lastIndexedPath);

            var invertedIndex = this.invertedIndexCache.get(directoryPathHash);
            //Console.WriteLine($"directoryPathHash = {directoryPathHash}, invertedIndex != null {invertedIndex!=null} ");
            if (invertedIndex != null) {
                var results = invertedIndex?.search(searchTerm);
                var matchingDocuments = new List<string>(); // filename (token) - path
                foreach (var result in results) {
                    matchingDocuments.AddRange(result.getStringResult());
                }

                foreach (var matchingDocument in matchingDocuments) {
                    Console.WriteLine($" - {matchingDocument}");
                }
            }
            // TODO: load if not indexed (with option)
            watch.Stop();
            var elapsedMs = watch.ElapsedMilliseconds;
            Console.WriteLine($"Search '{lastIndexedPath}' completed in {elapsedMs} ms");
        }
    }
}