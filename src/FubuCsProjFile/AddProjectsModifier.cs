using System.Collections.Generic;
using System.Text;
using FubuCore;

namespace FubuCsProjFile
{
    public class AddProjectsModifier : ISolutionFileModifier
    {
        private bool _appended;
        private readonly Sln _solution;

        public AddProjectsModifier(Sln solution)
        {
            _solution = solution;
        }

        public bool Matches(string line)
        {
            return line.Equals("Global") && !_appended;
        }

        public bool Modify(string line, StringBuilder builder)
        {
            _solution
                .Projects
                .Each(project =>
                {
                    var projectGuid = "{" + project.ProjectGuid + "}";
                    
                    
//                    //var projectType = "Project(\"{" + project.ProjectType + "}\")";
//                    builder.AppendLine("{0} = \"{1}\", \"{2}\", \"{3}\"".ToFormat(projectType,
//                                                                                  project.Name,
//                                                                                  project.RelativePath,
//                                                                                  projectGuid));
                    builder.AppendLine("EndProject");
                });
            
            _appended = true;
            return true;
        }
    }
}