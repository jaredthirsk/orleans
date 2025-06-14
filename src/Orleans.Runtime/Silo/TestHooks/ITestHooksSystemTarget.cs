using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Forkleans.Runtime.TestHooks
{
    internal interface ITestHooks
    {
        Task<SiloAddress> GetConsistentRingPrimaryTargetSilo(uint key);
        Task<string> GetConsistentRingProviderDiagnosticInfo();
        Task<string> GetServiceId();
        Task<bool> HasStorageProvider(string providerName);
        Task<bool> HasStreamProvider(string providerName);
        Task<int> UnregisterGrainForTesting(GrainId grain);
        Task LatchIsOverloaded(bool overloaded, TimeSpan latchPeriod);
        Task<Dictionary<SiloAddress, SiloStatus>> GetApproximateSiloStatuses();
    }

    internal interface ITestHooksSystemTarget : ITestHooks, ISystemTarget
    {
    }
}
