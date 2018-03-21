using Microsoft.Build.Utilities;

namespace Cms.Buildeploy.Tasks
{
    public abstract class LogWriterTaskBase : Task, ILogWriter
    {
        #region ILogWriter Members

        void ILogWriter.WriteLine(string format, params object[] args)
        {
            Log.LogMessage(format, args);
        }

        #endregion

    }
}
