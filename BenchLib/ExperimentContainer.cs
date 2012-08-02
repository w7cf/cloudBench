// Copyright (c) Microsoft Corporation.  All rights reserved.
namespace BenchLib
{
    using System;
    using System.Collections.Generic;
    using System.Threading;
    using BenchLib;

    public class ExperimentContainer
    {
        readonly string InstanceId;
        readonly ExperimentRequest ExperimentRequest;
        readonly RandomBlobData testDataSource;
        List<Experiment> experiments = new List<Experiment>();
        List<string> testFiles = new List<string>();

        public ExperimentContainer(string instanceId, ExperimentRequest request, IExperimentFactory experimentFactory)
        {
            this.ExperimentRequest = request;
            this.testDataSource = new RandomBlobData(request.MinDataSize);
            InstanceId = instanceId;
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
                waitHandles.Add(experiment.HasFinishedEvent);
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
