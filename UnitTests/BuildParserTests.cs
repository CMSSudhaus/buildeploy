using System;
using Cms.Buildeploy;
using Xunit;

namespace UnitTests
{
    public class BuildParserTests
    {
        [Fact]
        public void TestParseBuildName()
        {
            Assert.Equal("*.*.211.1000", BuildNameParser.ParseBuildNameToPattern("Development_20140731.10", new DateTime(2014, 1, 1), "*.*"));
        }

        
    }

}
