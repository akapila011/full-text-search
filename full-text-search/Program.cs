using System;
using System.Collections.Generic;
using full_text_search.cache;
using full_text_search.dataloader;
using full_text_search.exceptions;
using full_text_search.indices;
using full_text_search.models;
using full_text_search.repl;
using full_text_search.search;

namespace full_text_search 
{
    class Program {
        private const int NoOfArgs = 2;

        public MemoryCache<string, InvertedIndex> InvertedIndexCache { get; private set; } 
        public Program() {
            this.InitializeDependencies();
        }

        private void InitializeDependencies() {
            this.InvertedIndexCache = new MemoryCache<string, InvertedIndex>(1000, "InvertedIndexCache");  // TODO" see how to load from config files
        }
        
        static void Main(string[] args) {
            Program app = new Program(); // TODO: read configs and load dir with saved indexes
            var replParser = new ReplParser(app.InvertedIndexCache);
            
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

            if (args.Length != NoOfArgs) {
                throw new ArgumentException($"Program must be started with {NoOfArgs} arguments.");
            }
            // string searchTerm = args[0];
            // string searchPath = args[1];
            // Console.WriteLine($"SearchTerm={searchTerm}, SearchPath={searchPath}");
            //
            // List<Document> documents = XmlLoader.LoadDocumentsFromXmlList(searchPath, "doc");
            //
            // TextSearch textSearch = new TextSearch(searchTerm, documents);
            // textSearch.search();
        }
    }
}