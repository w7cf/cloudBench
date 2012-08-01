// Copyright (c) Microsoft Corporation.  All rights reserved.
namespace BenchLib
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Text;

    public interface IExperimentRepo
    {
        IEnumerable<ExperimentRequest> GetPendingRequests();
        ExperimentRequest GetRequest(Guid experimentId);
        void UpdateRequestState(Guid experimentId, ExperimentRequest.State state); // ????
        
        void AddStatusForInstance(Guid experimentId, ExperimentStatus status, string instanceId);
        IEnumerable<ExperimentStatus> GetStatusFor(Guid experimentId);

        void AddResults(Guid experimentId, IEnumerable<ExperimentResult> results);
        IEnumerable<ExperimentResult> GetResults(Guid experimentId);
    }
}
