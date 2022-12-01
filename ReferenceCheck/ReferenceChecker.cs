using System;
using System.Collections.Generic;
using System.Text;
using System.Reflection;
using System.IO;
using System.Linq;

namespace Cms.Buildeploy.ReferenceCheck
{
    class ReferenceChecker : MarshalByRefObject
    {
        private static string[] excludeNames = { "mscorlib", "System", "System.Drawing", "System.Windows.Forms" , "System.Core", "System.Xml", "System.Net",
                                                   "System.Windows.Browser","System.ServiceModel.Web", "System.ServiceModel", "System.Runtime.Serialization",
                                               "System.Data.DataSetExtensions","System.Xml.Linq"};
        private string rootPath;
        private const string frameworkLibResourceName = "Cms.Buildeploy.FrameworkAssemblies.txt";
        private static IList<AssemblyName> frameworkAssemblyNames;

        public ReferenceChecker()
        {

        }


        public bool IgnoreVersions { get; set; }

        private AssemblyCollection Assemblies { get; } = new AssemblyCollection();

        private static IList<AssemblyName> FrameworkAssemblyNames
        {
            get
            {
                if (frameworkAssemblyNames == null)
                {

                    List<AssemblyName> names = new List<AssemblyName>();
                    Assembly assembly = Assembly.GetExecutingAssembly();

                    using (Stream stream = assembly.GetManifestResourceStream(frameworkLibResourceName))
                    {
                        using (StreamReader reader = new StreamReader(stream))
                        {
                            while (!reader.EndOfStream)
                            {
                                string line = reader.ReadLine().Trim();
                                if (!string.IsNullOrEmpty(line))
                                    names.Add(new AssemblyName(line));
                            }
                        }
                    }
                    frameworkAssemblyNames = names.AsReadOnly();
                }
                return frameworkAssemblyNames;
            }
        }
        internal static bool IsFrameworkAssembly(AssemblyName assemblyName)
        {
            return FrameworkAssemblyNames.Any(an => string.Equals(an.FullName, assemblyName.FullName, StringComparison.OrdinalIgnoreCase));
        }

        internal string RootPath
        {
            get { return rootPath; }
            set { rootPath = value; }
        }

        private AssemblyCollection excludedAssemblies;
        private AssemblyCollection ExcludedAssemblies
        {
            get
            {
                if (excludedAssemblies == null)
                {
                    excludedAssemblies = new AssemblyCollection();
                    foreach (var asssemblyName in FrameworkAssemblyNames)
                        excludedAssemblies.Add(PrepareAssemblyName(asssemblyName));
                }

                return excludedAssemblies;
            }
        }
        private MissingAssemblyCollection ReportedAssemblies { get; } = new MissingAssemblyCollection();

        public MissingReference[] GetReportedAssemblies()
        {
            MissingReference[] names = new MissingReference[ReportedAssemblies.Count];
            ReportedAssemblies.CopyTo(names, 0);
            return names;
        }
        internal void Check()
        {
            if (Assemblies.Count == 0) CollectAssemblies(RootPath, true);
            foreach (AssemblyName assemblyName in Assemblies)
            {
                if (ExcludedAssemblies.FindLazy(assemblyName) == null)
                {
                    Assembly assembly = null;
                    try
                    {
                        assembly = Assembly.ReflectionOnlyLoadFrom(assemblyName.EscapedCodeBase);
                    }
                    catch (FileLoadException)
                    {
                    }

                    if (assembly != null)
                        CheckReferences(assembly);
                }
            }

        }
        private static bool IsExcluded(AssemblyName name)
        {
            return Array.Find(excludeNames, delegate (string s) { return name.Name == s; }) != null;
        }

        private AssemblyName PrepareAssemblyName(AssemblyName assemblyName)
        {
            if (IgnoreVersions)
                assemblyName.Version = null;
            return assemblyName;
        }
        private void CheckReferences(Assembly assembly)
        {
                var references = assembly.GetReferencedAssemblies()
                .Select(PrepareAssemblyName)
                .ToList();

            foreach (AssemblyName name in references)
            {
                //Wenn die Referenz nirgendwo zu finden ist, dann ausgeben.
                if (Assemblies.Find(name) == null && ExcludedAssemblies.FindLazy(name) == null && !IsExcluded(name) && ReportedAssemblies.Find(name) == null)
                {
                    ReportedAssemblies.Add(new MissingReference() { MissingAssembly = name, ReferencesBy = assembly.GetName() });
                }
            }
        }

        /// <summary>
        /// Gibt alle Assemblynamen aus dem angegebenem Verzeichnis zurück.
        /// </summary>
        /// <param name="path">Zu durchsuchende Verzeichnis</param>
        /// <param name="recursive">Gibt an, ob Unterverzeichnisse durchsucht werden sollen</param>
        /// <returns></returns>
        private void CollectAssemblies(string path, bool recursive)
        {
            AddAssemblies(path, recursive);
        }

        /// <summary>
        /// Fügt alle Assemblynamen aus dem angegebenem Verzeichnis der angegebenen Collection hinzu.
        /// </summary>
        /// <param name="path">Zu durchsuchende Verzeichnis</param>
        /// <param name="assemblies">Collection, die gefüllt wird</param>
        /// <param name="recursive">Gibt an, ob Unterverzeichnisse durchsucht werden sollen</param>
        private void AddAssemblies(string path, bool recursive)
        {
            //Dateinamen holen
            string[] files = Directory.GetFiles(path);
            foreach (string fileName in files)
            {
                AddAssembly(fileName);
            }

            //Wenn recusive=true, Unterverzeichnisse druchsuchen
            if (recursive)
            {
                string[] directories = Directory.GetDirectories(path);
                foreach (string dir in directories)
                    AddAssemblies(dir, recursive);
            }
        }

        internal void AddAssembly(string fileName)
        {
            try
            {
                //Assemblynamen erstellen und hinzufügen
                AssemblyName asm = AssemblyName.GetAssemblyName(fileName);
                Assemblies.Add(PrepareAssemblyName(asm));
            }
            //Exceptions abfangen, fall die Datei keine Assembly ist.
            catch (BadImageFormatException) { }
            catch (FileLoadException) { }
            catch (FileNotFoundException) { }
        }

        internal void AddExclude(AssemblyName assemblyName)
        {
            ExcludedAssemblies.Add(PrepareAssemblyName(assemblyName));
        }

        internal void LoadConfiguration(string configurationFile)
        {
            Assemblies.ReadRedirectsFromConfigXmlString(File.ReadAllText(configurationFile));
        }
    }
}

