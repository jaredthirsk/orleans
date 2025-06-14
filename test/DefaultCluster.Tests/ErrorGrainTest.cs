using System.Diagnostics;
using Microsoft.Extensions.Logging;
using Forkleans.Runtime;
using Forkleans.Internal;
using TestExtensions;
using UnitTests.GrainInterfaces;
using UnitTests.Grains;
using Xunit;
using Xunit.Abstractions;

namespace DefaultCluster.Tests
{
    /// <summary>
    /// Summary description for ErrorHandlingGrainTest
    /// </summary>
    public class ErrorGrainTest : HostedTestClusterEnsureDefaultStarted
    {
        private static readonly TimeSpan timeout = TimeSpan.FromSeconds(10);
        private readonly ITestOutputHelper output;

        public ErrorGrainTest(ITestOutputHelper output, DefaultClusterFixture fixture) : base(fixture)
        {
            this.output = output;
        }

        [Fact, TestCategory("BVT"), TestCategory("ErrorHandling")]
        public async Task ErrorGrain_GetGrain()
        {
            var grainFullName = typeof(ErrorGrain).FullName;
            IErrorGrain grain = this.GrainFactory.GetGrain<IErrorGrain>(GetRandomGrainId(), grainFullName);
            _ = await grain.GetA();
        }

        [Fact, TestCategory("BVT"), TestCategory("ErrorHandling")]
        public async Task ErrorHandlingLocalError()
        {
            LocalErrorGrain localGrain = new LocalErrorGrain();
            
            Task<int> intPromise = localGrain.GetAxBError();
            try
            {
                await intPromise;
                Assert.Fail("Should not have executed");
            }
            catch (Exception exc2)
            {
                Assert.Equal(exc2.GetBaseException().Message, (new Exception("GetAxBError-Exception")).Message);
            }

            Assert.True(intPromise.Status == TaskStatus.Faulted);                
        }

        [Fact, TestCategory("BVT"), TestCategory("ErrorHandling")]
        // check that grain that throws an error breaks its promise and later Wait and GetValue on it will throw
        public async Task ErrorHandlingGrainError1()
        {
            var grainFullName = typeof(ErrorGrain).FullName;
            IErrorGrain grain = this.GrainFactory.GetGrain<IErrorGrain>(GetRandomGrainId(), grainFullName);

            Task<int> intPromise = grain.GetAxBError();
            try
            {
                await intPromise;
                Assert.Fail("Should have thrown");
            }
            catch (Exception)
            {
                Assert.True(intPromise.Status == TaskStatus.Faulted);
            }

            try
            {
                await intPromise;
                Assert.Fail("Should have thrown");
            }
            catch (Exception exc2)
            {
                Assert.True(intPromise.Status == TaskStatus.Faulted);
                Assert.Equal((new Exception("GetAxBError-Exception")).Message, exc2.GetBaseException().Message);
            }

            Assert.True(intPromise.Status == TaskStatus.Faulted);
        }

        [Fact, TestCategory("BVT"), TestCategory("ErrorHandling")]
        // check that premature wait finishes on time with false.
        public async Task ErrorHandlingTimedMethod()
        {
            var grainFullName = typeof(ErrorGrain).FullName;
            IErrorGrain grain = this.GrainFactory.GetGrain<IErrorGrain>(GetRandomGrainId(), grainFullName);

            Task promise = grain.LongMethod(2000);

            // there is a race in the test here. If run in debugger, the invocation can actually finish OK
            Stopwatch stopwatch = Stopwatch.StartNew();

            await Task.Delay(1000);
            Assert.False(promise.IsCompleted, "The task shouldn't have completed yet.");

            // these asserts depend on timing issues and will be wrong for the sync version of OrleansTask
            Assert.True(stopwatch.ElapsedMilliseconds >= 900, $"Waited less than 900ms: ({stopwatch.ElapsedMilliseconds}ms)"); // check that we waited at least 0.9 second
            Assert.True(stopwatch.ElapsedMilliseconds <= 1300, $"Waited longer than 1300ms: ({stopwatch.ElapsedMilliseconds}ms)");

            await promise; // just wait for the server side grain invocation to finish
            
            Assert.True(promise.Status == TaskStatus.RanToCompletion);
        }

        [Fact, TestCategory("BVT"), TestCategory("ErrorHandling")]
        // check that premature wait finishes on time but does not throw with false and later wait throws.
        public async Task ErrorHandlingTimedMethodWithError()
        {
            var grainFullName = typeof(ErrorGrain).FullName;
            IErrorGrain grain = this.GrainFactory.GetGrain<IErrorGrain>(GetRandomGrainId(), grainFullName);

            Task promise = grain.LongMethodWithError(2000);

            // there is a race in the test here. If run in debugger, the invocation can actually finish OK
            Stopwatch stopwatch = Stopwatch.StartNew();

            await Task.Delay(1000);
            Assert.False(promise.IsCompleted, "The task shouldn't have completed yet.");

            stopwatch.Stop();
            Assert.True(stopwatch.ElapsedMilliseconds >= 900, $"Waited less than 900ms: ({stopwatch.ElapsedMilliseconds}ms)"); // check that we waited at least 0.9 second
            Assert.True(stopwatch.ElapsedMilliseconds <= 1300, $"Waited longer than 1300ms: ({stopwatch.ElapsedMilliseconds}ms)");

            await Assert.ThrowsAsync<Exception>(() => promise);

            Assert.True(promise.Status == TaskStatus.Faulted);
        }

        [Fact, TestCategory("Functional"), TestCategory("ErrorHandling"), TestCategory("Stress")]
        public async Task StressHandlingMultipleDelayedRequests()
        {
            IErrorGrain grain = this.GrainFactory.GetGrain<IErrorGrain>(GetRandomGrainId());
            bool once = true;
            List<Task> tasks = new List<Task>();
            for (int i = 0; i < 500; i++)
            {
                Task promise = grain.DelayMethod(1);
                tasks.Add(promise);
                if (once)
                {
                    once = false;
                    await promise;
                }

            }
            await Task.WhenAll(tasks).WaitAsync(TimeSpan.FromSeconds(20));
        }

        [Fact, TestCategory("BVT"), TestCategory("ErrorHandling"), TestCategory("GrainReference")]
        public async Task ArgumentTypes_ListOfGrainReferences()
        {
            var grainFullName = typeof(ErrorGrain).FullName;
            List<IErrorGrain> list = new List<IErrorGrain>();
            IErrorGrain grain = this.GrainFactory.GetGrain<IErrorGrain>(GetRandomGrainId(), grainFullName);
            list.Add(this.GrainFactory.GetGrain<IErrorGrain>(GetRandomGrainId(), grainFullName));
            list.Add(this.GrainFactory.GetGrain<IErrorGrain>(GetRandomGrainId(), grainFullName));
            await grain.AddChildren(list).WaitAsync(timeout);
        }

        [Fact, TestCategory("BVT"), TestCategory("AsynchronyPrimitives"), TestCategory("ErrorHandling")]
        public async Task AC_DelayedExecutor_2()
        {
            var grainFullName = typeof(ErrorGrain).FullName;
            IErrorGrain grain = this.GrainFactory.GetGrain<IErrorGrain>(GetRandomGrainId(), grainFullName);
            Task<bool> promise = grain.ExecuteDelayed(TimeSpan.FromMilliseconds(2000));
            bool result = await promise;
            Assert.True(result);
        }

        [Fact, TestCategory("BVT"), TestCategory("SimpleGrain")]
        public async Task SimpleGrain_AsyncMethods()
        {
            ISimpleGrainWithAsyncMethods grain = this.GrainFactory.GetGrain<ISimpleGrainWithAsyncMethods>(GetRandomGrainId());
            Task setPromise = grain.SetA_Async(10);
            await setPromise;

            setPromise = grain.SetB_Async(30);
            await setPromise;

            var value = await grain.GetAxB_Async();
            Assert.Equal(300, value);
        }

        [Fact, TestCategory("BVT"), TestCategory("SimpleGrain")]
        public async Task SimpleGrain_PromiseForward()
        {
            ISimpleGrain forwardGrain = this.GrainFactory.GetGrain<IPromiseForwardGrain>(GetRandomGrainId());
            Task<int> promise = forwardGrain.GetAxB(5, 6);
            int result = await promise;
            Assert.Equal(30, result);
        }

        [Fact, TestCategory("BVT"), TestCategory("SimpleGrain")]
        public void SimpleGrain_GuidDistribution()
        {
            int n = 0x1111;
            CreateGR(n, 1);
            CreateGR(n + 1, 1);
            CreateGR(n + 2, 1);
            CreateGR(n + 3, 1);
            CreateGR(n + 4, 1);

            Logger.LogInformation("================");

            CreateGR(n, 2);
            CreateGR(n + 1, 2);
            CreateGR(n + 2, 2);
            CreateGR(n + 3, 2);
            CreateGR(n + 4, 2);

            Logger.LogInformation("DONE.");
        }

        private void CreateGR(int n, int type)
        {
            Guid guid;
            if (type == 1)
            {
                guid = Guid.Parse(string.Format("00000000-0000-0000-0000-{0:X12}", n));
            }
            else
            {
                guid = Guid.Parse(string.Format("{0:X8}-0000-0000-0000-000000000000", n));
            }
            IEchoGrain grain = this.GrainFactory.GetGrain<IEchoGrain>(guid);
            GrainId grainId = ((GrainReference)grain.AsReference<IEchoGrain>()).GrainId;
            output.WriteLine("Guid = {0}, Guid.HashCode = x{1:X8}, GrainId.HashCode = x{2:X8}, GrainId.UniformHashCode = x{3:X8}", guid, guid.GetHashCode(), grainId.GetHashCode(), grainId.GetUniformHashCode());
        }

        [Fact, TestCategory("Revisit"), TestCategory("Observers")]
        public void ObserverTest_Disconnect()
        {
            ObserverTest_DisconnectRunner(false);
        }

        [Fact, TestCategory("Revisit"), TestCategory("Observers")]
        public void ObserverTest_Disconnect2()
        {
            ObserverTest_DisconnectRunner(true);
        }

        private void ObserverTest_DisconnectRunner(bool observeTwice)
        {
            // this is for manual repro & validation in the debugger
            // wait to send event because it takes 60s to drop client grain
            //var simple1 = SimpleGrainTests.GetSimpleGrain();
            //var simple2 = SimpleGrainFactory.Cast(Domain.Current.Create(typeof(ISimpleGrain).FullName,
            //    new Dictionary<string, object> { { "EventDelay", 70000 } }));
            //var result = new ResultHandle();
            //var callback = new SimpleGrainObserver((a, b, r) =>
            //{
            //    r.Done = (a == 10);
            //    output.WriteLine("Received observer callback: A={0} B={1} Done={2}", a, b, r.Done);
            //}, result);
            //var observer = SimpleGrainObserverFactory.CreateObjectReference(callback);
            //if (observeTwice)
            //{
            //    simple1.Subscribe(observer).Wait();
            //    simple1.SetB(1).Wait(); // send a message to the observer to get it in the cache
            //}
            //simple2.Subscribe(observer).Wait();
            //simple2.SetA(10).Wait();
            //Thread.Sleep(2000);
            //Client.Uninitialize();
            //var timeout80sec = TimeSpan.FromSeconds(80);
            //Assert.False(result.WaitForFinished(timeout80sec), "WaitforFinished Timeout=" + timeout80sec);
            //// prevent silo from shutting down right away
            //Thread.Sleep(Debugger.IsAttached ? TimeSpan.FromMinutes(2) : TimeSpan.FromSeconds(5));
        }
    }
}
