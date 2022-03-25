using System;
using System.Collections.Generic;
using System.Text;
using System.IO;
using ICSharpCode.SharpZipLib.Zip;

namespace bmx2escn {
    public class BmxZipUtils {

        private static readonly string TEMP_GUID = "7608400f30784c61b79f271fef797ad1";

        public static string DecompressBmxToTemp(string bmxFilePath) {
            // generate guid folder and preparing it in file system
            var guidFolder = Path.Combine(System.IO.Path.GetTempPath(), TEMP_GUID);
            if (Directory.Exists(guidFolder))
                Directory.Delete(guidFolder, true);
            Directory.CreateDirectory(guidFolder);

            // use UTF-8 filename
            ZipStrings.CodePage = 65001;    

            using (ZipInputStream s = new ZipInputStream(new FileStream(bmxFilePath, FileMode.Open, FileAccess.Read, FileShare.Read))) {
                ZipEntry theEntry;
                while ((theEntry = s.GetNextEntry()) != null) {
                    Console.WriteLine($"Decompressing {theEntry.Name}...");
                    
                    string directoryName = Path.GetDirectoryName(theEntry.Name);
                    string fileName = Path.GetFileName(theEntry.Name);

                    // create directory
                    if (directoryName != string.Empty) {
                        Directory.CreateDirectory(Path.Combine(guidFolder, directoryName));
                    }

                    if (fileName != string.Empty) {
                        using (FileStream streamWriter = new FileStream(
                            Path.Combine(guidFolder, theEntry.Name),
                            FileMode.Create, FileAccess.Write, FileShare.None)) {

                            int size = 2048;
                            byte[] data = new byte[2048];
                            while (true) {
                                size = s.Read(data, 0, data.Length);
                                if (size > 0) {
                                    streamWriter.Write(data, 0, size);
                                } else {
                                    break;
                                }
                            }
                        }
                    }
                }
            }

            return guidFolder;
        }

        public static void CleanTempFolder() {
            var guidFolder = Path.Join(System.IO.Path.GetTempPath(), TEMP_GUID);

            if (Directory.Exists(guidFolder))
                Directory.Delete(guidFolder, true);
        }

    }
}
