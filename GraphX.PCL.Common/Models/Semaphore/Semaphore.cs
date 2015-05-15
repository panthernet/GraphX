using System;
using System.Diagnostics;
using System.Threading;

namespace GraphX.PCL.Common.Models.Semaphore
{
    public class Semaphore : BWaitHandle, IDisposable
    {
        int _count;
        readonly int _maxCount = int.MaxValue;
        EventWaitHandle _ewh;

        public Semaphore()
        {
            _ewh = new AutoResetEvent(false);
        }

        public Semaphore(int initialCount, int maxCount)
        {
            if (initialCount < 0)
                throw new ArgumentException("Semaphore value should be >= 0.");
            if (initialCount >= maxCount)
                throw new ArgumentException();

            _count = initialCount;
            _maxCount = maxCount;
            _ewh = new AutoResetEvent(_count > 0);
        }

        protected override void OnSuccessfullWait()
        {
            var res = Interlocked.Decrement(ref _count);
            Debug.Assert(res >= 0, "The decremented value should be always >= 0.");
            if (res > 0)
                _ewh.Set();
        }

        public override bool WaitOne()
        {
            _ewh.WaitOne();
            OnSuccessfullWait();
            return true;
        }

        public override bool WaitOne(TimeSpan timeout)
        {
            if (_ewh.WaitOne(timeout))
            {
                OnSuccessfullWait();
                return true;
            }
            else
                return false;
        }

        public override bool WaitOne(int millisecondsTimeout)
        {
            if (_ewh.WaitOne(millisecondsTimeout))
            {
                OnSuccessfullWait();
                return true;
            }
            else
                return false;
        }

        public void Release()
        {
            var res = Interlocked.Increment(ref _count);
            if (res > _maxCount)
                throw new ArgumentException("The value of Semaphore is bigger than predefined maxValue.");

            if (res == 1)
                _ewh.Set();
        }

        public void Dispose()
        {
            if (_ewh != null)
            {
                _ewh.Dispose();
                _ewh = null;
            }
        }

        internal override WaitHandle WaitHandle
        {
            get { return _ewh; }
        }
    }
}
