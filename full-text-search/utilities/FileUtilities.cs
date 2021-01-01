
using System;
using System.Collections.Generic;
using System.IO;
using System.Runtime.Serialization;
using System.Runtime.Serialization.Formatters.Binary;
using full_text_search.indices;

namespace full_text_search.utilities {

    public class FileUtilities {
        
        /// <summary>
        /// Give path to dir or to file return a list of file paths
        /// recursive within the dir and the flag on whether contents need to be indexed
        /// </summary>
        /// <param name="path">path to dir/file</param>
        /// <param name="allowedExtensions">e.g. txt,csv,json</param>
        /// <returns>tuple of filepaths to read contents for indexing (with flag to indexContent when extension is correct)</returns>
        public IList<(string filepath, bool indexContent)> GetIndexableFilePaths(string path, IList<string> allowedExtensions) {
            IList<(string filepath, bool indexContent)> filepaths = new List<(string filepath, bool indexContent)>();
            
            FileAttributes attr = File.GetAttributes(path);
            if (attr.HasFlag(FileAttributes.Directory)) {
                filepaths = this.GetValidFilePaths(path, allowedExtensions);
            }
            else {
                var filepathsIndex = -1;
                this.RecordFilepathAndCheckIndexContent(path, ref filepaths, ref filepathsIndex, allowedExtensions);
            }

            return filepaths;
        }
        
        /// <summary>
        /// Given a directory recursively list all filepaths and valid for files with
        /// an allowed extension indicating contents can be indexed
        /// </summary>
        /// <param name="directoryPath">e.g C:\Users</param>
        /// <param name="allowedExtensions">e.g txt,csv</param>
        private IList<(string filepath, bool indexContent)> GetValidFilePaths(string directoryPath, IList<string> allowedExtensions)
        {
            IList<(string filepath, bool indexContent)> filepaths = new List<(string filepath, bool indexContent)>();
            var filepathsIndex = -1;
            try
            {
                foreach (string filepath in Directory.GetFiles(directoryPath)) {
                    this.RecordFilepathAndCheckIndexContent(filepath, ref filepaths, ref filepathsIndex, allowedExtensions);
                }
                foreach (string d in Directory.GetDirectories(directoryPath))
                {
                    ((List<(string filepath, bool indexContent)>)filepaths).AddRange(GetValidFilePaths(d, allowedExtensions));
                }
            }
            catch (Exception exception)
            {
                Console.WriteLine(exception.Message);
            }

            return filepaths;
        }

        private void RecordFilepathAndCheckIndexContent(
            string filepath,
            ref IList<(string filepath, bool indexContent)> filepaths,
            ref int filepathsIndex,
            IList<string> allowedExtensions) {

            filepaths.Add((filepath, false));
            filepathsIndex++;
            for (int i=0; i<allowedExtensions.Count; i++) {
                var extension = allowedExtensions[i];
                if (filepath.EndsWith(extension)) {
                    filepaths[filepathsIndex] = (filepath, true);
                    i = allowedExtensions.Count; // don't continue checking other extensions for a path
                }
            }
        }

        public string SerializeInvertedIndex(string savePath, InvertedIndex index) {
            Console.WriteLine($"Serializing: {index}");
            IFormatter formatter = new BinaryFormatter();
            FileStream stream = null;
            try {
                stream = new FileStream(savePath, FileMode.Create, FileAccess.Write);
                formatter.Serialize(stream, index);
            }
            catch (Exception ex) {
                Console.WriteLine(ex.Message);
                return null;
            }
            finally {
                if (stream != null) {
                    stream.Close();
                }
            }

            return savePath;
        }
        
        public InvertedIndex DeserializeInvertedIndex(string indexPath) {
            InvertedIndex index = null;
            IFormatter formatter = new BinaryFormatter();
            FileStream stream = null;
            try {
                stream = new FileStream(indexPath, FileMode.Open, FileAccess.Read);
                index = (InvertedIndex) formatter.Deserialize(stream);
                Console.WriteLine($"Deserialized: {index}");
            }
            catch (Exception ex) {
                Console.WriteLine(ex.Message);
                return index;
            }
            finally {
                if (stream != null) {
                    stream.Close();
                }
            }

            return index;
        } 
        
    }
}