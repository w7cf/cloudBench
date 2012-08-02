// Copyright (c) Microsoft Corporation.  All rights reserved.
namespace BenchLib
{
    using System;
    using System.Diagnostics;
    using System.Globalization;
    using System.Text;
    using System.Threading;

    public abstract class Experiment : AsyncTask
    {
        readonly Guid experimentId;
        readonly int requestedIterations;
        readonly string instanceId;
        string title;
        ExperimentResult result;
        Thread thread;

        public int RequestedIterations { get { return this.requestedIterations; } }
        public string Title { get { return this.title; } }

        public ExperimentResult Result { get { return this.result; } }

        public Experiment(Guid experimentId, string title, int requestedIterations, string instanceId)
        {
            this.experimentId = experimentId;
            this.title = title;
            this.requestedIterations = requestedIterations;
            this.instanceId = instanceId;
        }

        protected override void OnStart()
        {
            if (IsRunning)
            {
                throw new InvalidOperationException("An experiment is already running.");
            }
            this.thread = new Thread(RunCore);
            this.thread.Name = "Experiment: " + Title;
            this.thread.Start();
            base.OnStart();
        }

        protected override bool OnWaitForCompletion(TimeSpan timeout)
        {
            bool finishedInTime = base.OnWaitForCompletion(timeout);
            this.thread = null;
            return finishedInTime;
        }

        void RunCore()
        {
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
                result = new ExperimentResult(this.experimentId, this.instanceId) {
                    ThreadId = Thread.CurrentThread.ManagedThreadId,
                    Title = Title,
                    Success = true, 
                    Started = started, 
                    TotalDuration = totalDuration.Elapsed, 
                    NetDuration = netDuration.Elapsed,
                    CompletedIterations = currentIteration, 
                    AccumulatedMetric = accumulatedMetric, 
                    Error = string.Empty,
                };
            }
            catch (Exception ex)
            {
                netDuration.Stop();
                totalDuration.Stop();
                result = new ExperimentResult(this.experimentId, this.instanceId)
                {
                    ThreadId = Thread.CurrentThread.ManagedThreadId,
                    Title = Title,
                    Success = false,
                    Started = started,
                    TotalDuration = totalDuration.Elapsed,
                    NetDuration = netDuration.Elapsed,
                    CompletedIterations = currentIteration,
                    AccumulatedMetric = accumulatedMetric,
                    Error = WriteException(ex),
                };

            }
            finally
            {
                this.result = result;
                base.OnTaskFinished();
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
