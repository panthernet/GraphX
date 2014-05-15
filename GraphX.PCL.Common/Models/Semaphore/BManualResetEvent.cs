using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;

namespace GraphX
{
    public class BManualResetEvent : BWaitHandle, IDisposable
    {
        ManualResetEvent _mre;

        public BManualResetEvent(bool initialState)
        {
            _mre = new ManualResetEvent(initialState);
        }

        // Summary:
        //     Sets the state of the event to non-signaled, which causes threads to block.
        //
        // Returns:
        //     true if the operation succeeds; otherwise, false.
        public bool Reset()
        {
            return _mre.Reset();
        }
        //
        // Summary:
        //     Sets the state of the event to signaled, which allows one or more waiting
        //     threads to proceed.
        //
        // Returns:
        //     true if the operation succeeds; otherwise, false.
        public bool Set()
        {
            return _mre.Set();
        }

        protected override void OnSuccessfullWait()
        {
            // nothing special needed
        }

        public override bool WaitOne()
        {
            return _mre.WaitOne();
        }

        public override bool WaitOne(TimeSpan timeout)
        {
            return _mre.WaitOne(timeout);
        }

        public override bool WaitOne(int millisecondsTimeout)
        {
            return _mre.WaitOne(millisecondsTimeout);
        }

        internal override WaitHandle WaitHandle
        {
            get { return _mre; }
        }

        public void Dispose()
        {
            if (_mre != null)
            {
                _mre.Dispose();
                _mre = null;
            }
        }
    }
}
