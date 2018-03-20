using System;
using System.IO;
using System.Text;
using System.Reflection;

namespace Cms.Buildeploy
{

    public class DummyVersionChanger : VersionChangerBase
    {
        protected override int Change(int version)
        {
            return version;
        }
    }

}
