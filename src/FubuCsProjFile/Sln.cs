using System.Collections.Generic;
using FubuCore;

namespace FubuCsProjFile
{
    [MarkedForTermination]
    public class Sln
    {
        private readonly IList<CsProjFile>  _projects = new List<CsProjFile>();
        private readonly IList<string> _postSolution = new List<string>(); 

        public Sln(string fileName)
        {
            FileName = fileName;
        }

        public string FileName { get; private set; }
        public IEnumerable<CsProjFile> Projects { get { return _projects; } }
        public IEnumerable<string> PostSolutionConfiguration { get { return _postSolution; } }

        public void AddProject(CsProjFile project)
        {
            _projects.Fill(project);
        }

        public void RegisterPostSolutionConfiguration(string projectGuid, string config)
        {
            var id = "{" + projectGuid + "}";
            _postSolution.Fill("\t\t{0}.{1}".ToFormat(id, config));
        }

        public void RegisterPostSolutionConfigurations(string projectGuid, params string[] configs)
        {
            configs.Each(config => RegisterPostSolutionConfiguration(projectGuid, config));
        }
    }
}