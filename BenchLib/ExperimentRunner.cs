// Copyright (c) Microsoft Corporation.  All rights reserved.
namespace BenchLib
{
    using System;
    using System.Collections.Generic;
    using System.Diagnostics;
    using System.Linq;
    using System.Threading;
    using BenchLib;

    public class ExperimentRunner
    {
        readonly string InstanceId;
        readonly IExperimentRepo ExperimentRepo;
        readonly IExperimentFactory ExperimentFactory;

        public ExperimentRunner(string instanceId, IExperimentRepo experimentRepo, IExperimentFactory experimentFactory)
        {
            InstanceId = instanceId;
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
            while (true)
            {
                IEnumerable<ExperimentRequest> requests = ExperimentRepo.GetPendingRequests();
                if (requests.Any())
                {
                    ExperimentRequest request = requests.First();
                    Guid experimentId = request.ExperimentId;
                    Trace.TraceInformation("Start experiment batch '{0}' at {1}", experimentId, DateTime.UtcNow);

                    ExperimentContainer experiment = new ExperimentContainer(InstanceId, request, ExperimentFactory);

                    ExperimentRepo.UpdateRequestState(experimentId, ExperimentRequest.State.Running);
                    IEnumerable<ExperimentResult> results = experiment.Run();
                    ExperimentRepo.UpdateRequestState(experimentId, ExperimentRequest.State.Completed);

                    Trace.TraceInformation("Got results from experiment batch '{0}' at {1}", experimentId, DateTime.UtcNow);

                    ExperimentRepo.AddResults(request.ExperimentId, results);
                }
                else
                {
                    Thread.Sleep(TimeSpan.FromSeconds(10));
                }
            }
        }
    }
}
