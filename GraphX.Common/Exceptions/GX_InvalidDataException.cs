using System;

namespace GraphX
{
    public sealed class GX_InvalidDataException: Exception
    {
        public GX_InvalidDataException(string text):base(text)
        {
        }
    }
}
