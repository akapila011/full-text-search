# Full Text Search

Full Text Search is a console application allowing users to build an index of files read
which can then later be used for search as needed for faster searches and easier loading of index. 

![Demo Gif](git-resources/full-text-search-demo.gif?raw=true "Full Text Search Demo")

### How to Use
*Ensure the appsettings.json is in the working directory & has valid values (as per what has been committed)*.
To use the app build the project and call the dll to get to the REPL window.
In the REPLY window you can use 3 main commands:
 - **INDEX** - given a path to a directory or file index files with the valid extension (as per config AllowedExtensions in appsettings.json).
 This will build the index, save the index to working directory with and load it for search.
  example **index C:\Users\User\Documents**
 - **LOAD** - given a path to a generated index file, load the index for searching.
  example **load C:\Users\User\indices\2E0290919974107BABFD9B2439B81AC9.txt**
 - **SEARCH** - you need to run either INDEX/LOAD commands before running search. 
 Then you can use search to pass whatever you want fo find and it will search through the index.
 example **search my query sentence**
 
 
#### TODO
 - Add a way to read in config file from a specified path on start up
 - Consider using a cache for repeated queries (already 1 cache holding the index).
 - Improve the logic do decide on whether to compress index. Currently tries to use 
  repeated ids(/paths) vs non repeated ids to determine whether to create a mapping.
  In future we need a way to count the repeated text length vs what it would be only 
  using number ids + the mapping table. We are using number to map id's/paths so seems 
  it will always be smaller.
 - Handle spaces in Index/Load commands. Currently cannot work with spaces. Need to parse
  text based of everything after the command.
 - Use a logger (not sure if log file/just console).
 - use async/await. Especially in indexing as it will be very useful for handling
  multiple files indexing, hashing etc
 - Possible a new command is needed (e.g. VERIFY/CHANGECHECK/DETECTCHANGE) to check if the path indexed has changed. Already
  have a checksum of path indexed including contents+filepath. How this verify command
  will be used needs to be thought out. It could just be a part of the search.
 - Always room for improvement in how to parse different file formats e.g. json (ignore {} chars) vs xml (ignore <> chars)
 
 To contribute you can fork this repository, branch off master (with a descriptive feature branch name e.g. handling parsing json files during index build), make your changes, then submit a PR.

This project is based of learning found in the references section. It is mainly a 
proof of concept to get familiar with indexing concepts to use in future projects as needed.

References

- https://artem.krylysov.com/blog/2020/07/28/lets-build-a-full-text-search-engine/