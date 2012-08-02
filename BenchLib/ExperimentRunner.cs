// Copyright (c) Microsoft Corporation.  All rights reserved.
namespace BenchLib
{
    using System;
    using System.Collections.Generic;
    using System.Diagnostics;
    using System.Linq;
    using System.Threading;
    using BenchLib;

    public class ExperimentRunner : AsyncTask
    {
        readonly string InstanceId;
        readonly IExperimentRepo ExperimentRepo;
        readonly IExperimentFactory ExperimentFactory;
        readonly ManualResetEvent requestCancel = new ManualResetEvent(false);
        Thread runner;

        public ExperimentRunner(string instanceId, IExperimentRepo experimentRepo, IExperimentFactory experimentFactory)
        {
            InstanceId = instanceId;
            ExperimentRepo = experimentRepo;
            ExperimentFactory = experimentFactory;
        }

        protected override void OnStart()
        {
            if (IsRunning)
            {
                throw new InvalidOperationException("An experiment is already running.");
            }
            this.runner = new Thread(DispatchExperiments);
            this.runner.Name = "ExperimentRunner";
            this.runner.Start();
            base.OnStart();
        }

        void DispatchExperiments()
        {
            while (true)
            {
                IEnumerable<ExperimentRequest> requests = ExperimentRepo.GetPendingRequests();
                if (!WaitForCancelEvent(TimeSpan.FromTicks(1)) && requests.Any())
                {
                    ExperimentRequest request = requests.First();
                    Guid experimentId = request.ExperimentId;
                    Trace.TraceInformation("Start experiment batch '{0}' at {1}", experimentId, DateTime.UtcNow);

                    ExperimentContainer container = new ExperimentContainer(InstanceId, request, ExperimentFactory);

                    ExperimentRepo.UpdateRequestState(experimentId, ExperimentRequest.State.Running);
                    IEnumerable<ExperimentResult> results = container.Run();
                    ExperimentRepo.UpdateRequestState(experimentId, ExperimentRequest.State.Completed);

                    Trace.TraceInformation("Got results from experiment batch '{0}' at {1}", experimentId, DateTime.UtcNow);

                    ExperimentRepo.AddResults(request.ExperimentId, results);
                }
                else
                {
                    if (WaitForCancelEvent(TimeSpan.FromSeconds(5)))
                    {
                        // done, exit thread
                        OnTaskFinished();
                        return;
                    }
                }
            }
        }
    }
}
