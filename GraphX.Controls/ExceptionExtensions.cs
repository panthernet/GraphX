using System;
using System.Reflection;

namespace GraphX.Controls
{
    internal static class ExceptionExtensions
    {
        internal static void PreserveStackTrace(this Exception exception)
        {
            // In .NET 4.5 and later this isn't needed... (yes, this is a brutal hack!)
            var preserveStackTrace = typeof(Exception).GetMethod(
                "InternalPreserveStackTrace",
                BindingFlags.Instance | BindingFlags.NonPublic);

            preserveStackTrace.Invoke(exception, null);
        }
    }
}
