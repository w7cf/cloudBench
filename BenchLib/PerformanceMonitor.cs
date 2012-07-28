// Copyright (c) Microsoft Corporation.  All rights reserved.
namespace BenchLib
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Text;
    using System.Diagnostics;
    using System.Threading;
    using System.Globalization;

    public class BenchmarkMonitor
    {
        List<PerfMetric> metrics = new List<PerfMetric>();
        Dictionary<string, RunningStatistics> results = new Dictionary<string, RunningStatistics>();
        Timer sampleInterval;

        public BenchmarkMonitor()
        {
            AddMetric("Processor", "% Processor Time", "_Total");
            AddMetric("Network Interface", "Bytes Sent/sec");
            AddMetric("Network Interface", "Bytes Received/sec");
            AddMetric("Memory", "Committed Bytes");
            AddMetric("Memory", "Available MBytes");
            AddMetric("Memory", "Page Faults/sec");
            AddMetric("Memory", "Page Reads/sec");
        }

        public bool IsRunning { get { return this.sampleInterval != null; } }

        public Dictionary<string, RunningStatistics> Results
        {
            get
            {
                if (IsRunning)
                {
                    return new Dictionary<string, RunningStatistics>(0);
                }
                else
                {
                    return this.results;
                }
            }
        }

        public void Start()
        {
            if (this.sampleInterval != null)
            {
                throw new InvalidOperationException("Monitor is already running, cannot start it twice.");
            }
            this.sampleInterval = new Timer(CapturePerfSample, null, TimeSpan.FromSeconds(1), TimeSpan.FromSeconds(2));
        }

        public void Stop()
        {
            if (this.sampleInterval != null)
            {
                this.sampleInterval.Dispose();
                this.sampleInterval = null;
                PopulateResults();
            }
        }

        void AddMetric(string categoryName, string counterName, string instanceName = null)
        {
            PerformanceCounterCategory category = new PerformanceCounterCategory(categoryName);
            if (category.CategoryType == PerformanceCounterCategoryType.MultiInstance && string.IsNullOrEmpty(instanceName))
            {
                foreach (string name in category.GetInstanceNames())
                {
                    this.metrics.Add(new PerfMetric(new PerformanceCounter(categoryName, counterName, name)));
                }
            }
            else
            {
                this.metrics.Add(new PerfMetric(new PerformanceCounter(categoryName, counterName, instanceName)));
            }
        }

        void CapturePerfSample(object state)
        {
            foreach (PerfMetric metric in this.metrics)
            {
                try
                {
                    CounterSample sample = metric.PerfCounter.NextSample();
                    metric.Stats.AddSample(metric.PerfCounter.NextValue());
                }
                catch (InvalidOperationException ex)
                {
                    Trace.TraceError(string.Format(CultureInfo.InvariantCulture, @"Perf counter {0}\{1}: {2}", metric.PerfCounter.CategoryName, metric.PerfCounter.CounterName, ex.Message));
                }
            }
        }

        void PopulateResults()
        {
            this.results.Clear();
            foreach (PerfMetric metric in this.metrics)
            {
                this.results.Add(GetMetricName(metric.PerfCounter), metric.Stats);
            }
        }

        string GetMetricName(PerformanceCounter perfCounter)
        {
            if (string.IsNullOrEmpty(perfCounter.InstanceName))
            {
                return string.Format(CultureInfo.InvariantCulture, @"{0}\{1}", perfCounter.CategoryName, perfCounter.CounterName);
            }
            else
            {
                return string.Format(CultureInfo.InvariantCulture, @"{0}({2})\{1}", perfCounter.CategoryName, perfCounter.CounterName, perfCounter.InstanceName);
            }
        }

        class PerfMetric
        {
            public PerfMetric(PerformanceCounter perfCounter)
            {
                this.PerfCounter = perfCounter;
                this.Stats = new RunningStatistics();
            }

            public readonly PerformanceCounter PerfCounter;
            public readonly RunningStatistics Stats;
        }
    }
}
