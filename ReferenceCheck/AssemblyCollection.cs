using System;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Xml;
using System.Xml.Linq;

namespace Cms.Buildeploy.ReferenceCheck
{
    class AssemblyCollection : AssemblyCollectionBase<AssemblyName>
    {
        protected override AssemblyName GetAssemblyName(AssemblyName item)
        {
            return item;
        }
    }

}