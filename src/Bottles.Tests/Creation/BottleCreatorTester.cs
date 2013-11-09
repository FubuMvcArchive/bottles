using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Bottles.Commands;
using Bottles.Creation;
using Bottles.Diagnostics;
using Bottles.Exploding;
using Bottles.PackageLoaders.Assemblies;
using Bottles.Tests.Zipping;
using Bottles.Zipping;
using FubuCore;
using FubuTestingSupport;
using NUnit.Framework;
using Rhino.Mocks;

namespace Bottles.Tests.Creation
{
    [TestFixture]
    public class when_creating_a_bottle_for_all_assemblies_found_and_including_pdbs : InteractionContext<ZipPackageCreator>
    {
        private PackageManifest theManifest;
        private AssemblyFiles theAssemblyFiles;
        private CreateBottleInput theInput;
        private StubZipFileService _theZipFileService;
        private string thePackageManifestFileName;
		private string theBaseFolder;
		private string theBinFolder;
		
        protected override void beforeEach()
        {
			theBaseFolder = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "folder1");
			theBinFolder = Path.Combine(theBaseFolder, "bin");	
			
            theManifest = new PackageManifest
            {
                ContentFileSet = new FileSet()
            };

            theManifest.AddAssembly("A");
            theManifest.AddAssembly("B");
            theManifest.AddAssembly("C");

            theInput = new CreateBottleInput()
            {
                PackageFolder = theBaseFolder,
                ZipFileFlag = Path.Combine(theBaseFolder, "package1.zip"),
                PdbFlag = true
            };

            theAssemblyFiles = new AssemblyFiles()
            {
                Files = new string[] { 
					FileSystem.Combine(theBinFolder, "a.dll"), 
					FileSystem.Combine(theBinFolder, "b.dll"), 
					FileSystem.Combine(theBinFolder, "c.dll") 
				},
                MissingAssemblies = new string[0],
                PdbFiles = new string[] { 
					FileSystem.Combine(theBinFolder, "a.pdb"), 
					FileSystem.Combine(theBinFolder, "b.pdb"), 
					FileSystem.Combine(theBinFolder, "c.pdb")
				},
            };

            MockFor<IAssemblyFileFinder>()
                .Stub(x => x.FindAssemblies(theBinFolder, theManifest.AllAssemblies))
                .Return(theAssemblyFiles);

            _theZipFileService = new StubZipFileService();
            Services.Inject<IZipFileService>(_theZipFileService);

            thePackageManifestFileName = FileSystem.Combine(theBaseFolder, PackageManifest.FILE);

            ClassUnderTest.CreatePackage(theInput, theManifest);
        }

        [Test]
        public void should_not_log_assemblies_missing()
        {
            MockFor<IBottleLogger>().AssertWasNotCalled(x => x.WriteAssembliesNotFound(theAssemblyFiles, theManifest, theInput, theBinFolder));
        }

        [Test]
        public void should_have_written_a_zip_file_to_the_value_from_the_input()
        {
            _theZipFileService.FileName.ShouldEqual(theInput.ZipFileFlag);
        }

        [Test]
        public void should_have_written_each_assembly_to_the_zip_file()
        {
            _theZipFileService.AllEntries.ShouldContain(new StubZipEntry(Path.Combine(theBinFolder, "a.dll"), "bin"));
            _theZipFileService.AllEntries.ShouldContain(new StubZipEntry(Path.Combine(theBinFolder, "b.dll"), "bin"));
            _theZipFileService.AllEntries.ShouldContain(new StubZipEntry(Path.Combine(theBinFolder, "c.dll"), "bin"));
        }

        [Test]
        public void should_have_written_each_pdb_to_the_zip_file()
        {
            _theZipFileService.AllEntries.ShouldContain(new StubZipEntry(Path.Combine(theBinFolder, "a.pdb"), "bin"));
            _theZipFileService.AllEntries.ShouldContain(new StubZipEntry(Path.Combine(theBinFolder, "b.pdb"), "bin"));
            _theZipFileService.AllEntries.ShouldContain(new StubZipEntry(Path.Combine(theBinFolder, "c.pdb"), "bin"));
        }

        [Test]
        public void should_write_the_package_manifest_to_the_zip()
        {
            _theZipFileService.AllEntries.ShouldContain(new StubZipEntry(thePackageManifestFileName, string.Empty));
        }

        [Test]
        public void add_the_content_files()
        {
            _theZipFileService.ZipRequests.ShouldContain(new ZipFolderRequest(){
                FileSet = theManifest.ContentFileSet,
                ZipDirectory = BottleFiles.WebContentFolder,
                RootDirectory = theInput.PackageFolder
            });
        }
    }

    [TestFixture]
    public class when_creating_a_package_for_all_assemblies_found_and_not_including_pdbs : InteractionContext<ZipPackageCreator>
    {
        private PackageManifest theManifest;
        private AssemblyFiles theAssemblyFiles;
        private CreateBottleInput theInput;
        private StubZipFileService _theZipFileService;
		
		private string theBaseFolder;
		private string theBinFolder;

        protected override void beforeEach()
        {
			theBaseFolder = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "folder1");
			theBinFolder = Path.Combine(theBaseFolder, "bin");	
			
            theManifest = new PackageManifest
            {
                ContentFileSet = new FileSet()
            };

            theManifest.AddAssembly("A");
            theManifest.AddAssembly("B");
            theManifest.AddAssembly("C");

            theInput = new CreateBottleInput()
            {
                PackageFolder = theBaseFolder,
                ZipFileFlag = Path.Combine(theBaseFolder, "package1.zip"),
                PdbFlag = false
            };

            theAssemblyFiles = new AssemblyFiles()
            {
                Files = new string[] { 
					FileSystem.Combine(theBinFolder, "a.dll"), 
					FileSystem.Combine(theBinFolder, "b.dll"), 
					FileSystem.Combine(theBinFolder, "c.dll") 
				},
                MissingAssemblies = new string[0],
                PdbFiles = new string[] { 
					FileSystem.Combine(theBinFolder, "a.pdb"), 
					FileSystem.Combine(theBinFolder, "b.pdb"), 
					FileSystem.Combine(theBinFolder, "c.pdb") 
				},
            };

            MockFor<IAssemblyFileFinder>()
                .Stub(x => x.FindAssemblies(theBinFolder, theManifest.AllAssemblies))
                .Return(theAssemblyFiles);

            _theZipFileService = new StubZipFileService();
            Services.Inject<IZipFileService>(_theZipFileService);

            ClassUnderTest.CreatePackage(theInput, theManifest);
        }


        [Test]
        public void should_have_written_each_pdb_to_the_zip_file()
        {
            _theZipFileService.AllEntries.ShouldNotContain(new StubZipEntry(Path.Combine(theBinFolder, "a.pdb"), "bin"));
            _theZipFileService.AllEntries.ShouldNotContain(new StubZipEntry(Path.Combine(theBinFolder, "b.pdb"), "bin"));
            _theZipFileService.AllEntries.ShouldNotContain(new StubZipEntry(Path.Combine(theBinFolder, "c.pdb"), "bin"));
        }
    }

    public class StubZipFileService : IZipFileService
    {
        private string _fileName;
        private IList<StubZipEntry> _allEntries;
        private IList<ZipFolderRequest> _requests;

        public void CreateZipFile(string fileName, Action<IZipFile> configure)
        {
            _fileName = fileName;
            var stubFile = new StubZipFile();
            configure(stubFile);

            _allEntries = stubFile.AllZipEntries;
            _requests = stubFile.ZipRequests;
        }

        public void ExtractTo(string description, Stream stream, string folder)
        {
            throw new NotImplementedException();
        }

        public PackageManifest GetPackageManifest(string fileName)
        {
            throw new NotImplementedException();
        }

        public void ExtractTo(string fileName, string folder, ExplodeOptions options)
        {
            throw new NotImplementedException();
        }

        public string GetVersion(string fileName)
        {
            throw new NotImplementedException();
        }

        public IList<ZipFolderRequest> ZipRequests
        {
            get
            {
                return _requests;
            }
        }

        public string FileName
        {
            get { return _fileName; }
        }

        public IList<StubZipEntry> AllEntries
        {
            get { return _allEntries; }
        }
    }

    [TestFixture]
    public class when_trying_to_create_a_package_and_not_all_assemblies_are_found : InteractionContext<ZipPackageCreator>
    {
        private PackageManifest theManifest;
        private AssemblyFiles theAssemblyFiles;
        private CreateBottleInput theInput;
        private string theBaseFolder;
        private string theBinFolder;

        protected override void beforeEach()
        {
            theBaseFolder = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "folder1");
            theBinFolder = Path.Combine(theBaseFolder, "bin");

            theManifest = new PackageManifest
            {
                ContentFileSet = new FileSet(),
                Assemblies = new[] { "A", "B", "C" },
                NativeAssemblies = new[] { "D", "E" }
            };

            theInput = new CreateBottleInput { PackageFolder = theBaseFolder };

            theAssemblyFiles = new AssemblyFiles
            {
                Files = new[] { "a.dll", "b.dll", "d.dll" },
                MissingAssemblies = new[] { "c", "e" },
                PdbFiles = new[] { "a.pdb", "b.pdb", "c.pdb" }
            };

            MockFor<IAssemblyFileFinder>()
                .Stub(x => x.FindAssemblies(theBinFolder, theManifest.AllAssemblies))
                .Return(theAssemblyFiles);

            ClassUnderTest.CreatePackage(theInput, theManifest);
        }

        [Test]
        public void log_the_missing_assemblies()
        {
            MockFor<IBottleLogger>().AssertWasCalled(x => x.WriteAssembliesNotFound(theAssemblyFiles, theManifest, theInput, theBinFolder));
        }

        [Test]
        public void do_not_call_the_zip_file_creator_at_all()
        {
            MockFor<IZipFileService>().AssertWasNotCalled(x => x.CreateZipFile(null, null), x => x.IgnoreArguments());
        }
    }

    [TestFixture]
    public class when_creating_a_package_for_assemblies_and_native_assemblies : InteractionContext<ZipPackageCreator>
    {
        private PackageManifest theManifest;
        private AssemblyFiles theAssemblyFiles;
        private CreateBottleInput theInput;
        private StubZipFileService _theZipFileService;
        private string thePackageManifestFileName;
        private string theBaseFolder;
        private string theBinFolder;

        protected override void beforeEach()
        {
            theBaseFolder = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "folder1");
            theBinFolder = Path.Combine(theBaseFolder, "bin");

            theManifest = new PackageManifest
            {
                ContentFileSet = new FileSet(),
                Assemblies = new[] { "A" },
                NativeAssemblies = new[] { "B" }
            };

            theInput = new CreateBottleInput
            {
                PackageFolder = theBaseFolder,
                ZipFileFlag = Path.Combine(theBaseFolder, "package1.zip"),
                PdbFlag = true
            };

            theAssemblyFiles = new AssemblyFiles
            {
                Files = new[]
                {
                    FileSystem.Combine(theBinFolder, "a.dll"),
                    FileSystem.Combine(theBinFolder, "b.dll")
                },
                MissingAssemblies = new string[0],
                PdbFiles = new[]
                {
                    FileSystem.Combine(theBinFolder, "a.pdb")
                }
            };

            MockFor<IAssemblyFileFinder>()
                .Stub(x => x.FindAssemblies(theBinFolder, theManifest.AllAssemblies))
                .Return(theAssemblyFiles);

            _theZipFileService = new StubZipFileService();
            Services.Inject<IZipFileService>(_theZipFileService);

            thePackageManifestFileName = FileSystem.Combine(theBaseFolder, PackageManifest.FILE);

            ClassUnderTest.CreatePackage(theInput, theManifest);
        }

        [Test]
        public void should_not_log_assemblies_missing()
        {
            MockFor<IBottleLogger>().AssertWasNotCalled(x => x.WriteAssembliesNotFound(theAssemblyFiles, theManifest, theInput, theBinFolder));
        }

        [Test]
        public void should_have_written_a_zip_file_to_the_value_from_the_input()
        {
            _theZipFileService.FileName.ShouldEqual(theInput.ZipFileFlag);
        }

        [Test]
        public void should_have_written_each_assembly_to_the_zip_file()
        {
            _theZipFileService.AllEntries.ShouldContain(new StubZipEntry(Path.Combine(theBinFolder, "a.dll"), "bin"));
            _theZipFileService.AllEntries.ShouldContain(new StubZipEntry(Path.Combine(theBinFolder, "b.dll"), "bin"));
        }

        [Test]
        public void should_have_written_each_pdb_to_the_zip_file()
        {
            _theZipFileService.AllEntries.ShouldContain(new StubZipEntry(Path.Combine(theBinFolder, "a.pdb"), "bin"));
        }

        [Test]
        public void should_have_writen_the_package_manifest_to_the_zip()
        {
            _theZipFileService.AllEntries.ShouldContain(new StubZipEntry(thePackageManifestFileName, string.Empty));
        }

        [Test]
        public void should_have_added_the_content_files()
        {
            _theZipFileService.ZipRequests.ShouldContain(new ZipFolderRequest
            {
                FileSet = theManifest.ContentFileSet,
                ZipDirectory = BottleFiles.WebContentFolder,
                RootDirectory = theInput.PackageFolder
            });
        }
    }

    [TestFixture]
    public class when_adding_config_files_to_zip : InteractionContext<ZipPackageCreator>
    {
        private PackageManifest theManifest;
        private CreateBottleInput theInput;
        private string theBaseFolder;
        private StubZipFile theZipFile;
        private ZipFolderRequest theRequest;

        protected override void beforeEach()
        {
            theBaseFolder = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "folder1");

            theManifest = new PackageManifest();
            theManifest.SetRole(BottleRoles.Config);
            
            theInput = new CreateBottleInput { PackageFolder = theBaseFolder };

            theZipFile = new StubZipFile();

            ClassUnderTest.AddConfigFiles(theInput, theZipFile, theManifest);

            theRequest = theZipFile.ZipRequests.Single();
        }

        [Test]
        public void should_set_zip_directory_to_empty_to_avoid_having_nested_config_config_folders()
        {
            theRequest.ZipDirectory.ShouldEqual(BottleFiles.ConfigFolder);
        }

        [Test]
        public void should_set_the_root_directory_as_the_config_subfolder_under_the_package_folder()
        {
            theRequest.RootDirectory.ShouldEqual(Path.Combine(theBaseFolder, BottleFiles.ConfigFolder));
        }
    }

    [TestFixture]
    public class when_adding_data_files_to_zip : InteractionContext<ZipPackageCreator>
    {
        private PackageManifest theManifest;
        private CreateBottleInput theInput;
        private string theBaseFolder;
        private StubZipFile theZipFile;
        private ZipFolderRequest theRequest;

        protected override void beforeEach()
        {
            theBaseFolder = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "folder1");

            theManifest = new PackageManifest();
            theManifest.SetRole(BottleRoles.Data);

            theInput = new CreateBottleInput { PackageFolder = theBaseFolder };

            theZipFile = new StubZipFile();

            ClassUnderTest.AddDataFiles(theInput, theZipFile, theManifest);

            theRequest = theZipFile.ZipRequests.Single();
        }

        [Test]
        public void should_set_zip_directory_to_empty_to_avoid_having_nested_data_data_folders()
        {
            theRequest.ZipDirectory.ShouldEqual(BottleFiles.DataFolder);
        }

        [Test]
        public void should_set_the_root_directory_as_the_data_subfolder_under_the_package_folder()
        {
            theRequest.RootDirectory.ShouldEqual(Path.Combine(theBaseFolder, BottleFiles.DataFolder));
        }
    }
}