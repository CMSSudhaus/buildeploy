using System;
using System.IO;
using System.Text;
using System.Reflection;

namespace Cms.Buildeploy
{

    public class ConstantVersionChanger : VersionChangerBase
    {
        protected int newVersion;

        public ConstantVersionChanger(int init_newVersion)
        {
            newVersion = init_newVersion;
        }

        protected override int Change(int version)
        {
            return newVersion;
        }

        public int NewVersion { get { return newVersion; } }

    }

}
