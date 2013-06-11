using System.Collections.Generic;
using System.Linq;
using System.Text;
using FubuCore;

namespace FubuCsProjFile
{
    [MarkedForTermination]
    public class SolutionFileService : ISolutionFileService
    {
        private readonly IFileSystem _fileSystem;

        public SolutionFileService(IFileSystem fileSystem)
        {
            _fileSystem = fileSystem;
        }

        public string[] SplitSolution(string solutionContents)
        {
            return solutionContents.SplitOnNewLine();
        }

        public void Save(Sln solution)
        {
            var solutionContents = _fileSystem.ReadStringFromFile(solution.FileName);
            var solutionBuilder = new StringBuilder();
            var modifiers = new List<ISolutionFileModifier>
            {
                new AddProjectsModifier(solution),
                new AddConfigurationsModifier(solution),
                new AppendLineModifier()
            };

            var lines = SplitSolution(solutionContents);
            lines.Each(line =>
            {
                var filteredModifiers = modifiers.Where(m => m.Matches(line));
                foreach(var m in filteredModifiers)
                {
                    if(!m.Modify(line, solutionBuilder))
                    {
                        break;
                    }
                }
            });

            _fileSystem.WriteStringToFile(solution.FileName, solutionBuilder.ToString());
        }
    }
}