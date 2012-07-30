using System;
using System.Text;
using System.Collections.Generic;
using System.Linq;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using BenchLib;

namespace LibTests
{
    [TestClass]
    public class StatisticsTest
    {
        [TestMethod]
        public void StandardDev()
        {
            RunningStatistics stats = new RunningStatistics();
            stats.AddSample(42.0);
            stats.AddSample(38.0);
            stats.AddSample(44.0);
            Assert.AreEqual(2.49, stats.StdDev);
        }
    }
}
