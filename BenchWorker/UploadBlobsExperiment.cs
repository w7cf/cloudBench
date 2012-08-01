// Copyright (c) Microsoft Corporation.  All rights reserved.
namespace BenchWorker
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Text;
    using BenchLib;
    using Microsoft.WindowsAzure;
    using Microsoft.WindowsAzure.Diagnostics;
    using Microsoft.WindowsAzure.ServiceRuntime;
    using Microsoft.WindowsAzure.StorageClient;
    using System.IO;
    using System.Threading;

    class UploadBlobsExperiment : XStoreExperiment
    {
        readonly CloudBlobContainer container;
        readonly RandomBlobData blobDatasource;

        public UploadBlobsExperiment(string roleName, CloudBlobClient client, ExperimentRequest request, RandomBlobData blobDatasource)
            : base(request.ExperimentId, roleName, client, "Upload blobs", request.RequestedIterations)
        {
            this.container = this.client.GetContainerReference(Path.GetFileNameWithoutExtension(Path.GetRandomFileName()).ToLowerInvariant());
            this.blobDatasource = blobDatasource;
        }

        protected override void PrepareExperiment()
        {
            this.container.CreateIfNotExist(requestOptions);
        }

        protected override double RunSingleIteration(int currentIteration)
        {
            CloudBlob blob = this.container.GetBlobReference(Path.GetRandomFileName());
            byte[] testData = this.blobDatasource.GetBlob();
            blob.Properties.ContentType = "application/octet";
            blob.UploadByteArray(testData, requestOptions);
            return testData.Length;
        }

        protected override void CleanupExperiment()
        {
            this.container.Delete(requestOptions);
        }
    }
}
