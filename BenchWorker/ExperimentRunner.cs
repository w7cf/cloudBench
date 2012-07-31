// Copyright (c) Microsoft Corporation.  All rights reserved.
namespace WorkerRole1
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
        readonly CloudStorageAccount reportAccount;
        readonly CloudStorageAccount benchmarkAccount;

        public ExperimentRunner()
        {
            this.reportAccount = CloudStorageAccount.Parse(RoleEnvironment.GetConfigurationSettingValue("ReportStorage"));
            this.benchmarkAccount = CloudStorageAccount.Parse(RoleEnvironment.GetConfigurationSettingValue("BenchmarkStorage"));
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
                BenchmarkMonitor monitor = new BenchmarkMonitor();
                monitor.Start();
                Thread.Sleep(30000);
                monitor.Stop();
                
                XBlobExperiments experiment = new XBlobExperiments(Guid.NewGuid(), this.benchmarkAccount.CreateCloudBlobClient(), 1, 10, 100 * 1024);
                
                IEnumerable<ExperimentResult> results = experiment.Run();
                Trace.TraceInformation("Got results from experiment batch '{0}' at {1}", count++, DateTime.UtcNow);
            }
        }
    }
}
