// Copyright (c) Microsoft Corporation.  All rights reserved.
namespace BenchLib
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Text;
    using Microsoft.WindowsAzure.StorageClient;

    public class ExperimentRequest : TableServiceEntity
    {
        public enum State
        {
            Queued,
            Running,
            Completed,
        }

        public string ExperimentName { get { return PartitionKey; } }
        public int RequestedIterations { get; set; }
        public State CurrentState { get; set; }

        public ExperimentRequest(Guid experimentId, string experimentName)
            : base(experimentName, experimentId.ToString())
        {
        }
    }
}
