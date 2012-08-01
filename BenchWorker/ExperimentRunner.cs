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

    class ExperimentRunner
    {
        readonly IExperimentRepo ExperimentRepo;
        readonly IExperimentFactory ExperimentFactory;

        public ExperimentRunner(IExperimentRepo experimentRepo, IExperimentFactory experimentFactory)
        {
            ExperimentRepo = experimentRepo;
            ExperimentFactory = experimentFactory;
        }

        public void Start()
        {
            Thread runner = new Thread(DispatchExperiments);
            runner.Name = "ExperimentRunner";
            runner.Start();
            runner.Join();
        }

        void DispatchExperiments()
        {
            int count = 0;

            while (true)
            {
                Trace.TraceInformation("Start experiment batch '{0}' at {1}", count++, DateTime.UtcNow);

                ExperimentRequest request = ExperimentRepo.GetPendingRequests().First();

                ExperimentContainer experiment = new ExperimentContainer(request, ExperimentFactory);

                IEnumerable<ExperimentResult> results = experiment.Run();

                Trace.TraceInformation("Got results from experiment batch '{0}' at {1}", count++, DateTime.UtcNow);

                ExperimentRepo.AddResults(request.ExperimentId, results);
            }
        }
    }
}
