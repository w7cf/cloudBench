// Copyright (c) Microsoft Corporation.  All rights reserved.
namespace BenchLib
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Text;

    public interface IExperimentFactory
    {
        IEnumerable<string> ExperimentNames { get; }
        Experiment CreateExperiment(ExperimentRequest request);
    }
}
