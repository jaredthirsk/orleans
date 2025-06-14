using Forkleans.TestingHost;
using TestExtensions;
using Tester.AzureUtils;
using UnitTests.Grains.Directories;

namespace Tester.Directories
{
    [TestCategory("AzureStorage")]
    public class AzureMultipleGrainDirectoriesTests : MultipleGrainDirectoriesTests
    {
        public class SiloConfigurator : ISiloConfigurator
        {
            public void Configure(ISiloBuilder siloBuilder)
            {
                siloBuilder.AddAzureTableGrainDirectory(
                    CustomDirectoryGrain.DIRECTORY,
                    options => options.TableServiceClient = AzureStorageOperationOptionsExtensions.GetTableServiceClient());
            }
        }

        protected override void CheckPreconditionsOrThrow() => TestUtils.CheckForAzureStorage();

        protected override void ConfigureTestCluster(TestClusterBuilder builder)
        {
            EnsurePreconditionsMet();

            base.ConfigureTestCluster(builder);
            builder.AddSiloBuilderConfigurator<SiloConfigurator>();
        }
    }
}
