using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Forkleans.Configuration;
using Forkleans.Runtime;
using Forkleans.Runtime.Scheduler;

namespace UnitTests.TesterInternal
{
    public class SchedulingHelper
    {
        internal static WorkItemGroup CreateWorkItemGroupForTesting(
            IGrainContext context,
            ILoggerFactory loggerFactory)
        {
            ArgumentNullException.ThrowIfNull(context);
            ArgumentNullException.ThrowIfNull(loggerFactory);
            var services = new ServiceCollection();
            services.AddOptions();
            services.AddLogging();
            services.AddSingleton(loggerFactory);
            services.Configure<SchedulingOptions>(options =>
            {
                options.DelayWarningThreshold = TimeSpan.FromMilliseconds(100);
                options.ActivationSchedulingQuantum = TimeSpan.FromMilliseconds(100);
                options.TurnWarningLengthThreshold = TimeSpan.FromMilliseconds(100);
                options.StoppedActivationWarningInterval = TimeSpan.FromMilliseconds(200);
            });

            var s = services.BuildServiceProvider();
            var result = new WorkItemGroup(
                context,
                s.GetRequiredService<ILogger<WorkItemGroup>>(),
                s.GetRequiredService<ILogger<ActivationTaskScheduler>>(),
                s.GetRequiredService<IOptions<SchedulingOptions>>());
            return result;
        }
    }
}
