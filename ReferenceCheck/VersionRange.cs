using System;

namespace Cms.Buildeploy.ReferenceCheck
{
    internal class VersionRange
    {

        public VersionRange(Version low, Version high)
        {
            Low = low;
            High = high;
        }

        public Version Low { get; }
        public Version High { get; }


    }
}