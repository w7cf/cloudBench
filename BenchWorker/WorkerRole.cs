// Copyright (c) Microsoft Corporation.  All rights reserved.
namespace WorkerRole1
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
        public override void Run()
        {
            // This is a sample worker implementation. Replace with your logic.
            Trace.WriteLine("WorkerRole1 entry point called", "Information");

            ExperimentRunner runner = new ExperimentRunner();
            runner.Start();
        }

        public override bool OnStart()
        {
            // allow for enough parallel connections to XStore
            ServicePointManager.DefaultConnectionLimit = 64;

            return base.OnStart();
        }
    }
}
