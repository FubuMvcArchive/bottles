
using System;
using System.Collections.Generic;
using FubuCore;
using FubuCsProjFile.MSBuild;
using System.Linq;

namespace FubuCsProjFile
{
    public class CsProjFile
    {
        private readonly string _fileName;
        private readonly MSBuildProject _project;
        private readonly Lazy<MSBuildItemGroup> _codefiles;


        public CsProjFile(string fileName) : this(fileName, MSBuildProject.LoadFrom(fileName))
        {
        }

        private CsProjFile(string fileName, MSBuildProject project)
        {
            _fileName = fileName;
            _project = project;

            _codefiles = new Lazy<MSBuildItemGroup>(() => {
                var group = _project.ItemGroups.FirstOrDefault(x => x.Items.Any(item => item.IsCodeFile()));

                return group ?? _project.AddNewItemGroup();
            });
        }

        public void AddCodeFile(string relativePath)
        {
            _project.AddNewItem("Compile", relativePath);
        }

        public IEnumerable<CodeFile> CodeFiles()
        {
            var group = _project.ItemGroups.FirstOrDefault(x => x.Items.Any(item => item.IsCodeFile()));

            if (group == null)
            {
                yield break;
            }

            foreach (var item in group.Items)
            {
                yield return new CodeFile(item.Include);
            }
        }

        public static CsProjFile CreateAtSolutionDirectory(string assemblyName, string directory)
        {
            var fileName = directory.AppendPath(assemblyName) + ".csproj";
            var project = MSBuildProject.Create(assemblyName);

            return new CsProjFile(fileName, project);
        }

        public string FileName
        {
            get { return _fileName; }
        }

        public void Save()
        {
            reorderCodeFiles();

            _project.Save(_fileName);
        }

        private void reorderCodeFiles()
        {
            var codefiles = CodeFiles().OrderBy(x => x.RelativePath).ToArray();
            _codefiles.Value.Element.RemoveAll();

            codefiles.Each(x => _codefiles.Value.AddNewItem(CodeFile.COMPILE, x.RelativePath));
        }

        public static CsProjFile LoadFrom(string filename)
        {
            var project = MSBuildProject.LoadFrom(filename);
            return new CsProjFile(filename, project);
        }
    }

    

//    public class Target
//    {
//        public void do_something()
//        {
//            var csProjFile = new CsProjFile(@"C:\code\bottles\src\AssemblyPackage\AssemblyPackage.csproj");
//            new FileSystem().WriteStringToFile(@"C:\code\bottles\src\AssemblyPackage\1.txt", "some value");
//
//            csProjFile.EmbedResource("1.txt");
//
//            csProjFile.EmbeddedResources().Each(x => Debug.WriteLine(x));
//
//            csProjFile.Save();
//        }
//    }
}