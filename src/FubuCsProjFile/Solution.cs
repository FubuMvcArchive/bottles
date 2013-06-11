using System;
using System.IO;
using System.Reflection;
using FubuCore;

namespace FubuCsProjFile
{
    public class Solution
    {
        public static Solution CreateNew(string directory, string name)
        {
            var text = Assembly.GetExecutingAssembly().GetManifestResourceStream(typeof (Solution), "Solution.txt")
                               .ReadAllText();

            var filename = directory.AppendPath(name);
            if (!Path.HasExtension(filename))
            {
                filename = filename + ".sln";
            }

            return new Solution(filename, text);
        }

        public static Solution LoadFrom(string filename)
        {
            var text = new FileSystem().ReadStringFromFile(filename);
            return new Solution(filename, text);
        }

        private Solution(string filename, string text)
        {
            
        }
    }
}