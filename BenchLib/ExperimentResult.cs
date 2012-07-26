// Copyright (c) Microsoft Corporation.  All rights reserved.
namespace BenchLib
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Text;

    public class ExperimentResult
    {
        readonly string title;
        readonly bool success;
        readonly DateTime started;
        readonly TimeSpan totalDuration;
        readonly TimeSpan netDuration;
        readonly int completedIterations;
        readonly double accumulatedMetric;
        readonly string error;

        public string Title { get { return this.title; } }
        public bool Success { get { return this.success; } }
        public DateTime Started { get { return this.started; } }
        public TimeSpan TotalDuration { get { return this.totalDuration; } }
        public TimeSpan NetDuration { get { return this.netDuration; } }
        public int CompletedIterations { get { return this.completedIterations; } }
        public double AccumulatedMetric { get { return this.accumulatedMetric; } }
        public string Error { get { return this.error; } }

        public ExperimentResult(string title, bool success, DateTime started, TimeSpan totalDuration, TimeSpan netDuration, int completedIterations, double accumulatedMetric, string error)
        {
            this.title = title;
            this.success = success;
            this.started = started;
            this.totalDuration = totalDuration;
            this.netDuration = netDuration;
            this.completedIterations = completedIterations;
            this.accumulatedMetric = accumulatedMetric;
            this.error = error;
        }

    }
}
