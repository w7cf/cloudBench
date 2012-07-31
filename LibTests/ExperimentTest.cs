﻿using System;
using System.Text;
using System.Collections.Generic;
using System.Linq;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using BenchLib;
using System.Threading;
using System.Diagnostics;

namespace LibTests
{
    [TestClass]
    public class ExperimentTest
    {

        [TestMethod]
        public void HasCorrectStatus()
        {
            List<ExperimentStep> tracker = new List<ExperimentStep>();
            MockExperiment experiment = new MockExperiment(tracker, "HasCorrectStatus", 1,
                (i) => 
                {
                    Thread.Sleep(TimeSpan.FromSeconds(1)); 
                    return 3.5;
                });

            Assert.AreEqual("HasCorrectStatus", experiment.Title);
            Assert.IsFalse(experiment.IsRunning);
            Assert.AreEqual(1, experiment.RequestedIterations);
            Assert.IsNull(experiment.Result);
            Assert.IsNotNull(experiment.WaitHandle);

            experiment.Start();

            Assert.IsTrue(experiment.IsRunning);
            bool success = experiment.WaitForCompletion(TimeSpan.FromSeconds(20));
            Assert.IsTrue(success);
            Assert.IsFalse(experiment.IsRunning);
        }

        [TestMethod]
        public void HasValidResult()
        {
            List<ExperimentStep> tracker = new List<ExperimentStep>();
            MockExperiment experiment = new MockExperiment(tracker, "HasValidResult", 2,
                (i) =>
                {
                    Thread.Sleep(TimeSpan.FromSeconds(2));
                    return 3.5;
                });

            DateTime now = DateTime.UtcNow;
            Stopwatch sw = new Stopwatch();
            sw.Start();
            experiment.Start();
            Assert.IsTrue(experiment.WaitForCompletion(TimeSpan.FromSeconds(20)), "experiment timed out");
            sw.Stop();

            // validate result:
            Assert.IsNotNull(experiment.Result);
            ExperimentResult r = experiment.Result;
            Assert.IsTrue(r.Success);
            Assert.AreEqual(2, r.CompletedIterations);
            Assert.AreEqual(7.0, r.AccumulatedMetric);
            Assert.AreEqual(string.Empty, r.Error, "expected empty error string");
            Assert.IsTrue(r.TotalDuration > TimeSpan.FromSeconds(3),
                string.Format("TotalDuration too short to be credible: {0}", r.TotalDuration.ToString()));
            Assert.IsTrue(r.TotalDuration <= sw.Elapsed, 
                string.Format("TotalDuration too long: {0} (stopwatch says: {1}", r.TotalDuration.ToString(), sw.Elapsed.ToString()));
            Assert.IsTrue(r.NetDuration <= r.TotalDuration,
                string.Format("total duration '{0}' should >= net duration '{1}'", r.TotalDuration.ToString(), r.NetDuration.ToString()));
            Assert.IsTrue(Math.Abs(now.Ticks - r.Started.Ticks) <= Math.Pow(10, 6),
                string.Format("expected .Started to be a current time: {0} (now: {1}", r.Started.ToString("u"), now.ToString("u")));
        }

        [TestMethod]
        public void ExperimentWithException()
        {
            const string failureMessage = "Intentional experiment failure";

            List<ExperimentStep> tracker = new List<ExperimentStep>();
            MockExperiment experiment = new MockExperiment(tracker, "ExperimentHasRightOrderingOfSteps", 1,
                (i) =>
                {
                    Thread.Sleep(TimeSpan.FromSeconds(1));
                    throw new InvalidOperationException(failureMessage);
                });

            experiment.Start();
            Assert.IsTrue(experiment.WaitForCompletion(TimeSpan.FromSeconds(20)), "experiment timed out");

            Assert.IsNotNull(experiment.Result);
            ExperimentResult r = experiment.Result;
            Assert.IsFalse(r.Success);
            Assert.AreEqual(0, r.CompletedIterations);
            Assert.AreEqual(0.0, r.AccumulatedMetric);
            Assert.IsFalse(string.IsNullOrEmpty(r.Error), "expected actual error string");
            Assert.IsTrue(r.Error.Contains(failureMessage), "actual error string: " + r.Error);
        }

        [TestMethod]
        public void HasRightOrderingOfSteps()
        {
            List<ExperimentStep> tracker = new List<ExperimentStep>();
            MockExperiment experiment = new MockExperiment(tracker, "ExperimentWithException", 2);

            experiment.Start();
            Assert.IsTrue(experiment.WaitForCompletion(TimeSpan.FromSeconds(20)), "experiment timed out");

            AssertTracker(tracker, 
                new ExperimentStep[] 
                { 
                    ExperimentStep.PrepareExperiment, 
                    ExperimentStep.PrepareIteration, 
                    ExperimentStep.RunSingleIteration, 
                    ExperimentStep.CleanupIteration, 
                    ExperimentStep.PrepareIteration, 
                    ExperimentStep.RunSingleIteration, 
                    ExperimentStep.CleanupIteration, 
                    ExperimentStep.CleanupExperiment,
                });
        }

        void AssertTracker(IEnumerable<ExperimentStep> actual, IEnumerable<ExperimentStep> expected)
        {
            int expectedLength = expected.Count();
            int actualLength = actual.Count();

            int shortestLength = Math.Min(expectedLength, actualLength);
            IEnumerator<ExperimentStep> expectedEnum = expected.GetEnumerator();
            IEnumerator<ExperimentStep> actualEnum = actual.GetEnumerator();

            for (int i = 0; i < shortestLength; i++)
            {
                expectedEnum.MoveNext();
                actualEnum.MoveNext();
                Assert.AreEqual(expectedEnum.Current, actualEnum.Current,
                    string.Format("step {0} mismatch: expected: {1}, actual: {2}", i, expectedEnum.Current.ToString(), actualEnum.Current.ToString()));
            }
            Assert.AreEqual(expectedLength, actualLength,
                string.Format("expected and actual should have same length: expectedLength={0}, actualLength={1}", expectedLength, actualLength));
        }

        enum ExperimentStep
        {
            PrepareExperiment,
            PrepareIteration,
            RunSingleIteration,
            CleanupIteration,
            CleanupExperiment,
        }

        class MockExperiment : Experiment
        {
            List<ExperimentStep> tracker;
            Func<int, double> iteration;

            public MockExperiment(List<ExperimentStep> tracker, string title, int requestedIterations, Func<int,double> iteration = null)
                : base(title, requestedIterations)
            {
                this.tracker = tracker;
                this.iteration = iteration;
            }

            protected override double RunSingleIteration(int currentIteration)
            {
                this.tracker.Add(ExperimentStep.RunSingleIteration);
                if (this.iteration != null)
                {
                    return this.iteration(currentIteration);
                }
                return 42.0;
            }

            protected override void PrepareExperiment()
            {
                this.tracker.Add(ExperimentStep.PrepareExperiment);
            }

            protected override void PrepareIteration(int currentIteration)
            {
                this.tracker.Add(ExperimentStep.PrepareIteration);
            }

            protected override void CleanupIteration(int currentIteration)
            {
                this.tracker.Add(ExperimentStep.CleanupIteration);
            }

            protected override void CleanupExperiment()
            {
                this.tracker.Add(ExperimentStep.CleanupExperiment);
            }
        }
    }
}