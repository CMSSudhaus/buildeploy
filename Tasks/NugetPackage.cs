using System;
using System.IO;
using Microsoft.Build.Framework;
using System.Diagnostics;
using System.Threading;
using System.Globalization;

namespace Cms.Buildeploy.Tasks
{
    /// <summary>
    /// TODO: Update summary.
    /// </summary>
    public class NugetPackage : PackageTaskBase
    {
        [Required]
        public string NugetExePath
        {
            get;
            set;
        }

        [Required]
        public string NuspecFile
        {
            get;
            set;
        }

        [Required]
        public string OutputDirectory
        {
            get;
            set;
        }

        public string PackageId
        {
            get;
            set;
        }

        public string ApiKey
        {
            get;
            set;
        }

        public string PushLocation
        {
            get;
            set;
        }

        public string VersionTag
        {
            get; set;
        }
        protected override IPackageArchive CreatePackageArchive()
        {
            var archive = new NugetArchive(NugetExePath, Path.GetFullPath(NuspecFile), OutputDirectory, GetSematicVersion(), Log);
            if (!string.IsNullOrWhiteSpace(PackageId))
                archive.AddProperty("PackageId", PackageId);
            archive.ApiKey = ApiKey;
            archive.PushLocation = PushLocation;
            return archive;
        }

        private string GetSematicVersion()
        {
            if (string.IsNullOrWhiteSpace(VersionTag))
                return Version;
            else
                return string.Format(CultureInfo.InvariantCulture, "{0}-{1}", Version, VersionTag);
        }

        protected override string ReplaceDirectorySeparators(string entryName)
        {
            return entryName;
        }
    }

    public static class SilentProcessRunner
    {
        public static int ExecuteCommand(string executable, string arguments, string workingDirectory, Action<string> output, Action<string> error)
        {
            try
            {
                using (var process = new Process())
                {
                    process.StartInfo.FileName = executable;
                    process.StartInfo.Arguments = arguments;
                    process.StartInfo.WorkingDirectory = workingDirectory;
                    process.StartInfo.UseShellExecute = false;
                    process.StartInfo.CreateNoWindow = true;
                    process.StartInfo.RedirectStandardOutput = true;
                    process.StartInfo.RedirectStandardError = true;
                    using (var outputWaitHandle = new AutoResetEvent(false))
                    using (var errorWaitHandle = new AutoResetEvent(false))
                    {
                        process.OutputDataReceived += (sender, e) =>
                        {
                            if (e.Data == null)
                            {
                                outputWaitHandle.Set();
                            }
                            else
                            {
                                output(e.Data);
                            }
                        }

                        ;
                        process.ErrorDataReceived += (sender, e) =>
                        {
                            if (e.Data == null)
                            {
                                errorWaitHandle.Set();
                            }
                            else
                            {
                                error(e.Data);
                            }
                        }

                        ;
                        process.Start();
                        process.BeginOutputReadLine();
                        process.BeginErrorReadLine();
                        process.WaitForExit();
                        outputWaitHandle.WaitOne();
                        errorWaitHandle.WaitOne();
                        return process.ExitCode;
                    }
                }
            }
            catch (Exception ex)
            {
                throw new Exception(string.Format("Error when attempting to execute {0}: {1}", executable, ex.Message), ex);
            }
        }
    }
}