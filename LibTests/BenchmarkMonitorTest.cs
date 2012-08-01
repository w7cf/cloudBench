using System;
using System.Threading;
using BenchLib;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace LibTests
{
    [TestClass]
    public class BenchmarkMonitorTest
    {
        [TestMethod]
        public void GetResultSample()
        {
            BenchmarkMonitor monitor = new BenchmarkMonitor();
            Assert.IsFalse(monitor.IsRunning, "status should be not running yet");
            Assert.IsNotNull(monitor.Results);
            Assert.AreEqual(0, monitor.Results.Count);

            monitor.Start();
            Assert.IsTrue(monitor.IsRunning, "status should now be running yet");
            Thread.Sleep(TimeSpan.FromSeconds(10));

            monitor.Stop();
            Assert.IsFalse(monitor.IsRunning, "status should no longer be running");
            Assert.IsTrue(monitor.Results.Count > 0);
        }

        [TestMethod]
        [ExpectedException(typeof(InvalidOperationException))]
        public void StartingTwice()
        {
            BenchmarkMonitor monitor = new BenchmarkMonitor();
            Assert.IsFalse(monitor.IsRunning, "status should be not running yet");
            monitor.Start();
            Assert.IsTrue(monitor.IsRunning, "status should now be running yet");
            monitor.Start();
        }
    }
}
