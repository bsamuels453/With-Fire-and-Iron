#region

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

#endregion

namespace Forge.Framework.Resources{
    internal abstract class ResourceLoader : IDisposable{
        const int _numEstimatedDirectories = 20;
        const int _numEstimatedFiles = 50;

        #region IDisposable Members

        public abstract void Dispose();

        #endregion

        /// <summary>
        ///   Retrieves all files within a given directly, including the files within each subfolder.
        /// </summary>
        /// <param name="directory"> The binary-relative directory in which to search for files. Do not surround the directory name with escape characters. </param>
        /// <returns> </returns>
        protected List<FileAttributes> GetAllFilesInDirectory(string directory){
            var rawDir = (string) directory.Clone();
            directory = "\\" + directory + "\\";

            var directoriesToSearchForFiles = new List<string>();
            var directoriesToSearchForDirs = new Queue<string>(_numEstimatedDirectories);

            directoriesToSearchForDirs.Enqueue(Directory.GetCurrentDirectory() + directory);

            while (directoriesToSearchForDirs.Count > 0){
                var curDir = directoriesToSearchForDirs.Dequeue();
                directoriesToSearchForFiles.Add(curDir);

                var newDirs = Directory.GetDirectories(curDir);
                foreach (var dir in newDirs){
                    directoriesToSearchForDirs.Enqueue(dir);
                }
            }

            var files = new List<FileAttributes>(_numEstimatedFiles);

            foreach (var dir in directoriesToSearchForFiles){
                var dirFiles = Directory.GetFiles(dir);
                foreach (var file in dirFiles){
                    int extensionSepIdx = -1;
                    for (int i = file.Length - 1; i >= 0; i--){
                        if (file[i] == '.'){
                            extensionSepIdx = i;
                        }
                    }
                    string extension = extensionSepIdx == -1 ? "" : file.Substring(extensionSepIdx);

                    var splitByFolder = file.Split('\\').ToList();

                    string fileName = splitByFolder.Last();
                    string fullLocation = file;


                    var splitRawDir = rawDir.Split('\\');//make sure the split doesnt search for multiple nested files
                    int baseFolderIdx = splitByFolder.IndexOf(splitRawDir[0]);

                    var relativeDirArr = splitByFolder.GetRange(baseFolderIdx, splitByFolder.Count - baseFolderIdx);
                    string relativeLocation = "";
                    foreach (var name in relativeDirArr){
                        relativeLocation += "\\" + name;
                    }

                    files.Add
                        (new FileAttributes
                            (
                            fileName,
                            extension,
                            fullLocation,
                            relativeLocation
                            ));
                }
            }

            files.TrimExcess();
            return files;
        }

        #region Nested type: FileAttributes

        protected struct FileAttributes{
            /// <summary>
            ///   The extension of the file.
            /// </summary>
            public readonly string Extension;

            /// <summary>
            ///   The name of the file, including extension.
            /// </summary>
            public readonly string Filename;

            /// <summary>
            ///   The full blown file location.
            /// </summary>
            public readonly string FullFileLocation;

            public readonly string RelativeFileLocation;

            public FileAttributes(string filename, string extension, string fullFileLocation, string relativeFileLocation){
                Filename = filename;
                Extension = extension;
                FullFileLocation = fullFileLocation;
                RelativeFileLocation = relativeFileLocation;
            }
        }

        #endregion
    }
}