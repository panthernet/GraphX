using System;

namespace GraphX.PCL.Common.Exceptions
{
    public sealed class GX_GeneralException: Exception
    {
        public GX_GeneralException(string text)
            : base(text)
        {
        }
    }
}
