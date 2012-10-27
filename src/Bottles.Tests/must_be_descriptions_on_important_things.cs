using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using FubuCore;
using FubuCore.Descriptions;
using FubuTestingSupport;
using NUnit.Framework;

namespace Bottles.Tests
{
    [TestFixture]
    public class must_be_descriptions_on_important_things
    {
        [Test]
        public void must_be_a_description_on_all_IPackageLoader_types()
        {
            IEnumerable<Type> types = typeof(IPackageLoader).Assembly.GetExportedTypes()
                .Where(x => x.IsConcreteTypeOf<IPackageLoader>())
                .Where(x => !Description.HasExplicitDescription(x));

            types.Each(x => Debug.WriteLine(x.Name));

            types.Any().ShouldBeFalse();
        }

        [Test]
        public void must_be_a_description_on_all_PackageInfo_types()
        {
            IEnumerable<Type> types = typeof(IPackageLoader).Assembly.GetExportedTypes()
                .Where(x => x.IsConcreteTypeOf<IPackageInfo>())
                .Where(x => !Description.HasExplicitDescription(x));

            types.Each(x => Debug.WriteLine(x.Name));

            types.Any().ShouldBeFalse();
        }
    }
}