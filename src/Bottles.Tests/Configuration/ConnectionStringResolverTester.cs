using System.Collections.Generic;
using System.Xml;
using Bottles.Deployment;
using Bottles.Deployment.Deployers.Configuration;
using FubuCore;
using FubuCore.Util;
using NUnit.Framework;
using Rhino.Mocks;
using FubuTestingSupport;

namespace Bottles.Tests.Configuration
{
    [TestFixture]
    public class ConnectionStringResolverTester
    {
        private string xml = @"
<connectionStrings>
  <add name='DbConnectionString1' connectionString='Data Source={server1};Initial Catalog={catalog};User Id={user-id};Password={password};' providerName='System.Data.SqlClient'  />
  <add name='DbConnectionString2' connectionString='Data Source={server2};Initial Catalog={catalog};User Id={user-id};Password={password};' providerName='System.Data.SqlClient'  />
</connectionStrings>
";
        private string theFileName;

        [SetUp]
        public void SetUp()
        {
            var document = new XmlDocument();
            document.LoadXml(xml.Trim().Replace("'", "\""));
            theFileName = "connectionStrings.config";
            document.Save(theFileName);

            var settings = MockRepository.GenerateMock<DeploymentSettings>();
            var dict = new Dictionary<string, string>(){
                {"server1","first-server"}, 
                {"server2","second-server"}, 
                {"catalog","ourCatalog"}, 
                {"user-id","theUser"}, 
                {"password","thePassword"} 
            };

            var dictionaryKeyValues = new DictionaryKeyValues(dict);
            settings.Stub(x => x.SubstitutionValues()).Return(dictionaryKeyValues);


            var resolver = new ConnectionStringResolver(settings);
            resolver.Resolve(theFileName);
        }

        [Test]
        public void should_have_substituted_the_two_connection_strings()
        {
            var document = new XmlDocument();
            document.Load(theFileName);

            document.DocumentElement.FirstChild.As<XmlElement>().GetAttribute("connectionString")
                .ShouldEqual("Data Source=first-server;Initial Catalog=ourCatalog;User Id=theUser;Password=thePassword;");

            document.DocumentElement.LastChild.As<XmlElement>().GetAttribute("connectionString")
                .ShouldEqual("Data Source=second-server;Initial Catalog=ourCatalog;User Id=theUser;Password=thePassword;");
        }

    }
}