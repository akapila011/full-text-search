using System;
using System.Collections.Generic;
using System.Linq;
using full_text_search.models;
using full_text_search.utilities;

namespace full_text_search.indices {
    
    [Serializable]
    public class InvertedIndex {

        private const uint serializeVersion = 5;
        
        public string Path { get; private set; }  // directory path for now, allow file path and extensions parsing options later
        public string MD5 { get; private set; }
        public string Alias { get; private set; }
        public DateTime IndexedTime { get; private set; }
        private bool compressed;
        private string checksum;
        private IDictionary<string, HashSet<string>> index; // token: [document id's]
        private IDictionary<string, string> idMapping;  // document id: less space consuming value like hash

        public InvertedIndex(string path, string md5, string alias = null) {
            this.Path = path;
            this.MD5 = md5;
            this.index = new Dictionary<string, HashSet<string>>();
            this.idMapping = new Dictionary<string, string>();
            if (!string.IsNullOrWhiteSpace(alias)) {
                this.Alias = alias;
            }

            this.compressed = false;  // default to false
        }

        public override string ToString() {
            return $"InvertedIndex(Path={this.Path},MD5={this.MD5},Alias={this.Alias},IndexedTime={IndexedTime}," +
                   $"indexCount={this.index.Count},checksum={this.checksum},compressed={this.compressed})";
        }

        public int BuildIndex(IList<(string filepath, bool indexContent)> filepaths, FileUtilities fileUtilities, 
            ISet<string> stopWords = null) {
            var watch = System.Diagnostics.Stopwatch.StartNew();

            if (filepaths.Count == 0) {
                Console.WriteLine($"No index built for '{this.Path}', no files found.");
            }
            
            var indexContentCount = 0;
            this.index.Clear();
            this.idMapping.Clear();
            long noOfIds = 0;
            long lengthOfIds = 0;
            foreach (var row in filepaths) { // NOTE: filepaths order of listing might be important for checksum, test before using a different data structure
                var filepath = row.filepath;
                var indexContent = row.indexContent;
                var filename = System.IO.Path.GetFileName(filepath);
                var id = filepath;
                noOfIds++;
                
                if (indexContent) {
                    indexContentCount++;
                    var document = new BasicFileDocument(id, filename, id);
                    foreach (var line in document.LoadFileLine()) {
                        this.indexTextLine(line, id, ref lengthOfIds, stopWords);
                    }
                }
                // index filename
                var filenameParts = filename.Split(".");
                var filenameWithoutExtension = String.Join(" ", filenameParts, 0, filenameParts.Length - 1);
                this.indexTextLine(filenameWithoutExtension, id, ref lengthOfIds, stopWords);
                // checksum contents
                var contentHash = fileUtilities.HashFileContentMd5(filepath);
                var pathHash = HashUtilities.CreateMD5Hash(filepath);
                var combinedHash = HashUtilities.CreateMD5Hash(contentHash + pathHash);
                this.checksum = HashUtilities.CreateMD5Hash(this.checksum + combinedHash); 
            }

            var compressed = setIndexCompression(noOfIds, lengthOfIds);
            Console.WriteLine($"Index compressed on build {compressed}");
            this.IndexedTime = DateTime.Now;
            
            watch.Stop();
            var elapsedMs = watch.ElapsedMilliseconds;
            Console.WriteLine($"Built index for '{this.Path}', {this.index.Count} indexed words " +
                              $"(filesIndexed={indexContentCount}/{filepaths.Count}) in {elapsedMs} ms");
            return this.index.Count;
        }

        private void indexTextLine(string text, string id, ref long lengthOfIds, ISet<string> stopWords = null) {
            var tokens = TextUtilities.tokenize(text);
            tokens = TextUtilities.lowerCaseTokens(tokens);
            tokens = TextUtilities.cleanTokensOfSpecialChars(tokens);
            if (stopWords != null) {
                tokens = TextUtilities.removeTokens(tokens, stopWords);
            }
            foreach (var token in tokens) {
                if (!this.index.ContainsKey(token)) {
                    this.index[token] = new HashSet<string>();
                }
                this.index[token].Add(id);
                lengthOfIds += id.Length;
            }
        }

        public IList<SearchResult> Search(string searchTerm) {
            var watch = System.Diagnostics.Stopwatch.StartNew();
            var results = new List<IndexResult>();
            
            HashSet<string> resultDocumentIds = new HashSet<string>();
            var searchTokens = TextUtilities.tokenize(searchTerm);
            searchTokens = TextUtilities.lowerCaseTokens(searchTokens);
            foreach (var searchToken in searchTokens) {
                if (this.index.TryGetValue(searchToken, out var extractedDocumentIds)) {
                    var documentIds = this.compressed
                        ? this.extractCompressedDocumentIds(extractedDocumentIds)
                        : extractedDocumentIds;
                    resultDocumentIds.UnionWith(documentIds);
                    results.Add(new IndexResult(searchToken, documentIds.ToList()));
                }
            }
            watch.Stop();
            var elapsedMs = watch.ElapsedMilliseconds;
            Console.WriteLine($"Search for '{searchTerm}' returned {resultDocumentIds.Count} results {elapsedMs} ms");
            return SearchResultProcessor.ProcessResults(results);
        }
        
        /// <summary>
        /// When building an index, we need to determine if ids/paths per token are repeated
        /// enough to use a mapping dictionary for compression.
        /// The logic is based on length of id's as is vs length of id's compressed + mapping table
        /// as this is ensures the data in memory/disk is truly less.
        /// </summary>
        /// <param name="noOfIds">no of ids including duplicates used throughout the index</param>
        /// <param name="lengthOfIds">sum of lengths of each id in noOfIds</param>
        /// <returns></returns>
        private bool setIndexCompression(long noOfIds, long lengthOfIds) {
            long compressedIdsSize = 0;
            for (int i = 0; i < noOfIds; i++) {
                compressedIdsSize += i.ToString().Length;
            }

            Console.WriteLine($"setIndexCompression noOfIds={noOfIds} lengthOfIds={lengthOfIds} " +
                              $"compressedIdsSize={compressedIdsSize}");
            
            // Now determine if we should compress
            if (lengthOfIds > compressedIdsSize) {
                this.compressed = true;
            }
            else {
                this.compressed = false;
                return this.compressed;
            }
            
            // So if we need to compress we need to update the index with hash values
            var idNo = 0;  // counter to map a id/path to a less spacing consuming long number
            var idToIdNoMapping = new Dictionary<string, string>();
            foreach (var entry in this.index) {
                var ids = entry.Value;
                var compressedIds = new HashSet<string>(ids.Count);
                foreach (var id in ids) {
                    var idNoToUse = idNo.ToString();
                    if (idToIdNoMapping.ContainsKey(id)) {
                        idNoToUse = idToIdNoMapping[id];
                    }
                    else {
                        idToIdNoMapping[id] = idNoToUse;
                        idNo++;
                    }
                    this.idMapping[idNoToUse] = id;
                    compressedIds.Add(idNoToUse);
                }
                this.index[entry.Key].Clear();
                this.index[entry.Key].UnionWith(compressedIds);
            }

            return this.compressed;
        }

        private ISet<string> extractCompressedDocumentIds(ISet<string> extractedIds) {
            var documentIds = new HashSet<string>();
            foreach (var documentCompressedId in extractedIds) {
                var documentId = this.idMapping[documentCompressedId];
                documentIds.Add(documentId);
            }

            return documentIds;
        }
    }
}