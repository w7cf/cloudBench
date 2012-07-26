// Copyright (c) Microsoft Corporation.  All rights reserved.
namespace BenchLib
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Text;
    using Microsoft.WindowsAzure.StorageClient;

    public class ResultEntity : TableServiceEntity
    {
        public string Title { get; set; }
        public bool Success { get; set; }
        public DateTime Started { get; set; }
        public TimeSpan TotalDuration { get; set; }
        public TimeSpan NetDuration { get; set; }
        public int CompletedIterations { get; set; }
        public double AccumulatedMetric { get; set; }
        public string Error { get; set; }
        //public double Throughput { get; set; }
    }

    public static class ExperimentResultsExtensions
    {
        public static ResultEntity AsResultEntity(this ExperimentResult result)
        {
            return new ResultEntity
            {
                Title = result.Title,
                Success = result.Success,
                Started = result.Started,
                TotalDuration = result.TotalDuration,
                NetDuration = result.NetDuration,
                CompletedIterations = result.CompletedIterations,
                AccumulatedMetric = result.CompletedIterations,
                Error = result.Error,
            };
        }

#if needed
        public static ExperimentResult AsExperimentResult(this ResultEntity entity)
        {
            return new ExperimentResult(entity.Title, entity.Success, entity.Started, entity.TotalDuration, entity.NetDuration, entity.CompletedIterations, entity.AccumulatedMetric, entity.Error);
        }
#endif
    }
}
