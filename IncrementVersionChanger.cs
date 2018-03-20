using System;
using System.IO;
using System.Text;
using System.Reflection;

namespace Cms.Buildeploy
{

    public class IncrementVersionChanger : VersionChangerBase
    {
        int increment;
        public IncrementVersionChanger(int init_increment)
        {
            increment = init_increment;
        }

        protected override int Change(int version)
        {
            return version + increment;
        }

    }

}
