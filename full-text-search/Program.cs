using System;
using System.Collections.Generic;
using full_text_search.cache;
using full_text_search.configuration;
using full_text_search.dataloader;
using full_text_search.exceptions;
using full_text_search.indices;
using full_text_search.models;
using full_text_search.repl;
using full_text_search.utilities;
using Microsoft.Extensions.DependencyInjection;

namespace full_text_search 
{
    class Program {
        private const int NoOfArgs = 2;

        public Configuration Configuration { get; private set; } 
        public MemoryCache<string, InvertedIndex> InvertedIndexCache { get; private set; } 
        public FileUtilities FileUtil { get; private set; } 
        
        public Program() {
            this.InitializeDependencies();
        }

        private void InitializeDependencies() {
            this.Configuration = new Configuration();
            this.InvertedIndexCache = new MemoryCache<string, InvertedIndex>
                (this.Configuration.InvertedIndexCacheSize, "InvertedIndexCache"); 
            this.FileUtil = new FileUtilities();
        }

        static void Main(string[] args) {
            Program app = new Program();
            var replParser = new ReplParser(app.Configuration, app.InvertedIndexCache, app.FileUtil);
            
            Console.WriteLine($"Started Full Text Search. Enter '{ReplCommandConstants.HELP}' for command usage, '{ReplCommandConstants.EXIT}' to finish.");
            string userInput = "";
            do {
                Console.WriteLine(ReplCommandConstants.USER_COMMAND_PROMPT);
                userInput = Console.ReadLine();
                try {
                    replParser.execute(userInput);
                }
                catch (ArgumentException ex) {
                    Console.WriteLine($"Invalid command:\n{ex.Message}\nPlease type '{ReplCommandConstants.HELP}' for correct usage.");
                }
                catch (ReplExitException) {
                    break;
                }
            } while (true);
        }
    }
}