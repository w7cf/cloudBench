// Copyright (c) Microsoft Corporation.  All rights reserved.
namespace BenchLib
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Text;
    using Microsoft.WindowsAzure;
    //using Microsoft.WindowsAzure.Diagnostics;
    //using Microsoft.WindowsAzure.ServiceRuntime;
    using Microsoft.WindowsAzure.StorageClient;
    using System.IO;

    public abstract class XStoreExperiment : Experiment
    {
        protected readonly string instanceId;
        protected readonly CloudBlobClient client;
        protected readonly BlobRequestOptions requestOptions;
        public int TotalRetries { get; private set; }

        public XStoreExperiment(Guid experimentId, CloudBlobClient client, string title, int requestedIterations, string instanceId)
            : base(experimentId, title, requestedIterations, instanceId)
        {
            this.instanceId = instanceId;
            this.client = client;
            this.requestOptions = new BlobRequestOptions
            {
                RetryPolicy = TrackedPolicy,
                Timeout = TimeSpan.FromMinutes(5),
            };
        }

        ShouldRetry TrackedPolicy()
        {
            return RetryTracker;
        }

        bool RetryTracker(int retryCount, Exception lastException, out TimeSpan delay)
        {
            ++TotalRetries;
            delay = TimeSpan.FromSeconds(retryCount * 10);
            return retryCount < 5;
        }
    }
}
