using FubuCore;

namespace Bottles.PackageLoaders.LinkedFolders
{
    public interface ILinksService
    {
        LinkManifest GetLinkManifest(string path);
        bool LinkManifestExists(string path);
        void Save(LinkManifest manifest, string path);
    }


    public class LinksService : ILinksService
    {
        private IFileSystem _fileSystem;

        public LinksService(IFileSystem fileSystem)
        {
            _fileSystem = fileSystem;
        }

        public LinkManifest GetLinkManifest(string path)
        {
            var result = new LinkManifest();
            path = path.AppendPath(LinkManifest.FILE);

            if(_fileSystem.FileExists(path))
            {
                result = _fileSystem.LoadFromFile<LinkManifest>(path);
            }

            return result;

        }

        public bool LinkManifestExists(string path)
        {
            return _fileSystem.FileExists(path, LinkManifest.FILE);
        }

        public void Save(LinkManifest manifest, string path)
        {
            _fileSystem.PersistToFile(manifest, path, LinkManifest.FILE);
        }
    }
}