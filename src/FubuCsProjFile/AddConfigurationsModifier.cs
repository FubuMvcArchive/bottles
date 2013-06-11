using System.Collections.Generic;
using System.Text;

namespace FubuCsProjFile
{
    public class AddConfigurationsModifier : ISolutionFileModifier
    {
        private readonly Sln _solution;

        public AddConfigurationsModifier(Sln solution)
        {
            _solution = solution;
        }

        public bool Matches(string line)
        {
            return line.EndsWith("GlobalSection(ProjectConfigurationPlatforms) = postSolution");
        }

        public bool Modify(string line, StringBuilder builder)
        {
            builder.AppendLine(line);
            _solution
                .PostSolutionConfiguration
                .Each(config => builder.AppendLine(config));
            return false;
        }
    }
}