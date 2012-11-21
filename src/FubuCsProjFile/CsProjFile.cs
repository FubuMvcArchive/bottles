using System;
using System.Collections.Generic;
using System.Diagnostics;
using FubuCore;
using Microsoft.Build.Evaluation;
using System.Linq;

namespace FubuCsProjFile
{
    public class CsProjFile
    {
        private readonly Project _project;

        public CsProjFile(string filename)
        {
            _project = ProjectCollection.GlobalProjectCollection.LoadProject(filename);

            var list = new List<ProjectItem>();
            list.AddRange(_project.Items);

            foreach (var item in _project.Items)
            {
                Debug.WriteLine(item.ItemType);
            }
        }

        public IEnumerable<AssemblyReference> AssemblyReferences()
        {
            return _project.Items.Where(x => x.ItemType == "Reference").Select(x => new AssemblyReference(x));
        } 

        public IEnumerable<EmbeddedResource> EmbeddedResources()
        {
            return _project.Items.Where(x => x.ItemType == "EmbeddedResource").Select(x => new EmbeddedResource(x));
        } 

        public void EmbedResource(string filename)
        {
            var existing =
                _project.Items.FirstOrDefault(
                    x =>
                    x.ItemType == "EmbeddedResource" &&
                    x.EvaluatedInclude.Equals(filename, StringComparison.InvariantCultureIgnoreCase));

            if (existing != null) return;

            _project.AddItem("EmbeddedResource", filename);
        }

        public void Save()
        {
            _project.Save();
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