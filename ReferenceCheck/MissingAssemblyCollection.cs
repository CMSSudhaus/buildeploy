using System.Reflection;

namespace Cms.Buildeploy.ReferenceCheck
{
    class MissingAssemblyCollection : AssemblyCollectionBase<MissingReference>
    {
        protected override AssemblyName GetAssemblyName(MissingReference item)
        {
            return item.MissingAssembly;
        }
    }
}