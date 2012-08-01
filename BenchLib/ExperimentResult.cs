// Copyright (c) Microsoft Corporation.  All rights reserved.
namespace BenchLib
{
    using System;
    using Microsoft.WindowsAzure.StorageClient;

    public class ExperimentResult : TableServiceEntity
    {
        public string InstanceId { get { return PartitionKey; } }
        public string Title { get; set; }
        public bool Success { get; set; }
        public DateTime Started { get; set; }
        public TimeSpan TotalDuration { get; set; }
        public TimeSpan NetDuration { get; set; }
        public int CompletedIterations { get; set; }
        public double AccumulatedMetric { get; set; }
        public string Error { get; set; }

        public ExperimentResult(Guid experimentId, string instanceId)
            : base(instanceId, experimentId.ToString())
        {
        }
    }
}
