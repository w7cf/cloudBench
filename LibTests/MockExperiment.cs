// Copyright (c) Microsoft Corporation.  All rights reserved.
namespace LibTests
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Text;
    using BenchLib;

    enum Step
    {
        PrepareExperiment,
        PrepareIteration,
        RunSingleIteration,
        CleanupIteration,
        CleanupExperiment,
    }

    class MockExperiment : Experiment
    {
        List<Step> tracker;
        Func<int, double> iteration;

        public MockExperiment(List<Step> tracker, string title, int requestedIterations, string instanceId, Func<int, double> iteration = null)
            : base(Guid.NewGuid(), title, requestedIterations, instanceId)
        {
            this.tracker = tracker;
            this.iteration = iteration;
        }

        protected override double RunSingleIteration(int currentIteration)
        {
            this.tracker.Add(Step.RunSingleIteration);
            if (this.iteration != null)
            {
                return this.iteration(currentIteration);
            }
            return 42.0;
        }

        protected override void PrepareExperiment()
        {
            this.tracker.Add(Step.PrepareExperiment);
        }

        protected override void PrepareIteration(int currentIteration)
        {
            this.tracker.Add(Step.PrepareIteration);
        }

        protected override void CleanupIteration(int currentIteration)
        {
            this.tracker.Add(Step.CleanupIteration);
        }

        protected override void CleanupExperiment()
        {
            this.tracker.Add(Step.CleanupExperiment);
        }
    }
}
