namespace FubuCsProjFile
{
    public class EmbeddedResource : ProjectItem
    {
        public EmbeddedResource(string include) : base("EmbeddedResource", include)
        {
        }

        public EmbeddedResource() : base("EmbeddedResource")
        {
        }
    }
}