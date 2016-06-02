using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Cms.Buildeploy;

namespace UnitTests
{
    [TestClass]
    public class BuildParserTests
    {
        [TestMethod]
        public void TestParseBuildName()
        {
            Assert.AreEqual("*.*.211.1000", BuildNameParser.ParseBuildNameToPattern("Development_20140731.10",
                new DateTime(2014,1,1), "*.*"));
        }

        [TestMethod]
        public void TestGitBranchName()
        {
            Assert.AreEqual("master", Utils.GetGitBranchName());
        }
    }

}
