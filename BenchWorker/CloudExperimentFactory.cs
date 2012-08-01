// Copyright (c) Microsoft Corporation.  All rights reserved.
namespace BenchWorker
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Text;
    using BenchLib;
    using Microsoft.WindowsAzure;
    using Microsoft.WindowsAzure.ServiceRuntime;
    using Microsoft.WindowsAzure.StorageClient;

    class CloudExperimentFactory : IExperimentFactory
    {
        readonly CloudStorageAccount benchmarkAccount;
        Dictionary<string, Type> experiments = new Dictionary<string, Type>();
        RandomBlobData blobDatasource;

        public CloudExperimentFactory()
        {
            this.benchmarkAccount = CloudStorageAccount.Parse(RoleEnvironment.GetConfigurationSettingValue("BenchmarkStorage"));

            this.experiments.Add("UploadBlobsExperiment", typeof(UploadBlobsExperiment));
        }

        public Experiment CreateExperiment(ExperimentRequest request)
        {
            Type experiment = this.experiments[request.ExperimentName];
            return new UploadBlobsExperiment("", this.benchmarkAccount.CreateCloudBlobClient(), request, GetBlobDatasource(request.MinDataSize));
        }

        public IEnumerable<string> ExperimentNames
        {
            get { return this.experiments.Keys; }
        }

        RandomBlobData GetBlobDatasource(long minSize)
        {
            if (this.blobDatasource == null || this.blobDatasource.Size < minSize)
            {
                this.blobDatasource = new RandomBlobData(minSize);
            }
            return this.blobDatasource;
        }

    }
}
