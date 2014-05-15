using System;
using System.Collections.Generic;
using System.Text;
using System.Threading;

namespace GustavoAlgorithms
{
    /*public static class HighResolutionTime
    {
        #region Win32APIs
        [System.Runtime.InteropServices.DllImport("Kernel32.dll")]
        private static extern bool QueryPerformanceCounter(out long perfcount);

        [System.Runtime.InteropServices.DllImport("Kernel32.dll")]
        private static extern bool QueryPerformanceFrequency(out long freq);
        #endregion

        #region Variables Declaration
        private static long mStartCounter;
        private static long mFrequency;
        #endregion

        #region Constuctors
        static HighResolutionTime()
        {
            QueryPerformanceFrequency(out mFrequency);
        }
        #endregion

        #region Methods
        public static void Start()
        {
            QueryPerformanceCounter(out mStartCounter);
        }

        public static double GetTime()
        {
            long endCounter;
            QueryPerformanceCounter(out endCounter);
            long elapsed = endCounter - mStartCounter;
            return (double) elapsed / mFrequency;
        }
        #endregion
    }*/
}

