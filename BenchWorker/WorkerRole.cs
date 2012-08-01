// Copyright (c) Microsoft Corporation.  All rights reserved.
namespace BenchWorker
{
    using System;
    using System.Collections.Generic;
    using System.Diagnostics;
    using System.Linq;
    using System.Net;
    using System.Threading;
    using Microsoft.WindowsAzure;
    using Microsoft.WindowsAzure.Diagnostics;
    using Microsoft.WindowsAzure.ServiceRuntime;
    using Microsoft.WindowsAzure.StorageClient;
    using BenchLib;

    public class WorkerRole : RoleEntryPoint
    {
        public WorkerRole()
        {
            EnableWad();
        }

        public override void Run()
        {
            // This is a sample worker implementation. Replace with your logic.
            Trace.WriteLine("WorkerRole1 entry point called", "Information");

            ExperimentRunner runner = new ExperimentRunner(RoleEnvironment.CurrentRoleInstance.Id, new CloudExperimentRepo(), new CloudExperimentFactory());
            //runner.Start();
        }

        public override bool OnStart()
        {
            // allow for enough parallel connections to XStore
            ServicePointManager.DefaultConnectionLimit = 64;
            // StorageClient lib is authenticating correctly, no need for extra roundtrip on PUT/POST
            ServicePointManager.Expect100Continue = false;

            return base.OnStart();
        }

        void EnableWad()
        {
            DiagnosticMonitorConfiguration dmc = DiagnosticMonitor.GetDefaultInitialConfiguration();
            dmc.Logs.ScheduledTransferPeriod = TimeSpan.FromMinutes(2);
            dmc.Logs.ScheduledTransferLogLevelFilter = LogLevel.Verbose;
            DiagnosticMonitor.Start("Microsoft.WindowsAzure.Plugins.Diagnostics.ConnectionString", dmc);
        }
    }
}
