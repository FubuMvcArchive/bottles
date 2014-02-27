using NUnit.Framework;
using System;
using Bottles.PackageLoaders.LinkedFolders;
using FubuCore;
using Bottles.Diagnostics;
using System.Collections.Generic;
using FubuTestingSupport;
using System.Linq;

namespace Bottles.Tests.PackageLoaders.Assemblies.LinkedFolders
{
    [TestFixture]
    public class LinkedFolderPackageLoaderTester
    {
        [Test]
        public void linked_project_should_have_correct_package_name ()
        {
            var loader = new LinkedFolderPackageLoader (".".ToFullPath().ParentDirectory().ParentDirectory(), folder => folder);
            IEnumerable<IPackageInfo> packages = loader.Load (new PackageLog ());

            packages.ShouldHaveCount (1);
            packages.FirstOrDefault().Name.ShouldEqual ("FakeProject");
        }
    }
}

