using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Compression;
using System.IO.Pipes;
using System.Runtime.InteropServices;

namespace AbletonProjectSearch
{
    class Program
    {
        static void Main(string[] args)
        {
            bool verbose = false;
            List<string> MatchingProjects = new List<string>();

            if (args.Length <2 || args.Length > 3)
            {
                Console.WriteLine("AbletonProjectSearch is a helper tool to search for a certain sample name or plugin name to find out which projekts are using this plugin. The result shall help to find out if a sanple or plugin can be removed without any concerns it may be used in a project.");
                Console.WriteLine("Usage: AbletonProjectSearch [PathToProjects] [SearchPattern] (optional)[verbose]");
            }
            else
            {
                if(!Directory.Exists(args[0])) Console.WriteLine($"Path {args[0]} not found.");
                if (args.Length == 3 && args[2].ToLower().Contains("verbose"))
                {
                    verbose = true;
                }
                
                try
                {
                    DirectoryInfo di = new DirectoryInfo(args[0]);
                    var filelist = di.EnumerateFiles("*.als", SearchOption.AllDirectories);
                    foreach (FileInfo f in filelist)
                    {
                        if (f.Name.StartsWith("."))
                        {
                            if(verbose) Console.WriteLine($"Ignoring {f.Name}");
                            continue;
                        }
                        if (f.FullName.Contains(@"\Backup\"))
                        {
                            if (verbose) Console.WriteLine($"Ignoring {f.Name}");
                            continue;
                        }

                        if (verbose) Console.WriteLine($"Processing {f.Name}...");
                        else Console.Write(".");
                        Directory.CreateDirectory("temp");
                        string filecontent = UnpackAlsFileToString(f);
                        if(filecontent.Contains(args[1])) MatchingProjects.Add(f.Name);

                    }

                    if (MatchingProjects.Count > 0)
                    {
                        Console.WriteLine($"\r\nFollowing Projects seem to contain the sample or plugin {args[1]}:\r\n");
                        foreach (string match in MatchingProjects)
                        {
                            Console.WriteLine($"{match}");
                        }
                    }
                    else
                    {
                        Console.WriteLine($"{args[1]} could not be found in target path.");
                    }
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"Reading path {args[0]} failed: {ex.Message} ({ex.StackTrace})");
                }
            }

            Console.ReadLine();
        }

        public static string UnpackAlsFileToString(FileInfo targetPath)
        {
            using (FileStream originalFileStream = targetPath.OpenRead())
            {
                string currentFileName = targetPath.FullName;
                string newFileName = Path.Combine("temp", targetPath.Name.Replace("als", "xml"));
                //Directory.CreateDirectory(newFileName);
                using (Stream decompressedStream = new MemoryStream())
                {
                    using (GZipStream decompressionStream = new GZipStream(originalFileStream, CompressionMode.Decompress))
                    {
                        decompressionStream.CopyTo(decompressedStream);
                        string filecontent;
                        decompressedStream.Seek(0, SeekOrigin.Begin);
                        using (StreamReader reader = new StreamReader(decompressedStream))
                        {
                            filecontent = reader.ReadToEnd();
                        }
                        return filecontent;
                    }

                }
            }

        }
        private static void DeleteWithSubFolders(string path)
        {
            try
            {
                if (!Directory.Exists(path)) return;
                var subfolders = Directory.GetDirectories(path);
                foreach (var subfolder in subfolders)
                {
                    DeleteWithSubFolders(subfolder);
                    var files = Directory.GetFiles(subfolder);

                    foreach (var file in files)
                    {
                        (new FileInfo(file)).Attributes &= ~FileAttributes.ReadOnly;
                        File.Delete(file);
                    }

                    Directory.Delete(subfolder);
                }
                foreach (var file in Directory.GetFiles(path))
                {
                    (new FileInfo(file)).Attributes &= ~FileAttributes.ReadOnly;
                    File.Delete(file);
                }
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
            }

        }

        //public static void UnpackAlsFile(FileInfo targetPath)
        //{
        //    using (FileStream originalFileStream = targetPath.OpenRead())
        //    {
        //        string currentFileName = targetPath.FullName;
        //        string newFileName = Path.Combine("temp", targetPath.Name.Replace("als","xml"));
        //        //Directory.CreateDirectory(newFileName);
        //        using (FileStream decompressedFileStream = File.Create(newFileName))
        //        {
        //            using (GZipStream decompressionStream = new GZipStream(originalFileStream, CompressionMode.Decompress))
        //            {
        //                decompressionStream.CopyTo(decompressedFileStream);
        //            }

        //        }
        //    }

        //}
    }
}
