using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Forkleans.Runtime.ReminderService;
using Tester;
using TestExtensions;
using Xunit;
using Forkleans.Reminders.AzureStorage;
using Tester.AzureUtils;

namespace UnitTests.RemindersTest
{
    /// <summary>
    /// Tests for operation of Orleans Reminders Table using Azure
    /// </summary>
    [TestCategory("Reminders"), TestCategory("AzureStorage")]
    public class AzureRemindersTableTests : ReminderTableTestsBase
    {
        public AzureRemindersTableTests(ConnectionStringFixture fixture, TestEnvironmentFixture environment) : base(fixture, environment, CreateFilters())
        {
            TestUtils.CheckForAzureStorage();
        }

        private static LoggerFilterOptions CreateFilters()
        {
            var filters = new LoggerFilterOptions();
            filters.AddFilter("AzureTableDataManager", LogLevel.Trace);
            filters.AddFilter("OrleansSiloInstanceManager", LogLevel.Trace);
            filters.AddFilter("Storage", LogLevel.Trace);
            return filters;
        }

        public override Task DisposeAsync()
        {
            // Reset init timeout after tests
            return base.DisposeAsync();
        }

        protected override IReminderTable CreateRemindersTable()
        {
            TestUtils.CheckForAzureStorage();
            var options = Options.Create(new AzureTableReminderStorageOptions());
            options.Value.ConfigureTestDefaults();
            return new AzureBasedReminderTable(loggerFactory, this.clusterOptions, options);
        }

        protected override Task<string> GetConnectionString()
        {
            TestUtils.CheckForAzureStorage();
            return Task.FromResult("not used");
        }

        [SkippableFact]
        public void RemindersTable_Azure_Init()
        {
        }

        [SkippableFact, TestCategory("Functional")]
        public async Task RemindersTable_Azure_RemindersRange()
        {
            await RemindersRange(50);
        }

        [SkippableFact, TestCategory("Functional")]
        public async Task RemindersTable_Azure_RemindersParallelUpsert()
        {
            await RemindersParallelUpsert();
        }

        [SkippableFact, TestCategory("Functional")]
        public async Task RemindersTable_Azure_ReminderSimple()
        {
            await ReminderSimple();
        }
    }
}
