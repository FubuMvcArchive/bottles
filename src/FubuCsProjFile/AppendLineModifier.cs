using System.Text;

namespace FubuCsProjFile
{
    public class AppendLineModifier : ISolutionFileModifier
    {
        public bool Matches(string line)
        {
            return true;
        }

        public bool Modify(string line, StringBuilder builder)
        {
            builder.AppendLine(line);
            return true;
        }
    }
}