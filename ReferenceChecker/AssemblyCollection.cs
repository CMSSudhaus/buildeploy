using System.Reflection;

namespace CMS.Library.Addin
{
    class AssemblyCollection : AssemblyCollectionBase<AssemblyName>
    {
        protected override AssemblyName GetAssemblyName(AssemblyName item)
        {
            return item;
        }
    }
}