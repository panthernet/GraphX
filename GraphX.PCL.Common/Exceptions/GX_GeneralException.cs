using System;

namespace GraphX
{
    public sealed class GX_GeneralException: Exception
    {
        public GX_GeneralException(string text)
            : base(text)
        {
        }
    }
}
