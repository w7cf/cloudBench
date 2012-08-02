using System;
using System.Threading;
using BenchLib;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.Collections.Generic;
using System.Linq;

namespace LibTests
{

    [TestClass]
    public class ExperimentRunnerTest
    {
        [TestMethod]
        public void RunSingleExperimentOn2Threads()
        {
            IExperimentRepo repo = new MockExperimentRepo();
            IExperimentFactory factory = new MockExperimentFactory("Instance0",
                (i) =>
                {
                    Thread.Sleep(TimeSpan.FromSeconds(1));
                    return 3.5;
                });

            ExperimentRequest request = repo.GetPendingRequests().First();
            Assert.AreEqual(request.CurrentState, ExperimentRequest.State.Queued);
            Assert.AreEqual(2, request.NumberOfThreads);

            ExperimentRunner runner = new ExperimentRunner("Instance0", repo, factory);
            runner.Start();
            Assert.IsTrue(runner.IsRunning);
            Thread.Sleep(TimeSpan.FromSeconds(10 + request.RequestedIterations * 6));
            runner.Cancel();
            bool hasFinished = runner.WaitForCompletion(TimeSpan.FromSeconds(10));
            Assert.IsTrue(hasFinished, "unexpected tiemout waiting for runner to finish");

            // TODO: add proper runner/container result validation
            //Assert.AreEqual(request.CurrentState, ExperimentRequest.State.Completed);
            //IEnumerable<ExperimentResult> results = repo.GetResults(request.ExperimentId);
            //Assert.AreEqual(request.NumberOfThreads * request.RequestedIterations, results.Count());
        }

        class MockExperimentFactory : IExperimentFactory
        {
            readonly string instanceId;
            readonly Func<int, double> iteration;
            List<Step> tracker = new List<Step>();
            Dictionary<string, Type> experiments = new Dictionary<string, Type>();
            RandomBlobData blobDatasource;

            public MockExperimentFactory(string instanceId, Func<int, double> iteration = null)
            {
                this.instanceId = instanceId;
                this.iteration = iteration;
                this.experiments.Add("MockExperiment", typeof(MockExperiment));
            }

            public Experiment CreateExperiment(ExperimentRequest request)
            {
                Type experiment = this.experiments[request.ExperimentName];
                return new MockExperiment(this.tracker, request.ExperimentName, request.RequestedIterations, this.instanceId, this.iteration);
            }

            public IEnumerable<string> ExperimentNames
            {
                get { return this.experiments.Keys; }
            }

            RandomBlobData GetBlobDatasource(long minSize)
            {
                if (this.blobDatasource == null || this.blobDatasource.Size < minSize)
                {
                    this.blobDatasource = new RandomBlobData(minSize);
                }
                return this.blobDatasource;
            }
        }

        class MockExperimentRepo : IExperimentRepo
        {
            List<ExperimentRequest> experiments = new List<ExperimentRequest>();
            Dictionary<string, ExperimentStatus> statusList = new Dictionary<string, ExperimentStatus>();
            List<ExperimentResult> results = new List<ExperimentResult>();

            public MockExperimentRepo()
            {
                this.experiments.Add(
                    new ExperimentRequest(Guid.NewGuid(), "MockExperiment")
                    {
                        NumberOfThreads = 2,
                        RequestedIterations = 4,
                        MinDataSize = 100 * 1024,
                    });
            }

            public IEnumerable<ExperimentRequest> GetPendingRequests()
            {
                return this.experiments.Where((request) => request.CurrentState == ExperimentRequest.State.Queued);
            }

            public ExperimentRequest GetRequest(Guid experimentId)
            {
                AssertId(experimentId);
                return this.experiments[0];
            }

            public void AddStatusForInstance(Guid experimentId, ExperimentStatus status, string instanceId)
            {
                AssertId(experimentId);
                this.statusList.Add(instanceId, status);
            }

            public void UpdateRequestState(Guid experimentId, ExperimentRequest.State state)
            {
                AssertId(experimentId);
                this.experiments[0].CurrentState = state;
            }

            public IEnumerable<ExperimentStatus> GetStatusFor(Guid experimentId)
            {
                AssertId(experimentId);
                return this.statusList.Values;
            }

            public void AddResults(Guid experimentId, IEnumerable<ExperimentResult> results)
            {
                AssertId(experimentId);
                this.results.AddRange(results);
            }

            public IEnumerable<ExperimentResult> GetResults(Guid experimentId)
            {
                AssertId(experimentId);
                return this.results;
            }

            void AssertId(Guid experimentId)
            {
                if (this.experiments[0].ExperimentId != experimentId)
                {
                    throw new ArgumentException("unknown experimentId");
                }
            }
        }

    }
}
