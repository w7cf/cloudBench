using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace BenchLib
{
    // calculates running mean & stdDev, discarding samples
    // see http://www.johndcook.com/standard_deviation.html
    // and Knuth: The Art of Computer Programming, volume 2, 3rd ed. p 232
    public class RunningStatistics
    {
        long count;
        double squaredMean;
        double mean;
        double min = double.MaxValue;
        double max = double.MinValue;

        public long Count
        {
            get { return this.count; }
        }

        public double Mean
        {
            get { return (Count > 0) ? this.mean : 0.0; }
        }

        public double Min
        {
            get { return (Count > 0) ? this.min : 0.0; }
        }

        public double Max
        {
            get { return (Count > 0) ? this.max : 0.0; }
        }

        public double Variance
        {
            get { return (Count > 1) ? this.squaredMean / (Count - 1) : 0.0; }
        }

        public double StdDev
        {
            get { return Math.Sqrt(Variance); }
        }

        public void Clear()
        {
            this.count = 0;
            this.mean = 0.0;
            this.squaredMean = 0.0;
        }

        public void AddSample(double x)
        {
            this.count++;
            if (this.count == 1)
            {
                this.mean = x;
            }
            else
            {
                double newMean = this.mean + (x - this.mean) / this.count;
                this.squaredMean = this.squaredMean + (x - this.mean) * (x - newMean);
                this.mean = newMean;
            }
            if (x > this.max)
            {
                this.max = x;
            }
            if (x < this.min)
            {
                this.min = x;
            }
        }
    }
}
