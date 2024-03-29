﻿using System;
using System.Collections.Generic;
using System.IO;
using Cms.Buildeploy;
using Cms.Buildeploy.Tasks;
using Moq;
using Xunit;

namespace UnitTests
{
    public class VersioningTests
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


        [Fact]
        public void GitTagVersionWithNoTagsTest()
        {
            GitVersionWorker worker = new GitVersionWorker(CreateVersionTask("master", new string[0]));
            worker.Execute();
            Assert.Equal(new Version("0.0.1000.0"), worker.NewVersion);
            Assert.Equal("build-master-0.0.1000.0", worker.TagName);
        }

        [Fact]
        public void GitTagVersionMasterTest()
        {
            GitVersionWorker worker = new GitVersionWorker(CreateVersionTask("master", new[] { "aaaa", "build-master-2.0.0.0" }));
            worker.Execute();
            Assert.Equal(new Version("2.0.1000.0"), worker.NewVersion);
            Assert.Equal("build-master-2.0.1000.0", worker.TagName);
        }

        [Fact]
        public void GitTagVersionFirstHotFixTest()
        {
            GitVersionWorker worker = new GitVersionWorker(CreateVersionTask("hotfix-TTT-1", new[] { "aaaa", "build-master-2.0.1000.0" }));
            worker.Execute();
            Assert.Equal(new Version("2.0.1001.0"), worker.NewVersion);
            Assert.Equal("build-hotfix-TTT-1-2.0.1001.0", worker.TagName);
        }

        [Fact]
        public void GitTagVersionSecondHotFixTest()
        {
            GitVersionWorker worker = new GitVersionWorker(CreateVersionTask("hotfix-TTT-1", new[] { "aaaa", "build-master-2.0.1000.0" , "build-hotfix-TTT-1-2.0.1001.0"}));
            worker.Execute();
            Assert.Equal(new Version("2.0.1002.0"), worker.NewVersion);
            Assert.Equal("build-hotfix-TTT-1-2.0.1002.0", worker.TagName);
        }

        private static IGitVersionTask CreateVersionTask(string branchName, string[] tags)
        {
            var taskMock = new Mock<IGitVersionTask>();
            taskMock.Setup(t => t.HotfixBranchPrefix).Returns("hotfix-");
            taskMock.Setup(t => t.HotfixVersionPattern).Returns("*.*.+1.*");
            taskMock.Setup(t => t.MasterBranchName).Returns("master");
            taskMock.Setup(t => t.MasterVersionPattern).Returns("*.*.+1000.*");
            taskMock.Setup(t => t.BuildTagPrefix).Returns("build-");

            var tagProviderMock = new Mock<IGitTagProvider>();
            tagProviderMock.Setup(tp => tp.GetTags()).Returns(tags);
            tagProviderMock.Setup(tp => tp.CurrentBranchName).Returns(branchName);
            taskMock.Setup(t => t.CreateTagProvider()).Returns(tagProviderMock.Object);
            return taskMock.Object;
        }

        [Fact]
        public void ChangeVersionWithUsingTest()
        {
            string versionCode = "using System.Reflection;\r\n[assembly: AssemblyVersion(\"1.0.0.0\")]\r\n";
            ChangeVersionParser parser = new ChangeVersionParser("2.0.0.0", new Mock<ILogWriter>().Object);
            var sw = new StringWriter();
            parser.ProcessAssemblyInfo(new StringReader(versionCode), sw);
            Assert.Equal("using System.Reflection;\r\n[assembly: AssemblyVersion(\"2.0.0.0\")]\r\n", sw.ToString());
        }

        [Fact]
        public void ChangeVersionExplicitNamespaceTest()
        {
            string versionCode = "[assembly: System.Reflection.AssemblyVersion(\"1.0.0.0\")]\r\n";
            ChangeVersionParser parser = new ChangeVersionParser("2.0.0.0", new Mock<ILogWriter>().Object);
            var sw = new StringWriter();
            parser.ProcessAssemblyInfo(new StringReader(versionCode), sw);
            Assert.Equal("[assembly: System.Reflection.AssemblyVersion(\"2.0.0.0\")]\r\n", sw.ToString());
        }


        [Fact]
        public void ChangeSmallerVersionTest()
        {
            string versionCode = "using System.Reflection;\r\n[assembly: AssemblyVersion(\"2.0.0.0\")]\r\n";
            ChangeVersionParser parser = new ChangeVersionParser("1.0.0.0", new Mock<ILogWriter>().Object);
            var sw = new StringWriter();
            parser.ProcessAssemblyInfo(new StringReader(versionCode), sw);
            Assert.Equal("using System.Reflection;\r\n[assembly: AssemblyVersion(\"2.0.0.0\")]\r\n", sw.ToString());
        }

    }


}
