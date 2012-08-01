// Copyright (c) Microsoft Corporation.  All rights reserved.
namespace BenchLib
{
    using System;
    using Microsoft.WindowsAzure.StorageClient;

    public class ExperimentStatus : TableServiceEntity
    {
        public string InstanceId { get { return PartitionKey; } }
        public DateTime Started { get; set; }
        ExperimentResult Result { get; set; }

        public ExperimentStatus(Guid experimentId, string instanceId)
            : base(instanceId, experimentId.ToString())
        {
        }
    }
}
