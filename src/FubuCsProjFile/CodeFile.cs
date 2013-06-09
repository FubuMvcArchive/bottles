namespace FubuCsProjFile
{
    public class CodeFile : ProjectItem
    {
        public CodeFile(string relativePath) : base("Compile", relativePath)
        {
        }

        public CodeFile() : base("Compile")
        {
        }
    }
}