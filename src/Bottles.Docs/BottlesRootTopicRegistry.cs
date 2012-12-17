namespace Bottles.Docs
{
    public class BottlesRootTopicRegistry : FubuDocs.TopicRegistry
    {
        public BottlesRootTopicRegistry()
        {
            For<Bottles.Docs.BottlesRoot>().Append<Bottles.Docs.Architecture.ConsumingBottles>();
            For<Bottles.Docs.BottlesRoot>().Append<Bottles.Docs.Architecture.Architecture>();
            For<Bottles.Docs.BottlesRoot>().Append<Bottles.Docs.Architecture.CombiningBottlesWithStructuremap>();
            For<Bottles.Docs.BottlesRoot>().Append<Bottles.Docs.Architecture.PackageManifestSchema>();
            For<Bottles.Docs.BottlesRoot>().Append<Bottles.Docs.Architecture.BottlesFromTheCommandLine>();

            For<Bottles.Docs.Architecture.Architecture>().Append<Bottles.Docs.Architecture.Ipackageinfo>();
            For<Bottles.Docs.Architecture.Architecture>().Append<Bottles.Docs.Architecture.Packageinfo>();
            For<Bottles.Docs.Architecture.Architecture>().Append<Bottles.Docs.Architecture.Assemblypackageinfo>();
            For<Bottles.Docs.Architecture.Architecture>().Append<Bottles.Docs.Architecture.Packageregistry>();
            For<Bottles.Docs.Architecture.Architecture>().Append<Bottles.Docs.Architecture.Ipackageloader>();
            For<Bottles.Docs.Architecture.Architecture>().Append<Bottles.Docs.Architecture.Ibootstrapper>();
            For<Bottles.Docs.Architecture.Architecture>().Append<Bottles.Docs.Architecture.Iactivator>();
            For<Bottles.Docs.Architecture.Architecture>().Append<Bottles.Docs.Architecture.Packagefacility>();
            For<Bottles.Docs.Architecture.Architecture>().Append<Bottles.Docs.Architecture.Iassemblyloader>();

            For<Bottles.Docs.Architecture.Packageregistry>().Append<Bottles.Docs.Architecture.PackageAssemblies>();
            For<Bottles.Docs.Architecture.Packageregistry>().Append<Bottles.Docs.Architecture.Diagnostics>();
            For<Bottles.Docs.Architecture.Packageregistry>().Append<Bottles.Docs.Architecture.Packages>();

            For<Bottles.Docs.Architecture.BottlesFromTheCommandLine>().Append<Bottles.Docs.Architecture.BottlesInit>();
            For<Bottles.Docs.Architecture.BottlesFromTheCommandLine>().Append<Bottles.Docs.Architecture.BottlesOpenmanifest>();
            For<Bottles.Docs.Architecture.BottlesFromTheCommandLine>().Append<Bottles.Docs.Architecture.WorkingWithZipBottles>();
            For<Bottles.Docs.Architecture.BottlesFromTheCommandLine>().Append<Bottles.Docs.Architecture.LinkedFolderBottles>();
            For<Bottles.Docs.Architecture.BottlesFromTheCommandLine>().Append<Bottles.Docs.Architecture.AliasingFolders>();

        }
    }
}
