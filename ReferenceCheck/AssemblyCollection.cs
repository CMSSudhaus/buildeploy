using System.Reflection;

namespace Buildeploy.ReferenceCheck
{
    class AssemblyCollection : AssemblyCollectionBase<AssemblyName>
    {
        protected override AssemblyName GetAssemblyName(AssemblyName item)
        {
            return item;
        }
    }
}