using System.Text;

namespace FubuCsProjFile
{
    public interface ISolutionFileModifier
    {
        bool Matches(string line);
        bool Modify(string line, StringBuilder builder);
    }
}