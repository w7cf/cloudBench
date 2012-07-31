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
        public void EmptySamples()
        {
            RunningStatistics stats = new RunningStatistics();
            Assert.AreEqual(0, stats.Count);
            Assert.AreEqual(0, stats.Min);
            Assert.AreEqual(0, stats.Max);
            Assert.AreEqual(0, stats.Mean);
            Assert.AreEqual(0, stats.StdDev);
        }


        [TestMethod]
        public void StandardDevSmallCollection()
        {
            RunningStatistics stats = new RunningStatistics();
            stats.AddSample(2.0);
            stats.AddSample(4.0);
            stats.AddSample(4.0);
            stats.AddSample(4.0);
            stats.AddSample(5.0);
            stats.AddSample(5.0);
            stats.AddSample(7.0);
            stats.AddSample(9.0);

            Assert.AreEqual(8, stats.Count);
            Assert.AreEqual(2.0, stats.Min);
            Assert.AreEqual(9.0, stats.Max);
            AreEqual(5, stats.Mean);
            // low number of samples yields low precision of the running stdDev estimation
            AreEqual(2, stats.StdDev, 0);
        }

        void AreEqual(double expected, double actual, int significantFigures = 4)
        {
            decimal actualRounded = decimal.Round(new decimal(actual), significantFigures, MidpointRounding.AwayFromZero);
            Assert.IsTrue(Math.Abs(new decimal(expected) - actualRounded) == 0, string.Format("expected: {0}; actual: {1} ({2})", expected, actualRounded, actual));
        }
    }
}
