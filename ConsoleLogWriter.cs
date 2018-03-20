using System;
using System.IO;
using System.Text;
using System.Reflection;

namespace Cms.Buildeploy
{

    internal class ConsoleLogWriter : ILogWriter
    {
        #region ILogWriter Members

        public void WriteLine(string format, params object[] args)
        {
            Console.WriteLine(format, args);
        }

        #endregion
    }

}
