using System;
using System.IO;
using System.Reflection;
using FubuCore;

namespace Bottles.Services.Remote
{
    public class AssemblyRequirement
    {
        private readonly static FileSystem fileSystem = new FileSystem();
        private readonly Assembly _assembly;

        public AssemblyRequirement(string name)
        {
            _assembly = Assembly.Load(name);
        }

        public AssemblyRequirement(Assembly assembly)
        {
            _assembly = assembly;
        }

        private static bool ShouldCopyFile(string fileName, AssemblyCopyMode copyMode)
        {
            if (copyMode == AssemblyCopyMode.Always) return true;

            return !fileSystem.FileExists(fileName);
        }

        public void Move(string directory)
        {
            Move(directory, AssemblyCopyMode.Once);
        }

        public void Move(string directory, AssemblyCopyMode copyMode)
        {
            var location = _assembly.Location;
            var fileName = Path.GetFileName(location);

            var filePath = directory.AppendPath(fileName);
            if (ShouldCopyFile(filePath, copyMode))
            {
                fileSystem.CopyToDirectory(location, directory);
            }


            var pdb = Path.GetFileNameWithoutExtension(fileName) + ".pdb";
            var pdbPath = directory.AppendPath(Path.GetFileName(pdb));
            if (fileSystem.FileExists(pdb) && ShouldCopyFile(pdbPath, copyMode))
            {
                fileSystem.CopyToDirectory(location.ParentDirectory().AppendPath(pdb), directory);
            }
        }
    }
}