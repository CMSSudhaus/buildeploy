using System;
using Cms.Buildeploy;

namespace UnitTests
{
    internal class ConsoleLogWriter : ILogWriter
    {
        public void WriteLine(string format, params object[] args) => Console.WriteLine(format, args);

    }


}
