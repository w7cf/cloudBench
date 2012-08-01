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

    class ExperimentContainer
    {
        readonly ExperimentRequest ExperimentRequest;
        readonly RandomBlobData testDataSource;
        List<Experiment> experiments = new List<Experiment>();
        List<string> testFiles = new List<string>();

        public string RoleName { get; private set; }

        public ExperimentContainer(ExperimentRequest request, IExperimentFactory experimentFactory)
        {
            this.ExperimentRequest = request;
            this.testDataSource = new RandomBlobData(request.MinDataSize);
            RoleName = RoleEnvironment.CurrentRoleInstance.Id;
            for (int i = 0; i < request.NumberOfThreads; i++)
            {
                this.experiments.Add(experimentFactory.CreateExperiment(request));
            }
        }

        public IEnumerable<ExperimentResult> Run()
        {
            // TODO: add warmup stage

            List<WaitHandle> waitHandles = new List<WaitHandle>();

            BenchmarkMonitor monitor = new BenchmarkMonitor();
            monitor.Start();

            foreach (Experiment experiment in this.experiments)
            {
                experiment.Start();
                waitHandles.Add(experiment.WaitHandle);
            }
            bool finishedInTime = WaitHandle.WaitAll(waitHandles.ToArray(), TimeSpan.FromMinutes(60));
            monitor.Stop();
            
            List<ExperimentResult> results = new List<ExperimentResult>();
            foreach (Experiment experiment in this.experiments)
            {
                results.Add(experiment.Result);
            }
            return results;
        }

    }
}
