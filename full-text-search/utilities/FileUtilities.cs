
using System;
using System.Collections.Generic;
using System.IO;
using System.Runtime.Serialization;
using System.Runtime.Serialization.Formatters.Binary;
using full_text_search.indices;

namespace full_text_search.utilities {

    public class FileUtilities {
        
        /// <summary>
        /// Give path ro dir or to file return a list of file paths
        /// that have files with the allowed extensions
        /// </summary>
        /// <param name="path">path to dir/file</param>
        /// <param name="allowedExtensions">e.g. txt,csv,json</param>
        /// <returns></returns>
        public IList<string> GetIndexableFilePaths(string path, IList<string> allowedExtensions) {
            IList<string> validFilePaths = new List<string>();
            FileAttributes attr = File.GetAttributes(path);
            if (attr.HasFlag(FileAttributes.Directory)) {
                validFilePaths = this.GetValidFilePaths(path, allowedExtensions);
            }
            else {
                for (int i=0; i<allowedExtensions.Count; i++) {
                    var extension = allowedExtensions[i];
                    if (path.EndsWith(extension)) {
                        validFilePaths.Add(path);
                        i = allowedExtensions.Count; // don't continue checking other extensions for a path
                    }
                }
            }

            return validFilePaths;
        }
        
        /// <summary>
        /// Given a directory recursively find file with a valid extension
        /// </summary>
        /// <param name="directoryPath">e.g C:\Users</param>
        /// <param name="allowedExtensions">e.g txt,csv</param>
        private IList<string> GetValidFilePaths(string directoryPath, IList<string> allowedExtensions)
        {
            var validFilePaths = new List<string>();
            try
            {
                foreach (string f in Directory.GetFiles(directoryPath))
                {
                    for (int i=0; i<allowedExtensions.Count; i++) {
                        var extension = allowedExtensions[i];
                        if (f.EndsWith(extension)) {
                            validFilePaths.Add(f);
                            i = allowedExtensions.Count; // don't continue checking other extensions for a path
                        }
                    }
                }

                foreach (string d in Directory.GetDirectories(directoryPath))
                {
                    validFilePaths.AddRange(GetValidFilePaths(d, allowedExtensions));
                }
            }
            catch (Exception exception)
            {
                Console.WriteLine(exception.Message);
            }

            return validFilePaths;
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