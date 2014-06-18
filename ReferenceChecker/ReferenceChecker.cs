using System;
using System.Collections.Generic;
using System.Text;
using System.Reflection;
using System.IO;

namespace CMS.Library.Addin
{
    class ReferenceChecker : MarshalByRefObject
    {
        private static string[] excludeNames = { "mscorlib", "System", "System.Drawing", "System.Windows.Forms" , "System.Core", "System.Xml", "System.Net", 
                                                   "System.Windows.Browser","System.ServiceModel.Web", "System.ServiceModel", "System.Runtime.Serialization",
                                               "System.Data.DataSetExtensions","System.Xml.Linq"};
        private string rootPath;
        private AssemblyCollection excludedAssemblies = new AssemblyCollection();
        private MissingAssemblyCollection reportedAssemblies = new MissingAssemblyCollection();
        private const string frameworkLibResourceName = "CMS.Library.Addin.FrameworkAssemblies.txt";
        public ReferenceChecker()
        {
            Assembly assembly = Assembly.GetExecutingAssembly();
            using (Stream stream = assembly.GetManifestResourceStream(frameworkLibResourceName))
            {
                using (StreamReader reader = new StreamReader(stream))
                {
                    while (!reader.EndOfStream)
                    {
                        string line = reader.ReadLine().Trim();
                        if (!string.IsNullOrEmpty(line))
                            excludedAssemblies.Add(new AssemblyName(line));
                    }
                }
            }
        }

        internal string RootPath
        {
            get { return rootPath; }
            set { rootPath = value; }
        }
        private AssemblyCollection ExcludedAssemblies { get { return excludedAssemblies; } }
        private MissingAssemblyCollection ReportedAssemblies { get { return reportedAssemblies; } }

        public MissingReference[] GetReportedAssemblies()
        {
            MissingReference[] names = new MissingReference[ReportedAssemblies.Count];
            ReportedAssemblies.CopyTo(names, 0);
            return names;
        }
        internal void Check()
        {
            AssemblyCollection assemblies = GetAssemblies(RootPath, true);
            foreach (AssemblyName assemblyName in assemblies)
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
                        CheckReferences(assemblies, assembly);
                }
            }

        }
        private static bool IsExcluded(AssemblyName name)
        {
            return Array.Find(excludeNames, delegate(string s) { return name.Name == s; }) != null;
        }

        private void CheckReferences(AssemblyCollection assemblies, Assembly assembly)
        {
            AssemblyName[] references = assembly.GetReferencedAssemblies();
            foreach (AssemblyName name in references)
            {
                //Wenn die Referenz nirgendwo zu finden ist, dann ausgeben.
                if (assemblies.Find(name) == null && excludedAssemblies.FindLazy(name) == null && !IsExcluded(name) && reportedAssemblies.Find(name) == null)
                {
                    reportedAssemblies.Add(new MissingReference() { MissingAssembly = name, ReferencesBy = assembly.GetName() });
                }
            }
        }

        /// <summary>
        /// Gibt alle Assemblynamen aus dem angegebenem Verzeichnis zurück.
        /// </summary>
        /// <param name="path">Zu durchsuchende Verzeichnis</param>
        /// <param name="recursive">Gibt an, ob Unterverzeichnisse durchsucht werden sollen</param>
        /// <returns></returns>
        private AssemblyCollection GetAssemblies(string path, bool recursive)
        {

            AssemblyCollection assemblies = new AssemblyCollection();
            AddAssemblies(path, assemblies, recursive);
            return assemblies;
        }

        /// <summary>
        /// Fügt alle Assemblynamen aus dem angegebenem Verzeichnis der angegebenen Collection hinzu.
        /// </summary>
        /// <param name="path">Zu durchsuchende Verzeichnis</param>
        /// <param name="assemblies">Collection, die gefüllt wird</param>
        /// <param name="recursive">Gibt an, ob Unterverzeichnisse durchsucht werden sollen</param>
        private void AddAssemblies(string path, AssemblyCollection assemblies, bool recursive)
        {
            //Dateinamen holen
            string[] files = Directory.GetFiles(path);
            foreach (string fileName in files)
            {
                try
                {
                    //Assemblynamen erstellen und hinzufügen
                    AssemblyName asm = AssemblyName.GetAssemblyName(fileName);
                    assemblies.Add(asm);
                }
                //Exceptions abfangen, fall die Datei keine Assembly ist.
                catch (BadImageFormatException) { }
                catch (FileLoadException) { }
                catch (FileNotFoundException) { }
            }

            //Wenn recusive=true, Unterverzeichnisse druchsuchen
            if (recursive)
            {
                string[] directories = Directory.GetDirectories(path);
                foreach (string dir in directories)
                    AddAssemblies(dir, assemblies, recursive);
            }
        }

        internal void AddExclude(AssemblyName assemblyName)
        {
            ExcludedAssemblies.Add(assemblyName);
        }
    }
}

