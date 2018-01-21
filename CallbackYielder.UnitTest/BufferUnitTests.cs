using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace CallbackYielder.UnitTest
{
    [TestClass]
    public class BufferUnitTests
    {
        private string MockInput1 => "blah";
        private string MockInput2 => "bleh";
        private string MockInput3 => "blöh";
        private string MockInput4 => "blyh";
        private string MockInput5 => "blih";
        private string[] MockInputCollectionOf2 => new[] {MockInput1, MockInput2};

        private string CallbackTransform1(string input) => $"output for {input}";

        private async Task MethodWithCallback<T>(IEnumerable<T> inputs, Action<T> callback,
            Func<T, T> transform)
        {
            await Task.Run(() =>
            {
                foreach (var item in inputs)
                    callback(transform(item));
            });
        }

        [TestMethod]
        public void TestYieldItemsFromBuffer()
        {                      
            // Assign
            var count = MockInputCollectionOf2.Length;

            // Act
            var buffer = new Buffer<string>(async addToBuffer =>
                    await MethodWithCallback(MockInputCollectionOf2, addToBuffer, CallbackTransform1))
                .StopIf.PushedItemAmountIs(count);
            var items = buffer.Enumerate().ToList();

            // Assert
            Assert.AreEqual(count, items.Count);
            Assert.AreEqual(CallbackTransform1(MockInput1), items.First());
            Assert.AreEqual(CallbackTransform1(MockInput2), items.ElementAt(1));
        }

        [TestMethod]
        public void TestTimeLimitedBuffer()
        {
            // Assign
            var since = 2;
            var count = 1000;
            var collection = Enumerable.Range(1, count);
            
            // Act
            var buffer = new Buffer<int>(async addToBuffer =>
                    await MethodWithCallback(collection, addToBuffer, v => v * 2))
                .StopIf.NoPushSince(since);
            var items = buffer.Enumerate().ToList();

            // Assert
            Assert.AreEqual(count, items.Count);
        }

        [TestMethod]
        public void TestAddAfterYield()
        {
            // Assign
            var source = new BlockingCollection<string>();
            MockInputCollectionOf2.ToList().ForEach(source.Add);
            var target = new List<string>();

            // Act
            var buffer = new Buffer<string>(addToBuffer =>
#pragma warning disable 4014
                    // Fire and forget an async method
                    MethodWithCallback(source.GetConsumingEnumerable(), addToBuffer, CallbackTransform1))
#pragma warning restore 4014
                .StopIf.NoPushSince(seconds: 2);

            void WaitAndAdd(string item, int milliseconds = 1500)
            {
                Thread.Sleep(milliseconds);
                source.Add(item);
            }

            foreach (var item in buffer.Enumerate())
            {
                target.Add(item);

                if (target.Count == 1)
                    WaitAndAdd(MockInput3);

                if (target.Count == 2)
                    WaitAndAdd(MockInput4);

                if (target.Count == 4)
                    WaitAndAdd(MockInput5, 3000);
            }

            // Assert
            Assert.AreEqual(4, target.Count);
        }
    }
}
