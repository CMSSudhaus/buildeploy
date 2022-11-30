using System;
using System.Reflection;

namespace Cms.Buildeploy.ReferenceCheck
{
    class AssemblyRedirect
    {
        internal AssemblyRedirect(Version redirectFromLowRange, Version redirectFromHighRange, AssemblyName redirectTo)
        {
            RedirectFromLowRange = redirectFromLowRange ?? throw new ArgumentNullException(nameof(redirectFromLowRange));
            RedirectFromHighRange = redirectFromHighRange ?? throw new ArgumentNullException(nameof(redirectFromHighRange));
            RedirectTo = redirectTo ?? throw new ArgumentNullException(nameof(redirectTo));
        }

        public Version RedirectFromLowRange { get; }
        public Version RedirectFromHighRange { get; }
        public AssemblyName RedirectTo { get; }
    }

}