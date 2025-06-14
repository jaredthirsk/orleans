using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Forkleans.Providers;
using Forkleans.Runtime;

namespace Forkleans
{
    internal interface ISiloControl : ISystemTarget, IVersionManager
    {
        Task Ping(string message);

        Task ForceGarbageCollection();
        Task ForceActivationCollection(TimeSpan ageLimit);
        Task ForceRuntimeStatisticsCollection();

        Task<SiloRuntimeStatistics> GetRuntimeStatistics();
        Task<List<Tuple<GrainId, string, int>>> GetGrainStatistics();
        Task<List<DetailedGrainStatistic>> GetDetailedGrainStatistics(string[] types = null);
        Task<SimpleGrainStatistic[]> GetSimpleGrainStatistics();
        Task<DetailedGrainReport> GetDetailedGrainReport(GrainId grainId);

        Task<int> GetActivationCount();
        Task MigrateRandomActivations(SiloAddress target, int count);

        Task<object> SendControlCommandToProvider<T>(string providerName, int command, object arg) where T : IControllable;
        Task<List<GrainId>> GetActiveGrains(GrainType grainType);
    }
}
