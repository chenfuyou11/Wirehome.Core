using HA4IoT.Extensions.Messaging.Core;
using Microsoft.Reactive.Testing;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Collections.Generic;
using System.Reactive.Linq;
using System.Reactive.Threading.Tasks;
using System.Threading;
using System.Threading.Tasks;

namespace HA4IoT.Extensions.Tests
{
    [TestClass]
    public class EventAggregatorTests : ReactiveTest
    {
        private static IEventAggregator InitAggregator()
        {
            return new EventAggregator();
        }

        [TestMethod]
        public void GetSubscriptors_WhenSubscribeForType_ShouldReturnProperSubscriptions()
        {
            var aggregator = InitAggregator();

            aggregator.Subscribe<TestMessage>(message => { });
            aggregator.Subscribe<OtherMessage>(message => { });

            var result = aggregator.GetSubscriptors<TestMessage>();

            Assert.AreEqual(1, result.Count);
        }


        [TestMethod]
        public void GetSubscriptors_WhenSubscribeForType_ShouldReturnAlsoDerivedTypesSubscriptions()
        {
            var aggregator = InitAggregator();

            aggregator.Subscribe<TestMessage>(message => { });
            aggregator.Subscribe<OtherMessage>(message => { });

            var result = aggregator.GetSubscriptors<DerivedTestMessage>();

            Assert.AreEqual(1, result.Count);
        }

        [TestMethod]
        public void GetSubscriptors_WhenSubscribeWithSimpleFilter_ShouldReturnOnlySubscriptionsWithThatType()
        {
            var aggregator = InitAggregator();

            aggregator.Subscribe<TestMessage>(message => { });
            aggregator.Subscribe<TestMessage>(message => { }, "x");


            var result = aggregator.GetSubscriptors<TestMessage>("x");

            Assert.AreEqual(1, result.Count);
        }

        [TestMethod]
        public void GetSubscriptors_WhenSubscribeWithSimpleFilterAndSubscriblesHaveNoFilter_ShouldReturnNone()
        {
            var aggregator = InitAggregator();

            aggregator.Subscribe<TestMessage>(message => { });
            aggregator.Subscribe<TestMessage>(message => { });


            var result = aggregator.GetSubscriptors<TestMessage>("x");

            Assert.AreEqual(0, result.Count);
        }

        [TestMethod]
        public void GetSubscriptors_WhenSubscribeWithNoFilter_ShouldReturnOnlyNotFilteredSubscribles()
        {
            var aggregator = InitAggregator();

            aggregator.Subscribe<TestMessage>(message => { });
            aggregator.Subscribe<TestMessage>(message => { }, "x");


            var result = aggregator.GetSubscriptors<TestMessage>();

            Assert.AreEqual(1, result.Count);
        }

        [TestMethod]
        public void GetSubscriptors_WhenSubscribeWithStarFilter_ShouldResultAllNotfilteredAndFilteredSubscriblesByType()
        {
            var aggregator = InitAggregator();

            aggregator.Subscribe<TestMessage>(message => { });
            aggregator.Subscribe<TestMessage>(message => { }, "x");


            var result = aggregator.GetSubscriptors<TestMessage>("*");

            Assert.AreEqual(2, result.Count);
        }

        [TestMethod]
        public void GetSubscriptors_WhenSubscribeAsDefaultSubscriber_ShouldReturnonlyDefaultSubscribers()
        {
            var aggregator = InitAggregator();

            aggregator.Subscribe<TestMessage>(message => { });
            aggregator.Subscribe<TestMessage>(message => { }, "@");


            var result = aggregator.GetSubscriptors<TestMessage>("@");

            Assert.AreEqual(1, result.Count);
        }


        [TestMethod]
        public async Task PublishWithResultAsync_WhenSubscribed_ShouldReturnProperResult()
        {
            var aggregator = InitAggregator();

            aggregator.SubscribeForAsyncResult<TestMessage>(async message =>
            {
                await Task.Delay(10).ConfigureAwait(false);
                return "Test";
            });

            var result = await aggregator.PublishWithResultAsync<TestMessage, string>(new TestMessage()).ConfigureAwait(false);

            Assert.AreEqual("Test", result);
        }

        [TestMethod]
        public async Task PublishWithResultAsync_WhenSubscribedWithProperSimpleFilter_ShouldReturnProperResult()
        {
            var aggregator = InitAggregator();

            aggregator.SubscribeForAsyncResult<TestMessage>(async message =>
            {
                await Task.Delay(10).ConfigureAwait(false);
                return "Test";
            }, "DNF");

            var result = await aggregator.PublishWithResultAsync<TestMessage, string>(new TestMessage(), "DNF").ConfigureAwait(false);

            Assert.AreEqual("Test", result);
        }

        [TestMethod]
        public async Task PublishWithResultAsync_WhenTwoSubscribed_ShouldReturnFirstProperResult()
        {
            var aggregator = InitAggregator();

            aggregator.SubscribeForAsyncResult<TestMessage>(async message =>
            {
                await Task.Delay(50).ConfigureAwait(false);
                return "Slower";
            });

            aggregator.SubscribeForAsyncResult<TestMessage>(async message =>
            {
                await Task.Delay(10).ConfigureAwait(false);
                return "Faster";
            });

            var result = await aggregator.PublishWithResultAsync<TestMessage, string>(new TestMessage()).ConfigureAwait(false);

            Assert.AreEqual("Faster", result);
        }

        [TestMethod]
        [ExpectedException(typeof(InvalidCastException))]
        public async Task PublishWithResultAsync_WhenSubscribedForWrongReturnType_ShouldThrowInvalidCastException()
        {
            var aggregator = InitAggregator();

            aggregator.SubscribeForAsyncResult<TestMessage>(async message =>
            {
                await Task.Delay(10).ConfigureAwait(false);
                return "Test";
            });

            await aggregator.PublishWithResultAsync<TestMessage, List<string>>(new TestMessage()).ConfigureAwait(false);
        }

        [TestMethod]
        [ExpectedException(typeof(TimeoutException))]
        public async Task PublishWithResultAsync_WhenLongRun_ShouldThrowTimeoutException()
        {
            var aggregator = InitAggregator();

            aggregator.SubscribeForAsyncResult<TestMessage>(async message =>
            {
                await Task.Delay(100).ConfigureAwait(false);
                return "Test";
            });
            
            await aggregator.PublishWithResultAsync<TestMessage, string>(new TestMessage(), millisecondsTimeOut: 50).ConfigureAwait(false);
        }

        [TestMethod]
        public void PublishWithResultAsync_WhenExceptionInHandler_ShouldCatchIt()
        {
            var aggregator = InitAggregator();

            aggregator.SubscribeForAsyncResult<TestMessage>(async message =>
            {
                await Task.Delay(10).ConfigureAwait(false);
                throw new TestException();
            });

            AggregateExceptionHelper.AssertInnerException<TestException>(aggregator.PublishWithResultAsync<TestMessage, string>(new TestMessage()));

        }

        [TestMethod]
        public async Task PublishWithResultAsync_WhenRetry_ShouldRunAgainAndSucceed()
        {
            var aggregator = InitAggregator();
            int i = 1;

            aggregator.SubscribeForAsyncResult<TestMessage>(async message =>
            {
                await Task.Delay(10).ConfigureAwait(false);
                if (i-- > 0) throw new Exception("Test");
                return "OK";
            });

            var result = await aggregator.PublishWithResultAsync<TestMessage, string>(new TestMessage(), retryCount: 1).ConfigureAwait(false);
            Assert.AreEqual("OK", result);

        }

        [TestMethod]
        public void PublishWithResultAsync_WhenTwoSubscribedAndFasterWithException_ShouldGetException()
        {
            var aggregator = InitAggregator();

            aggregator.SubscribeForAsyncResult<TestMessage>(async message =>
            {
                await Task.Delay(30).ConfigureAwait(false);
                return "Slower";
            });

            aggregator.SubscribeForAsyncResult<TestMessage>(async message =>
            {
                await Task.Delay(10).ConfigureAwait(false);
                throw new TestException();
            });

            AggregateExceptionHelper.AssertInnerException<TestException>(aggregator.PublishWithResultAsync<TestMessage, string>(new TestMessage()));
        }

        [TestMethod]
        public async Task PublishWithResultAsync_WhenTwoSubscribedAndSlowerWithException_ShouldGetResult()
        {
            var aggregator = InitAggregator();

            aggregator.SubscribeForAsyncResult<TestMessage>(async message =>
            {
                await Task.Delay(20).ConfigureAwait(false);
                throw new Exception("test");
                
            });

            aggregator.SubscribeForAsyncResult<TestMessage>(async message =>
            {
                await Task.Delay(10).ConfigureAwait(false);
                return "Faster";
            });

            var result = await aggregator.PublishWithResultAsync<TestMessage, string>(new TestMessage()).ConfigureAwait(false);

            Assert.AreEqual("Faster", result);
        }

        [TestMethod]
        [ExpectedException(typeof(OperationCanceledException))]
        public async Task PublishWithResultAsync_WhenCanceled_ShouldThrowOperationCancel()
        {
            var aggregator = InitAggregator();

            aggregator.SubscribeForAsyncResult<TestMessage>(async message =>
            {
                await Task.Delay(50).ConfigureAwait(false);
                return "OK";

            });
            
            var ts = new CancellationTokenSource();
            var result = aggregator.PublishWithResultAsync<TestMessage, string>(new TestMessage(), cancellationToken: ts.Token).ConfigureAwait(false);
            ts.Cancel();
            await result;
        }

        [TestMethod]
        public void IsSubscribed_WhenCheckForActiveSubscription_ShouldReturnTrue()
        {
            var aggregator = InitAggregator();

            var subscription = aggregator.SubscribeForAsyncResult<TestMessage>(async message =>
            {
                await Task.Delay(50).ConfigureAwait(false);
                return "OK";

            });

            var result = aggregator.IsSubscribed(subscription);
            Assert.AreEqual(result, true);
        }

        [TestMethod]
        public void UnSubscribe_WhenInvokedForActiveSubscription_ShouldRemoveIt()
        {
            var aggregator = InitAggregator();

            var subscription = aggregator.SubscribeForAsyncResult<TestMessage>(async message =>
            {
                await Task.Delay(50).ConfigureAwait(false);
                return "OK";

            });

            aggregator.UnSubscribe(subscription);

            var result = aggregator.IsSubscribed(subscription);
            Assert.AreEqual(result, false);
        }

        [TestMethod]
        public void ClearSubscriptions_WhenInvoked_ShouldClearAllSubscriptions()
        {
            var aggregator = InitAggregator();

            aggregator.SubscribeForAsyncResult<TestMessage>(async message =>
            {
                await Task.Delay(50).ConfigureAwait(false);
                return "OK";

            });

            aggregator.SubscribeForAsyncResult<TestMessage>(async message =>
            {
                await Task.Delay(50).ConfigureAwait(false);
                return "OK";

            });

            aggregator.ClearSubscriptions();

            var result = aggregator.GetSubscriptors<TestMessage>();
            Assert.AreEqual(result.Count, 0);
        }

        [TestMethod]
        public void PublishWithResultsAsync_WhenSubscribed_ShouldReturnProperResult()
        {
            var aggregator = InitAggregator();
            var expected = new List<string> { "Test", "Test2" };

            aggregator.SubscribeForAsyncResult<TestMessage>(async message =>
            {
                await Task.Delay(10).ConfigureAwait(false);
                return expected[0];
            });

            aggregator.SubscribeForAsyncResult<TestMessage>(async message =>
            {
                await Task.Delay(30).ConfigureAwait(false);
                return expected[1];
            });


            var subscription = aggregator.PublishWithResults<TestMessage, string>(new TestMessage());
            
            subscription.AssertEqual(expected.ToObservable());
        }

        [TestMethod]
        public void PublishWithResultsAsync_WhenLongRun_ShouldTimeOut()
        {
            var aggregator = InitAggregator();
            var expected = new List<string> { "Test", "Test2" };

            aggregator.SubscribeForAsyncResult<TestMessage>(async message =>
            {
                await Task.Delay(100).ConfigureAwait(false);
                return expected[1];
            });

            aggregator.SubscribeForAsyncResult<TestMessage>(async message =>
            {
                await Task.Delay(100).ConfigureAwait(false);
                return expected[0];
            });


            var subscription = aggregator.PublishWithResults<TestMessage, string>(new TestMessage(), millisecondsTimeOut: 10);

            AggregateExceptionHelper.AssertInnerException<TimeoutException>(subscription);
        }

        [TestMethod]
        public void PublishWithResultsAsync_WhenCanceled_ShouldThrowOperationCanceledException()
        {
            var aggregator = InitAggregator();
            var expected = new List<string> { "Test", "Test2" };

            aggregator.SubscribeForAsyncResult<TestMessage>(async message =>
            {
                message.CancellationToken.ThrowIfCancellationRequested();
                await Task.Delay(100).ConfigureAwait(false);
                return expected[1];
            });

            var ts = new CancellationTokenSource();
            ts.Cancel();

            var subscription = aggregator.PublishWithResults<TestMessage, string>(new TestMessage(), cancellationToken: ts.Token);

            AggregateExceptionHelper.AssertInnerException<TaskCanceledException>(subscription);
        }

        [TestMethod]
        public async Task Publish_WhenSubscribed_ShouldInvokeSubscriber()
        {
            var aggregator = InitAggregator();
            bool isWorking = false;
            
            aggregator.Subscribe<TestMessage>(message =>
            {
                isWorking = true;
            });

            await aggregator.Publish(new TestMessage());
            
            Assert.AreEqual(true, isWorking);
        }

        [TestMethod]
        public void Publish_WhenCanceled_ShouldThrowOperationCanceledException()
        {
            var aggregator = InitAggregator();

            aggregator.Subscribe<TestMessage>(message =>
            {
                message.CancellationToken.ThrowIfCancellationRequested();
            });

            var ts = new CancellationTokenSource();
            ts.Cancel();
         
            AggregateExceptionHelper.AssertInnerException<OperationCanceledException>(() => aggregator.Publish(new TestMessage(), cancellationToken: ts.Token).Wait());
        }

        [TestMethod]
        public async Task PublishWithRepublishResult_WhenPublishWithResend_ShouldGetResultInSeparateSubscription()
        {
            var aggregator = InitAggregator();
            bool isWorking = false;
           // var taskSource = new TaskCompletionSource();

            aggregator.SubscribeForAsyncResult<TestMessage>(async message =>
            {
                await Task.Delay(10).ConfigureAwait(false);
                return new OtherMessage();
            });

            aggregator.Subscribe<OtherMessage>((x) =>
            {
                isWorking = true;
            });

            await aggregator.PublishWithRepublishResult<TestMessage, OtherMessage>(new TestMessage()).ConfigureAwait(false);

           
            Assert.AreEqual(true, isWorking);
        }

    }
}
