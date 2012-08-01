// Copyright (c) Microsoft Corporation.  All rights reserved.
namespace BenchWorker
{
    using System.IO;
    using BenchLib;
    using Microsoft.WindowsAzure.StorageClient;

    class UploadBlobsExperiment : XStoreExperiment
    {
        readonly CloudBlobContainer container;
        readonly RandomBlobData blobDatasource;

        public UploadBlobsExperiment(string instanceId, CloudBlobClient client, ExperimentRequest request, RandomBlobData blobDatasource)
            : base(request.ExperimentId, client, "Upload blobs", request.RequestedIterations, instanceId)
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
