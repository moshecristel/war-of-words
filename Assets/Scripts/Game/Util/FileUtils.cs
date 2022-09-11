using System.Collections.Generic;
using System.IO;
using UnityEngine;

namespace WarOfWords
{
    public static class FileUtils
    {
        public static void  WriteToFile(List<string> lines, string path)
        {
            int lineCount = lines.Count;
            using (var sw = File.CreateText(path))
            {
                foreach (string line in lines)
                {
                    sw.WriteLine(line);
                }
            }
            
            Debug.Log($"{lineCount} lines successfully written to '{path}'.");
        }
    }
}
