using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Build.Framework;
using System.Reflection;
using Microsoft.Build.Utilities;
using Buildeploy.ReferenceCheck;

namespace Buildeploy.Tasks
{
    public class CheckReferences : Task
    {
        public string Path { get; set; }



        public override bool Execute()
        {

            //Neue AppDomain erstellen, um das Sperren der Assemblies zu vermeiden
            AppDomainSetup setup = new AppDomainSetup();
            setup.ApplicationBase = System.IO.Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location);
            AppDomain domain = AppDomain.CreateDomain("RefCheckDomain", null, setup);

            try
            {

                string assembly = typeof(ReferenceChecker).Assembly.Location;
                string className = typeof(ReferenceChecker).FullName;

                Log.LogMessage("ReferenceChecker: Assembly: {0}, Class: {1}", assembly, className);
                ReferenceChecker checker = (ReferenceChecker)domain.CreateInstanceFromAndUnwrap(assembly, className);

                checker.RootPath = Path;

                if (Excludes != null)
                {
                    foreach (ITaskItem item in Excludes)
                        checker.AddExclude(new AssemblyName(item.ItemSpec));
                }

                checker.Check();
                MissingReference[] reportedAssemblies = checker.GetReportedAssemblies();

                if (reportedAssemblies != null && reportedAssemblies.Length > 0)
                {
                    Log.LogMessage("Following references were not found:");
                    foreach (var name in reportedAssemblies)
                    {
                        Log.LogError("[MISSING REFERENCE ({2})] {0} referenced by {1}", name.MissingAssembly.FullName, name.ReferencesBy.Name, MessageInfo);
                    }

                    
                    Log.LogMessage(MessageImportance.High, "Copy & Paste friendly list of assemblies: \r\n{0}",
                        string.Join(";\r\n", reportedAssemblies.Select(ra => ra.MissingAssembly.Name + ".dll")));

                    return false;
                }
                else
                    return true;
            }
            finally
            {
                AppDomain.Unload(domain);
            }
        }

        public ITaskItem[] Excludes { get; set; }

        [Required]
        public string MessageInfo { get; set; }
    }

}
