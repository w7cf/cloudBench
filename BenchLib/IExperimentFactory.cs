// Copyright (c) Microsoft Corporation.  All rights reserved.
namespace BenchLib
{
    using System.Collections.Generic;

    public interface IExperimentFactory
    {
        IEnumerable<string> ExperimentNames { get; }
        Experiment CreateExperiment(ExperimentRequest request);
    }
}
