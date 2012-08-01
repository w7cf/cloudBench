// Copyright (c) Microsoft Corporation.  All rights reserved.
namespace BenchWorker
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Text;
    using Microsoft.WindowsAzure;
    using System.Threading;
    using BenchLib;
    using System.Diagnostics;
    using Microsoft.WindowsAzure.ServiceRuntime;
    using Microsoft.WindowsAzure.StorageClient;

    class CloudExperimentRepo : IExperimentRepo
    {
        readonly CloudStorageAccount reportAccount;

        List<ExperimentRequest> experiments = new List<ExperimentRequest>();
        Dictionary<string, ExperimentStatus> statusList = new Dictionary<string, ExperimentStatus>();
        List<ExperimentResult> results = new List<ExperimentResult>();

        public CloudExperimentRepo()
        {
            this.experiments.Add(
                new ExperimentRequest(Guid.NewGuid(), "UploadBlobsExperiment")
                {
                    NumberOfThreads = 1,
                    RequestedIterations = 10,
                    MinDataSize = 100 * 1024,
                });
            this.reportAccount = CloudStorageAccount.Parse(RoleEnvironment.GetConfigurationSettingValue("ReportStorage"));
        }

        public IEnumerable<ExperimentRequest> GetPendingRequests()
        {
            return this.experiments;
        }

        public ExperimentRequest GetRequest(Guid experimentId)
        {
            AssertId(experimentId);
            return this.experiments[0];
        }

        public void AddStatusForInstance(Guid experimentId, ExperimentStatus status, string instanceId)
        {
            AssertId(experimentId);
            this.statusList.Add(instanceId, status);
        }

        public void UpdateRequestState(Guid experimentId, ExperimentRequest.State state)
        {
            AssertId(experimentId);
            this.experiments[0].CurrentState = state;
        }

        public IEnumerable<ExperimentStatus> GetStatusFor(Guid experimentId)
        {
            AssertId(experimentId);
            return this.statusList.Values;
        }

        public void AddResults(Guid experimentId, IEnumerable<ExperimentResult> results)
        {
            AssertId(experimentId);
            this.results.AddRange(results);
        }

        public IEnumerable<ExperimentResult> GetResults(Guid experimentId)
        {
            AssertId(experimentId);
            return this.results;
        }

        void AssertId(Guid experimentId)
        {
            if (this.experiments[0].ExperimentId != experimentId)
            {
                throw new ArgumentException("unknown experimentId");
            }
        }
    }
}
