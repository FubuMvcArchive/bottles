namespace FubuCsProjFile.MSBuild
{
    public static class MsBuildExtensions
    {
         public static bool IsCodeFile(this MSBuildItem item)
         {
             return item.Include != null && item.Include.EndsWith(".cs");
         }
    }
}