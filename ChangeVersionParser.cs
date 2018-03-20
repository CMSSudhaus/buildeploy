using System;
using System.IO;
using System.Text;
using System.Reflection;

namespace Cms.Buildeploy
{

    public class ChangeVersionParser
    {

        private VersionChangerBase[] versionChangers;
        private string constantVersion = string.Empty;
        private string version;
        private ILogWriter logWriter;

        public ChangeVersionParser(string version, ILogWriter logWriter)
        {
            if (version == null) throw new ArgumentNullException(nameof(version));
            if (logWriter == null) throw new ArgumentNullException(nameof(logWriter));

            this.logWriter = logWriter;

            this.version = version;
            ParseVersion(version);
        }

        public bool ShouldCreateBackup { get; set; }
        public string ConstantVersion { get { return constantVersion; } }


        public VersionChangerBase[] VersionChangers
        {
            get { return versionChangers; }
        }

        private VersionChangerBase CreateVersionChanger(string newVersion)
        {
            if (string.IsNullOrEmpty(newVersion)) return null;

            if (newVersion == "*") return new DummyVersionChanger();

            int intVersion;
            if (!int.TryParse(newVersion, out intVersion))
                return null;

            switch (newVersion[0])
            {
                case '+':
                case '-':
                    return new IncrementVersionChanger(intVersion);
                default:
                    return new ConstantVersionChanger(intVersion);
            }

        }

        protected bool ParseVersion(string version)
        {
            string[] parts = version.Split('.');
            if (parts.Length > 4) return false;

            VersionChangerBase[] changers = new VersionChangerBase[parts.Length];

            bool constant = true;

            for (int i = 0; i < parts.Length; i++)
            {
                VersionChangerBase changer = CreateVersionChanger(parts[i]);
                constant &= changer is ConstantVersionChanger;

                if (changer == null) return false;
                changers[i] = changer;
            }

            if (constant)
            {
                foreach (ConstantVersionChanger changer in changers)
                    constantVersion += changer.NewVersion.ToString() + ".";
                constantVersion = constantVersion.Substring(0, constantVersion.Length - 1);
            }
            else
                constantVersion = "";
            versionChangers = changers;
            return true;
        }


        protected void WriteLine(string format, params object[] args)
        {
            logWriter.WriteLine(format, args);
        }

        public string ProcessAssemblyInfo(string fileName)
        {
            WriteLine("Processing File {0}", fileName);
            string newFile = Path.Combine(Path.GetDirectoryName(fileName), "AssemblyInfo.new");

            bool versionFound = false;
            string newVersion = string.Empty;

            using (StreamReader reader = new StreamReader(fileName, Encoding.Default))
            {

                using (StreamWriter writer = new StreamWriter(newFile, false, Encoding.Default))
                {

                    const string versionString = "[assembly: AssemblyVersion(\"";


                    while (!reader.EndOfStream)
                    {
                        string line = reader.ReadLine();
                        if (line.StartsWith(versionString, StringComparison.Ordinal) && !versionFound)
                        {
                            int versionEnd = line.IndexOf("\"", versionString.Length, StringComparison.Ordinal);
                            if (versionEnd >= 0)
                            {
                                string version = line.Substring(versionString.Length, versionEnd - versionString.Length);
                                newVersion = ChangeVersion(version);

                                line = string.Format("{0}{1}\")]", versionString, newVersion);


                                versionFound = true;
                            }
                        }
                        writer.WriteLine(line);
                    }

                }
            }

            if (!versionFound)
            {
                File.Delete(newFile);
                throw new InvalidOperationException("Assembly Version Attribute not found.");
            }

            if (ShouldCreateBackup)
            {
                try
                {
                    string backupFile = Path.Combine(Path.GetDirectoryName(fileName), Path.GetFileNameWithoutExtension(fileName)) + ".bak";
                    if (File.Exists(backupFile)) File.Delete(backupFile);
                    File.Move(fileName, backupFile);
                }
                catch (Exception ex)
                {
                    throw new InvalidOperationException("Cannot create backup file", ex);
                }
            }

            try
            {
                File.Delete(fileName);
                File.Move(newFile, fileName);
            }
            catch (Exception ex)
            {
                try
                {
                    File.Delete(newFile);
                }
                catch (IOException) { }

                throw new InvalidOperationException("Cannot update file", ex);
            }

            if (!string.IsNullOrEmpty(newVersion))
            {
                WriteLine("Version Changed to {0}", newVersion);
                return newVersion;
            }

            return null;
        }

        public string ChangeVersion(string version)
        {
            string newVersion;
            string[] parts = version.Split('.');


            if (string.IsNullOrWhiteSpace(ConstantVersion))
            {
                for (int i = 0; i < parts.Length && i < VersionChangers.Length; i++)
                    parts[i] = VersionChangers[i].Change(parts[i]);

                newVersion = string.Join(".", parts);
            }
            else
                newVersion = ConstantVersion;

            return newVersion;
        }
    }

}
