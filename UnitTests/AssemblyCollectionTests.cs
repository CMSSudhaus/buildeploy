using Cms.Buildeploy.ReferenceCheck;
using System;
using System.Reflection;
using Xunit;

namespace UnitTests
{
    public class AssemblyCollectionTests
    {
        [Fact]
        public void RedirectTest()
        {
            AssemblyCollection assemblies = new AssemblyCollection
            {
                new AssemblyName("Test, Version=2.0.0.0")
            };
            assemblies.AddRedirect(new Version(1, 0, 0, 0), new Version(2, 0, 0, 0), new AssemblyName("Test, Version=2.0.0.0"));
            Assert.NotNull(assemblies.Find(new AssemblyName("Test, Version=1.0.0.0")));
        }

        [Fact]
        public void RedirectReadFromConfigTest()
        {
            AssemblyCollection assemblies = new AssemblyCollection
            {
                new AssemblyName("Test, Version=2.0.0.0, publicKeyToken=31bf3856ad364e35")
            };
            assemblies.ReadRedirectsFromConfigXmlString(@"<?xml version=""1.0"" encoding=""utf-8""?>
                <configuration>
	                <runtime>
		                <assemblyBinding xmlns=""urn:schemas-microsoft-com:asm.v1"">
			                <dependentAssembly>
				                <assemblyIdentity name=""Test"" publicKeyToken=""31bf3856ad364e35"" culture=""neutral"" />
				                <bindingRedirect oldVersion=""0.0.0.0-2.0.0.0"" newVersion=""2.0.0.0"" />
			                </dependentAssembly>
		                </assemblyBinding>
	                </runtime>
                </configuration>");
            Assert.NotNull(assemblies.Find(new AssemblyName("Test, Version=1.0.0.0, publicKeyToken=31bf3856ad364e35")));
        }
    }
}
