// Copyright (c) Microsoft Corporation.  All rights reserved.
namespace BenchLib
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Text;
    using System.Diagnostics;
    using System.Globalization;
using System.Threading;

    public abstract class Experiment
    {
        readonly int requestedIterations;
        readonly ManualResetEvent requestCancel = new ManualResetEvent(false);
        readonly ManualResetEvent hasFinished = new ManualResetEvent(false);
        string title;
        ExperimentResult result;
        bool isRunning;
        Thread thread;

        public int RequestedIterations { get { return this.requestedIterations; } }
        public string Title { get { return this.title; } }

        public ExperimentResult Result { get { return this.result; } }
        public bool IsRunning { get { return this.isRunning; } }
        public WaitHandle WaitHandle { get { return this.hasFinished; } }


        public Experiment(string title, int requestedIterations)
        {
            this.title = title;
            this.requestedIterations = requestedIterations;
        }

        public void Start()
        {
            if (IsRunning)
            {
                throw new InvalidOperationException("An experiment is already running.");
            }
            this.requestCancel.Reset();
            this.thread = new Thread(RunCore);
            this.thread.Name = "Experiment: " + Title;
            this.thread.Start();
        }

        public void Cancel()
        {
            if (!IsRunning)
            {
                return;
            }
            this.requestCancel.Set();
        }

        public bool WaitForCompletion()
        {
            return WaitForCompletion(new TimeSpan(-1 * 10000));    // -1 msec -> infinite time out
        }

        public bool WaitForCompletion(TimeSpan timeout)
        {
            if (!IsRunning)
            {
                return false;
            }
            bool finishedInTime = this.hasFinished.WaitOne(timeout);
            this.thread = null;
            return finishedInTime;
        }

        void RunCore()
        {
            this.isRunning = true;
            Stopwatch totalDuration = new Stopwatch();
            Stopwatch netDuration = new Stopwatch();
            DateTime started = DateTime.UtcNow;
            int currentIteration = 0;
            double accumulatedMetric = 0;
            ExperimentResult result = null;
            try
            {
                PrepareExperiment();

                totalDuration.Start();
                for (currentIteration = 0; currentIteration < RequestedIterations; currentIteration++)
                {
                    PrepareIteration(currentIteration);
                    long startTicks = netDuration.ElapsedTicks;
                    netDuration.Start();
                    accumulatedMetric += RunSingleIteration(currentIteration);
                    netDuration.Stop();
                    long netTicks = netDuration.ElapsedTicks - startTicks;  // TODO: average/std dev netTicks
                    CleanupIteration(currentIteration);
                }
                totalDuration.Stop();
                CleanupExperiment();
                result = new ExperimentResult(Title, true, started, totalDuration.Elapsed, netDuration.Elapsed, currentIteration, accumulatedMetric, string.Empty);
            }
            catch (Exception ex)
            {
                netDuration.Stop();
                totalDuration.Stop();
                result = new ExperimentResult(Title, false, started, totalDuration.Elapsed, netDuration.Elapsed, currentIteration, accumulatedMetric, WriteException(ex));
            }
            finally
            {
                this.isRunning = true;
                this.result = result;
                this.hasFinished.Set();
            }
        }

        protected abstract double RunSingleIteration(int currentIteration);

        protected virtual void PrepareExperiment() { }

        protected virtual void PrepareIteration(int currentIteration) { }

        protected virtual void CleanupIteration(int currentIteration) { }

        protected virtual void CleanupExperiment() { }

        protected static string WriteException(Exception exception)
        {
            if (exception != null)
            {
                StringBuilder error = new StringBuilder();
                string stackTrace = exception.StackTrace;
                do
                {
                    error.AppendLine(string.Format(CultureInfo.CurrentCulture, "{0}: {1}", exception.GetType().Name, exception.Message));

                    exception = exception.InnerException;
                }
                while (exception != null);

                error.AppendLine("stack trace:" + stackTrace);
                return error.ToString();
            }
            return string.Empty;
        }
    }
}
