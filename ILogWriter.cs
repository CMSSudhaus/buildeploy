using System;
using System.IO;
using System.Text;
using System.Reflection;

namespace Cms.Buildeploy
{

    public interface ILogWriter
    {
        void WriteLine(string format, params object[] args);
    }

}
