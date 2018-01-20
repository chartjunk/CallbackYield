using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace CallbackYielder.UnitTest
{
    [TestClass]
    public class BufferUnitTests
    {
        private string MockInput1 => "blah";
        private string MockInput2 => "bleh";
        private string[] MockInputCollectionOf2 => new[] {MockInput1, MockInput2};

        private string CallbackTransform1(string input) => $"output for {input}";

        private void MethodWithCallback<T>(IEnumerable<T> inputs, Action<T> callback,
            Func<T, T> transform)
            => inputs.ToList().ForEach(input => callback(transform(input)));

        [TestMethod]
        public void TestYieldItemsFromBuffer()
        {                      
            // Assign
            var count = MockInputCollectionOf2.Length;

            // Act
            var buffer = CallbackYielderBuilder
                .Buffer<string>(addToBuffer => MethodWithCallback(MockInputCollectionOf2, addToBuffer, CallbackTransform1))
                .StopAfter.PushedItemAmountIs(count)
                .Build();
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
            var buffer = CallbackYielderBuilder
                .Buffer<int>(addToBuffer =>
                    MethodWithCallback(collection, addToBuffer, v => v*2))
                .StopAfter.NoYieldSince(since)
                .Build();
            var items = buffer.Enumerate().ToList();

            // Assert
            Assert.AreEqual(count, items.Count);
        }
    }
}
