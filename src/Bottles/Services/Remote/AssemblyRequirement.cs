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
        private readonly AssemblyCopyMode _copyMode;

        public AssemblyRequirement(string name)
            : this(name, AssemblyCopyMode.Once)
        {
        }

        public AssemblyRequirement(string name, AssemblyCopyMode copyMode)
        {
            _copyMode = copyMode;
            _assembly = Assembly.Load(name);
        }

        public AssemblyRequirement(Assembly assembly)
            : this(assembly, AssemblyCopyMode.Once)
        {
        }

        public AssemblyRequirement(Assembly assembly, AssemblyCopyMode copyMode)
        {
            _copyMode = copyMode;
            _assembly = assembly;
        }

        private bool ShouldCopyFile(string fileName)
        {
            if (_copyMode == AssemblyCopyMode.Always) return true;

            return !fileSystem.FileExists(fileName);
        }

        public void Move(string directory)
        {
            var location = _assembly.Location;
            var fileName = Path.GetFileName(location);

            var filePath = directory.AppendPath(fileName);
            if (ShouldCopyFile(filePath))
            {
                fileSystem.CopyToDirectory(location, directory);
            }


            var pdb = Path.GetFileNameWithoutExtension(fileName) + ".pdb";
            var pdbPath = directory.AppendPath(Path.GetFileName(pdb));
            if (fileSystem.FileExists(pdb) && ShouldCopyFile(pdbPath))
            {
                fileSystem.CopyToDirectory(location.ParentDirectory().AppendPath(pdb), directory);
            }
        }
    }
}