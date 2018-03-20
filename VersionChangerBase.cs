using System;
using System.IO;
using System.Text;
using System.Reflection;

namespace Cms.Buildeploy
{
    public abstract class VersionChangerBase
    {
        protected abstract int Change(int version);

        public string Change(string version)
        {
            int intVersion;
            try
            {
                intVersion = int.Parse(version);
            }
            catch (FormatException)
            {
                return version;
            }

            return Change(intVersion).ToString();
        }
    }

}
