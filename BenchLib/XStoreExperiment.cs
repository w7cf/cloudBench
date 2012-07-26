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

    public class XStoreExperiment : Experiment
    {
        protected readonly string roleName;
        protected readonly CloudBlobClient client;
        protected readonly BlobRequestOptions requestOptions;
        public int TotalRetries { get; private set; }

        public XStoreExperiment(string roleName, CloudBlobClient client, string title, int requestedIterations)
            : base(title, requestedIterations)
        {
            this.roleName = roleName;
            this.client = client;
            this.requestOptions = new BlobRequestOptions
            {
                RetryPolicy = TrackedPolicy,
            };
        }

        protected override double RunSingleIteration(int currentIteration)
        {
            throw new NotImplementedException();
        }

        ShouldRetry TrackedPolicy()
        {
            return RetryTracker;
        }

        bool RetryTracker(int retryCount, Exception lastException, out TimeSpan delay)
        {
            ++TotalRetries;
            delay = new TimeSpan(0, 0, retryCount * 10);
            return retryCount < 5;
        }
    }
}
