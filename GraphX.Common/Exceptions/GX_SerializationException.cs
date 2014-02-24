using System;

namespace GraphX
{
    public sealed class GX_SerializationException: Exception
    {
        public GX_SerializationException(string text)
            : base(text)
        {
        }
    }
}
