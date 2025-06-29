using System.Diagnostics;
using Forkleans.Core.Internal;
using Forkleans.Placement;
using Forkleans.Runtime;
using Forkleans.Runtime.Placement;
using TestExtensions;
using Xunit;

namespace DefaultCluster.Tests.General
{
    public class MigrationTests : HostedTestClusterEnsureDefaultStarted
    {
        public MigrationTests(DefaultClusterFixture fixture) : base(fixture)
        {
        }

        /// <summary>
        /// Tests that grain migration works for a simple grain which directly implements <see cref="IGrainMigrationParticipant"/>.
        /// The test does not specify an alternative location for the grain to migrate to, but spins until it selects an alternative on its own.
        /// </summary>
        [Fact, TestCategory("BVT")]
        public async Task BasicGrainMigrationTest()
        {
            for (var i = 1; i < 100; ++i)
            {
                var grain = GrainFactory.GetGrain<IMigrationTestGrain>(GetRandomGrainId());
                var expectedState = Random.Shared.Next();
                await grain.SetState(expectedState);
                var originalAddress = await grain.GetGrainAddress();
                GrainAddress newAddress;
                do
                {
                    // Trigger migration without setting a placement hint, so the grain placement provider will be
                    // free to select any location including the existing one.
                    await grain.Cast<IGrainManagementExtension>().MigrateOnIdle();
                    newAddress = await grain.GetGrainAddress();
                } while (originalAddress == newAddress);

                var newState = await grain.GetState();
                Assert.Equal(expectedState, newState);
            }
        }

        /// <summary>
        /// Tests that migration can be initiated from within the grain's <see cref="IGrainBase.OnDeactivateAsync(DeactivationReason, CancellationToken)"/> implementation.
        /// </summary>
        [Fact, TestCategory("BVT")]
        public async Task InitiateMigrationFromOnDeactivateAsyncTest()
        {
            for (var i = 1; i < 100; ++i)
            {
                var grain = GrainFactory.GetGrain<IMigrationTestGrain>(GetRandomGrainId());
                var expectedState = Random.Shared.Next();
                await grain.SetState(expectedState);
                var originalAddress = await grain.GetGrainAddress();
                var originalHost = originalAddress.SiloAddress;
                var targetHost = Fixture.HostedCluster.GetActiveSilos().Select(s => s.SiloAddress).First(address => address != originalHost);

                // Trigger deactivation and tell the grain that it should migrate to the target host on deactivation.
                await grain.MigrateDuringDeactivation(targetHost);

                GrainAddress newAddress;
                do
                {
                    newAddress = await grain.GetGrainAddress();
                } while (newAddress.ActivationId == originalAddress.ActivationId);

                var newHost = newAddress.SiloAddress;
                Assert.Equal(targetHost, newHost);

                var newState = await grain.GetState();
                Assert.Equal(expectedState, newState);
            }
        }

        /// <summary>
        /// Tests that grain migration works for a simple grain which directly implements <see cref="IGrainMigrationParticipant"/>.
        /// The test specifies an alternative location for the grain to migrate to and asserts that it migrates to that location.
        /// </summary>
        [Fact, TestCategory("BVT")]
        public async Task DirectedGrainMigrationTest()
        {
            for (var i = 1; i < 100; ++i)
            {
                var grain = GrainFactory.GetGrain<IMigrationTestGrain>(GetRandomGrainId());
                var expectedState = Random.Shared.Next();
                await grain.SetState(expectedState);
                var originalAddress = await grain.GetGrainAddress();
                var originalHost = originalAddress.SiloAddress;
                var targetHost = Fixture.HostedCluster.GetActiveSilos().Select(s => s.SiloAddress).First(address => address != originalHost);

                // Trigger migration, setting a placement hint to coerce the placement director to use the target silo
                RequestContext.Set(IPlacementDirector.PlacementHintKey, targetHost);
                await grain.Cast<IGrainManagementExtension>().MigrateOnIdle();

                GrainAddress newAddress;
                do
                {
                    newAddress = await grain.GetGrainAddress();
                } while (newAddress.ActivationId == originalAddress.ActivationId);

                var newHost = newAddress.SiloAddress;
                Assert.Equal(targetHost, newHost);

                var newState = await grain.GetState();
                Assert.Equal(expectedState, newState);
            }
        }

        /// <summary>
        /// Tests that multiple grains can be migrated simultaneously.
        /// </summary>
        [Fact, TestCategory("BVT")]
        public async Task MultiGrainDirectedMigrationTest()
        {
            var baseId = GetRandomGrainId();
            for (var i = 1; i < 100; ++i)
            {
                var a = GrainFactory.GetGrain<IMigrationTestGrain>(baseId + 2 * i);
                var expectedState = Random.Shared.Next();
                await a.SetState(expectedState);
                var originalAddressA = await a.GetGrainAddress();
                var originalHostA = originalAddressA.SiloAddress;

                RequestContext.Set(IPlacementDirector.PlacementHintKey, originalHostA);
                var b = GrainFactory.GetGrain<IMigrationTestGrain>(baseId + 1 + 2 * i);
                await b.SetState(expectedState);
                var originalAddressB = await b.GetGrainAddress();
                Assert.Equal(originalHostA, originalAddressB.SiloAddress);

                var targetHost = Fixture.HostedCluster.GetActiveSilos().Select(s => s.SiloAddress).First(address => address != originalHostA);

                // Trigger migration, setting a placement hint to coerce the placement director to use the target silo
                RequestContext.Set(IPlacementDirector.PlacementHintKey, targetHost);
                var migrateA = a.Cast<IGrainManagementExtension>().MigrateOnIdle();
                var migrateB = b.Cast<IGrainManagementExtension>().MigrateOnIdle();
                await migrateA;
                await migrateB;

                while (true)
                {
                    var newAddress = await a.GetGrainAddress();
                    if (newAddress.ActivationId != originalAddressA.ActivationId)
                    {
                        Assert.Equal(targetHost, newAddress.SiloAddress);
                        break;
                    }
                }

                while (true)
                {
                    var newAddress = await b.GetGrainAddress();
                    if (newAddress.ActivationId != originalAddressB.ActivationId)
                    {
                        Assert.Equal(targetHost, newAddress.SiloAddress);
                        break;
                    }
                }

                Assert.Equal(expectedState, await a.GetState());
                Assert.Equal(expectedState, await b.GetState());
            }
        }

        /// <summary>
        /// Tests that grain migration works for a simple grain which uses <see cref="Grain{TGrainState}"/> for state.
        /// The test specifies an alternative location for the grain to migrate to and asserts that it migrates to that location.
        /// </summary>
        [Fact, TestCategory("BVT")]
        public async Task DirectedGrainMigrationTest_GrainOfT()
        {
            for (var i = 1; i < 100; ++i)
            {
                var grain = GrainFactory.GetGrain<IMigrationTestGrain_GrainOfT>(GetRandomGrainId());
                var expectedState = Random.Shared.Next();
                await grain.SetState(expectedState);
                var originalAddress = await grain.GetGrainAddress();
                var originalHost = originalAddress.SiloAddress;
                var targetHost = Fixture.HostedCluster.GetActiveSilos().Select(s => s.SiloAddress).First(address => address != originalHost);

                // Trigger migration, setting a placement hint to coerce the placement director to use the target silo
                RequestContext.Set(IPlacementDirector.PlacementHintKey, targetHost);
                await grain.Cast<IGrainManagementExtension>().MigrateOnIdle();

                GrainAddress newAddress;
                do
                {
                    newAddress = await grain.GetGrainAddress();
                } while (newAddress.ActivationId == originalAddress.ActivationId);

                var newHost = newAddress.SiloAddress;
                Assert.Equal(targetHost, newHost);

                var newState = await grain.GetState();
                Assert.Equal(expectedState, newState);
            }
        }

        /// <summary>
        /// Tests that grain migration works for a simple grain which uses <see cref="IPersistentState{TState}"/> for state.
        /// The test specifies an alternative location for the grain to migrate to and asserts that it migrates to that location.
        /// </summary>
        [Fact, TestCategory("BVT")]
        public async Task DirectedGrainMigrationTest_IPersistentStateOfT()
        {
            for (var i = 1; i < 100; ++i)
            {
                var grain = GrainFactory.GetGrain<IMigrationTestGrain_IPersistentStateOfT>(GetRandomGrainId());
                var expectedStateA = Random.Shared.Next();
                var expectedStateB = Random.Shared.Next();
                await grain.SetState(expectedStateA, expectedStateB);
                var originalAddress = await grain.GetGrainAddress();
                var originalHost = originalAddress.SiloAddress;
                var targetHost = Fixture.HostedCluster.GetActiveSilos().Select(s => s.SiloAddress).First(address => address != originalHost);

                // Trigger migration, setting a placement hint to coerce the placement director to use the target silo
                RequestContext.Set(IPlacementDirector.PlacementHintKey, targetHost);
                await grain.Cast<IGrainManagementExtension>().MigrateOnIdle();

                GrainAddress newAddress;
                do
                {
                    newAddress = await grain.GetGrainAddress();
                } while (newAddress.ActivationId == originalAddress.ActivationId);

                var newHost = newAddress.SiloAddress;
                Assert.Equal(targetHost, newHost);

                var (actualA, actualB) = await grain.GetState();
                Assert.Equal(expectedStateA, actualA);
                Assert.Equal(expectedStateB, actualB);
            }
        }

        /// <summary>
        /// When grain dehydration fails, the grain should be deactivated but will not retain migration state.
        /// </summary>
        [Fact, TestCategory("BVT")]
        public async Task FailDehydrationTest()
        {
            var grain = GrainFactory.GetGrain<IMigrationTestGrain>(GetRandomGrainId());
            var expectedState = Random.Shared.Next();
            await grain.SetState(expectedState);
            var originalAddress = await grain.GetGrainAddress();
            var targetHost = Fixture.HostedCluster.GetActiveSilos().Select(s => s.SiloAddress).First(address => address != originalAddress.SiloAddress);

            // Trigger migration, setting a placement hint to coerce the placement director to use the target silo
            // Also, tell the grain to fail to dehydrate (by stuffing some data into the request context which tells it to throw)
            RequestContext.Set("fail_dehydrate", true);
            RequestContext.Set(IPlacementDirector.PlacementHintKey, targetHost);
            await grain.Cast<IGrainManagementExtension>().MigrateOnIdle();

            var newAddress = await grain.GetGrainAddress();
            Assert.Equal(targetHost, newAddress.SiloAddress);

            // The grain should have lost its state during the failed migration.
            var newState = await grain.GetState();
            Assert.NotEqual(expectedState, newState);
        }

        /// <summary>
        /// When grain rehydration fails, the grain should be deactivated but will not retain migration state.
        /// </summary>
        [Fact, TestCategory("BVT")]
        public async Task FailRehydrationTest()
        {
            var grain = GrainFactory.GetGrain<IMigrationTestGrain>(GetRandomGrainId());
            var expectedState = Random.Shared.Next();
            await grain.SetState(expectedState);
            var originalAddress = await grain.GetGrainAddress();
            var targetHost = Fixture.HostedCluster.GetActiveSilos().Select(s => s.SiloAddress).First(address => address != originalAddress.SiloAddress);

            // Trigger migration, setting a placement hint to coerce the placement director to use the target silo
            // Also, tell the grain to fail to rehydrate (by stuffing some data into the rehydration context which tells it to throw)
            RequestContext.Set("fail_rehydrate", true);
            RequestContext.Set(IPlacementDirector.PlacementHintKey, targetHost);
            await grain.Cast<IGrainManagementExtension>().MigrateOnIdle();

            var newAddress = await grain.GetGrainAddress();
            Assert.Equal(targetHost, newAddress.SiloAddress);

            // The grain should have lost its state during the failed migration.
            var newState = await grain.GetState();
            Assert.NotEqual(expectedState, newState);
        }
    }

    public interface IMigrationTestGrain : IGrainWithIntegerKey
    {
        ValueTask<GrainAddress> GetGrainAddress();
        ValueTask SetState(int state);
        ValueTask<int> GetState();
        ValueTask MigrateDuringDeactivation(SiloAddress targetHost);
    }

    // Use random placement because this is used to test undirected migration.
    [RandomPlacement]
    public class MigrationTestGrain : Grain, IMigrationTestGrain, IGrainMigrationParticipant
    {
        private int _state;
        private SiloAddress _migrateDuringDeactivationTargetHost;
        public ValueTask<int> GetState() => new(_state);

        public ValueTask SetState(int state)
        {
            _state = state;
            return default;
        }

        public void OnDehydrate(IDehydrationContext migrationContext)
        {
            migrationContext.TryAddValue("state", _state);

            {
                if (RequestContext.Get("fail_rehydrate") is bool fail && fail)
                {
                    migrationContext.TryAddValue("fail_rehydrate", true);
                }
            }

            {
                if (RequestContext.Get("fail_dehydrate") is bool fail && fail)
                {
                    throw new InvalidOperationException("Failing to dehydrate on-command");
                }
            }
        }

        public void OnRehydrate(IRehydrationContext migrationContext)
        {
            if (migrationContext.TryGetValue("fail_rehydrate", out bool fail) && fail)
            {
                throw new InvalidOperationException("Failing to rehydrate on-command");
            }

            migrationContext.TryGetValue("state", out _state);
        }

        public ValueTask<GrainAddress> GetGrainAddress() => new(GrainContext.Address);

        public ValueTask MigrateDuringDeactivation(SiloAddress targetHost)
        {
            _migrateDuringDeactivationTargetHost = targetHost;
            this.DeactivateOnIdle();
            return ValueTask.CompletedTask;
        }

        public override Task OnDeactivateAsync(DeactivationReason reason, CancellationToken cancellationToken)
        {
            if (reason.ReasonCode is DeactivationReasonCode.ApplicationRequested && _migrateDuringDeactivationTargetHost is not null)
            {
                RequestContext.Set(IPlacementDirector.PlacementHintKey, _migrateDuringDeactivationTargetHost);
                this.MigrateOnIdle();
            }

            return base.OnDeactivateAsync(reason, cancellationToken);
        }
    }

    public interface IMigrationTestGrain_GrainOfT : IGrainWithIntegerKey
    {
        ValueTask SetState(int state);
        ValueTask<int> GetState();
        ValueTask<GrainAddress> GetGrainAddress();
    }

    public class MigrationTestGrainWithMemoryStorage : Grain<MyMigrationStateClass>, IMigrationTestGrain_GrainOfT
    {
        public ValueTask<int> GetState() => new(State.Value);

        public ValueTask SetState(int state)
        {
            State.Value = state;
            return default;
        }

        public ValueTask<GrainAddress> GetGrainAddress() => new(GrainContext.Address);
    }

    [GenerateSerializer]
    public class MyMigrationStateClass
    {
        [Id(0)]
        public int Value { get; set; }
    }

    public interface IMigrationTestGrain_IPersistentStateOfT : IGrainWithIntegerKey
    {
        ValueTask SetState(int a, int b);
        ValueTask<(int A, int B)> GetState();
        ValueTask<GrainAddress> GetGrainAddress();
    }

    public class MigrationTestGrainWithInjectedMemoryStorage : Grain, IMigrationTestGrain_IPersistentStateOfT
    {
        private readonly IPersistentState<MyMigrationStateClass> _stateA;
        private readonly IPersistentState<MyMigrationStateClass> _stateB;

        public MigrationTestGrainWithInjectedMemoryStorage(
            [PersistentState("a")] IPersistentState<MyMigrationStateClass> stateA,
            [PersistentState("b")] IPersistentState<MyMigrationStateClass> stateB)
        {
            _stateA = stateA;
            _stateB = stateB;
        }

        public ValueTask<(int A, int B)> GetState() => new((_stateA.State.Value, _stateB.State.Value));

        public ValueTask SetState(int a, int b)
        {
            _stateA.State.Value = a;
            _stateB.State.Value = b;
            return default;
        }

        public ValueTask<GrainAddress> GetGrainAddress() => new(GrainContext.Address);
    }
}
