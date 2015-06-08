using System;

namespace GraphX.PCL.Common.Exceptions
{
    public sealed class GX_ObsoleteException: Exception
    {
        public GX_ObsoleteException(string text)
            : base(text)
        {
        }
    }
}
