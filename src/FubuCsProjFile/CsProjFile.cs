
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

        public CsProjFile(string fileName) : this(fileName, MSBuildProject.LoadFrom(fileName))
        {
        }

        private CsProjFile(string fileName, MSBuildProject project)
        {
            _fileName = fileName;
            _project = project;
        }

        public void Add<T>(T item) where T : ProjectItem
        {
            var group = _project.FindGroup(item.Matches) ?? _project.FindGroup(x => x.Name == item.Name);
            item.Configure(group);
        }

        public IEnumerable<T> All<T>() where T : ProjectItem, new()
        {
            var name = new T().Name;

            return _project.GetAllItems(name).OrderBy(x => x.Include)
                           .Select(item => {
                               var projectItem = new T();
                               projectItem.Read(item);

                               return projectItem;
                           });
        } 

        public T Add<T>(string include) where T : ProjectItem, new()
        {
            var item = new T {Include = include};
            Add(item);

            return item;
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
            _project.Save(_fileName);
        }

        public static CsProjFile LoadFrom(string filename)
        {
            var project = MSBuildProject.LoadFrom(filename);
            return new CsProjFile(filename, project);
        }
    }
}