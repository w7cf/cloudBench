// Copyright (c) Microsoft Corporation.  All rights reserved.
namespace BenchLib
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Text;
    using System.Threading;

    public abstract class AsyncTask
    {
        readonly ManualResetEvent requestCancel = new ManualResetEvent(false);
        readonly ManualResetEvent hasFinished = new ManualResetEvent(false);
        bool isRunning;

        public bool IsRunning { get { return OnIsRunning; } }
        public WaitHandle HasFinishedEvent { get { return this.hasFinished; } }
        
        protected bool OnIsRunning { get { return this.isRunning; } }

        public void Start()
        {
            OnStart();
        }

        public void Cancel()
        {
            OnCancel();
        }

        public bool WaitForCompletion()
        {
            return WaitForCompletion(TimeSpan.FromMilliseconds(-1));    // -1 msec -> infinite time out
        }

        public bool WaitForCompletion(TimeSpan timeout)
        {
            return OnWaitForCompletion(timeout);
        }

        protected virtual void OnStart()
        {
            this.requestCancel.Reset();
            this.isRunning = true;
        }

        protected virtual void OnCancel()
        {
            if (!IsRunning)
            {
                return;
            }
            this.requestCancel.Set();
        }

        protected virtual bool OnWaitForCompletion(TimeSpan timeout)
        {
            if (!IsRunning)
            {
                return false;
            }
            bool finishedInTime = this.hasFinished.WaitOne(timeout);
            return finishedInTime;
        }

        protected void OnTaskFinished()
        {
            this.isRunning = false;
            this.hasFinished.Set();
        }

        protected bool WaitForCancelEvent(TimeSpan timeout)
        {
            return this.requestCancel.WaitOne(timeout);
        }
    }
}
