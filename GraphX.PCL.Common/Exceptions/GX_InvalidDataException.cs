using System;

namespace GraphX.PCL.Common.Exceptions
{
    public sealed class GX_InvalidDataException: Exception
    {
        public GX_InvalidDataException(string text):base(text)
        {
        }
    }
}
