using System;
using System.Collections.Generic;
using full_text_search.dataloader;
using full_text_search.exceptions;
using full_text_search.models;
using full_text_search.repl;
using full_text_search.search;

namespace full_text_search 
{
    class Program {
        private const int NoOfArgs = 2;
        
        static void Main(string[] args) {
            var replParser = new ReplParser();
            Console.WriteLine("Started Full Text Search. Enter 'help' for command usage, 'exit' to finish.");
            string userInput = "";
            do {
                Console.WriteLine(ReplCommandConstants.USER_COMMAND_PROMPT);
                userInput = Console.ReadLine();
                try {
                    replParser.execute(userInput);
                }
                catch (ArgumentException ex) {
                    Console.WriteLine("Invalid command:\n" + ex.Message + "\nPlease type 'help' for correct usage.");
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