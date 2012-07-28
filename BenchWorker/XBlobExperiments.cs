// Copyright (c) Microsoft Corporation.  All rights reserved.
namespace WorkerRole1
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

    class XBlobExperiments
    {
        //readonly CloudBlobClient client;
        List<XStoreExperiment> experiments = new List<XStoreExperiment>();
        readonly RandomTestData testDataSource;
        List<string> testFiles = new List<string>();

        public string RoleName { get; private set; }

        public XBlobExperiments(CloudBlobClient client, int numOfThreads, int requestedIterations, int minSizeOfBlob)
        {
            //this.client = client;
            this.testDataSource = new RandomTestData(minSizeOfBlob);
            RoleName = RoleEnvironment.CurrentRoleInstance.Id;
            for (int i = 0; i < numOfThreads; i++)
            {
                this.experiments.Add(new UploadBlobs(RoleName, client, requestedIterations, minSizeOfBlob, this.testDataSource));
            }
        }

        public IEnumerable<ExperimentResult> Run()
        {
            List<WaitHandle> waitHandles = new List<WaitHandle>();
            foreach (XStoreExperiment experiment in this.experiments)
            {
                experiment.Start();
                waitHandles.Add(experiment.WaitHandle);
            }
            bool finishedInTime = WaitHandle.WaitAll(waitHandles.ToArray(), TimeSpan.FromMinutes(60));
            List<ExperimentResult> results = new List<ExperimentResult>();
            foreach (XStoreExperiment experiment in this.experiments)
            {
                results.Add(experiment.Result);
            }
            return results;
        }

        class UploadBlobs : XStoreExperiment
        {
            readonly CloudBlobContainer container;
            readonly RandomTestData testDataSource;

            public UploadBlobs(string roleName, CloudBlobClient client, int requestedIterations, int minSizeOfBlob, RandomTestData testDataSource)
                : base(roleName, client, "Upload blobs", requestedIterations)
            {
                this.container = this.client.GetContainerReference(Path.GetFileNameWithoutExtension(Path.GetRandomFileName()).ToLowerInvariant());
                this.testDataSource = testDataSource;
            }

            protected override void PrepareExperiment()
            {
                this.container.CreateIfNotExist(requestOptions);
            }

            protected override double RunSingleIteration(int currentIteration)
            {
                CloudBlob blob = this.container.GetBlobReference(Path.GetRandomFileName());
                byte[] testData = this.testDataSource.GetData();
                blob.Properties.ContentType = "application/octet";
                blob.UploadByteArray(testData, requestOptions);
                return testData.Length;
            }

            protected override void CleanupExperiment()
            {
                this.container.Delete(requestOptions);
            }
        }

        class RandomTestData
        {
            const int headroom = 10000;
            readonly int size;
            readonly byte[] randomData;
            Random randomizer = new Random((int)DateTime.Now.Ticks & 0x0000FFFF);
            
            public RandomTestData(int minSize)
            {
                this.size = minSize;
                int totalLength = minSize + headroom;
                this.randomData = new byte[totalLength];
                for (int i = 0; i < totalLength; i++)
                {
                    this.randomData[i] = (byte) this.randomizer.Next(0x100);
                }
            }

            public byte[] GetData()
            {
                byte[] data = new byte[this.size];
                Array.Copy(this.randomData, this.randomizer.Next(headroom), data, 0, this.size);
                return data;
            }
        }
    }
}
