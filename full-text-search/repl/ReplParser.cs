using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using full_text_search.cache;
using full_text_search.configuration;
using full_text_search.exceptions;
using full_text_search.indices;
using full_text_search.utilities;

namespace full_text_search.repl {
    public class ReplParser {

        private Configuration configuration;
        private MemoryCache<string, InvertedIndex> invertedIndexCache;
        private FileUtilities fileUtilities;
        
        private string lastIndexedPath;

        public ReplParser(Configuration configuration,
            MemoryCache<string, InvertedIndex> invertedIndexCache,
            FileUtilities fileUtilities) {
            this.configuration = configuration;
            this.invertedIndexCache = invertedIndexCache;
            this.fileUtilities = fileUtilities;
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
            var helpText = new StringBuilder("Full text search allows for indexing of file and directories for faster searches in future." +
                                             " Usage:\n");
            helpText.Append(
                "\n-Index - allows for loading a directory/file with files in order to index contents for searching. " +
                "Use: '>> load path/to/dir_or_file'");
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

            var path = String.Join(" ", tokens, 1, tokens.Length - 1)
                .Replace("'", String.Empty).Replace("\"", String.Empty);
            var pathHash = HashUtilities.CreateMD5Hash(path);
            Console.WriteLine($"pathHash = {pathHash}");
            Console.WriteLine($"Indexing path '{path}' started at {DateTime.Now}");
            var invertedIndex = new InvertedIndex(path, pathHash);
            var filepaths = fileUtilities.GetIndexableFilePaths(path, this.configuration.AllowedExtensions);
            var indexBuildResult = invertedIndex.BuildIndex(filepaths, fileUtilities, stopWords: this.configuration.StopWords).Result;

            this.invertedIndexCache.set(invertedIndex.MD5, invertedIndex);  // save to memory, force overwrite
            this.lastIndexedPath = invertedIndex.Path;

            var savePath = $"{pathHash}.txt";
            this.fileUtilities.SerializeInvertedIndex(savePath, invertedIndex);
            Console.WriteLine($"Index written to '{savePath}'");
            
            watch.Stop();
            var elapsedMs = watch.ElapsedMilliseconds;
            Console.WriteLine($"Indexed Path '{path}' with {filepaths.Count} files in {elapsedMs} ms");
        }
        
        private void handleLoadCommand(string[] tokens) {  // TODO: tokens for force refresh (currently on), walking dir to child files
            var watch = System.Diagnostics.Stopwatch.StartNew();
            if (tokens.Length < 2) {
                throw new ArgumentException("No path provided for loading index.");
            }

            var indexPath = String.Join(" ", tokens, 1, tokens.Length - 1)
                .Replace("'", String.Empty).Replace("\"", String.Empty);
            if (!File.Exists(indexPath)) {
                throw new ArgumentException($"Index '{indexPath}' does not exist.");
            }

            var invertedIndex = this.fileUtilities.DeserializeInvertedIndex(indexPath);
            this.invertedIndexCache.set(invertedIndex.MD5, invertedIndex);  // save to memory, force overwrite
            this.lastIndexedPath = invertedIndex.Path;
            
            
            watch.Stop();
            var elapsedMs = watch.ElapsedMilliseconds;
            Console.WriteLine($"Loaded index '{indexPath}' last indexed at {invertedIndex.IndexedTime} in {elapsedMs} ms");
        }
        
        private void handleSearchCommand(string[] tokens) {
            var watch = System.Diagnostics.Stopwatch.StartNew();
            if (tokens.Length < 2) {
                throw new ArgumentException("No search term provided while searching");
            }
            if (string.IsNullOrEmpty(this.lastIndexedPath)) {
                throw new ArgumentException("No load index. Try use the Index command or Load command before searching.");
            }

            var searchTerm = String.Join(" ", tokens, 1, tokens.Length - 1);
            Console.WriteLine($"Searching '{searchTerm}' in {this.lastIndexedPath}");
            var directoryPathHash = HashUtilities.CreateMD5Hash(this.lastIndexedPath);

            var invertedIndex = this.invertedIndexCache.get(directoryPathHash);
            if (invertedIndex != null) {
                var results = invertedIndex?.Search(searchTerm);
                foreach (var result in results) {
                    Console.WriteLine($" - {result}");
                }
            }
            
            watch.Stop();
            var elapsedMs = watch.ElapsedMilliseconds;
            Console.WriteLine($"Search '{lastIndexedPath}' (last indexed at {invertedIndex?.IndexedTime}) completed in {elapsedMs} ms");
        }
    }
}