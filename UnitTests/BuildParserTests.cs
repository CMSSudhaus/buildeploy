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
        

        [Fact]
        public void TestVersionIncrement()
        {
            ChangeVersionParser parser = new ChangeVersionParser("*.*.*.+1", new ConsoleLogWriter());
            Assert.Equal("1.0.0.1", parser.ChangeVersion("1.0.0.0"));
            parser = new ChangeVersionParser("*.*.+1000.*", new ConsoleLogWriter());
            Assert.Equal("1.0.1001.0", parser.ChangeVersion("1.0.1.0"));
        }
    }


}
