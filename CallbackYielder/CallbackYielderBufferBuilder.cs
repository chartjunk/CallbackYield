using System;
using System.Collections;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;

namespace CallbackYielder
{
    public class CallbackYielderBufferBuilder<TItem>
    {
        private readonly Action<Action<TItem>> _createCallbackMethod;
        private Action _doFinally;
        internal List<Action<CallbackYielderBuffer<TItem>>> DoOnBufferActions { get; } = new List<Action<CallbackYielderBuffer<TItem>>>();

        public CallbackYielderBufferBuilderStopAfterCondition<TItem> StopAfter 
            => new CallbackYielderBufferBuilderStopAfterCondition<TItem>(this); 

        public Func<BlockingCollection<TItem>> InstantiateBufferCollection { get; set; } =
            () => new BlockingCollection<TItem>();

        public CallbackYielderBufferBuilder<TItem> Finally(Action doFinally)
        {
            _doFinally = doFinally;
            return this;
        }

        public CallbackYielderBufferBuilder(Action<Action<TItem>> createCallbackMethod)
        {
            _createCallbackMethod = createCallbackMethod;
        }
        
        public CallbackYielderBuffer<TItem> Build()
        {
            var collection = InstantiateBufferCollection();
            var buffer = new CallbackYielderBuffer<TItem>(collection.GetConsumingEnumerable);
            buffer.StoppingBuffer += (e, a) => collection.CompleteAdding();
            buffer.Disposing += (e, a) => _doFinally();
            foreach (var action in DoOnBufferActions)
                action(buffer);

            _createCallbackMethod(item =>
            {
                buffer.InvokeBeforePushingNewItemEvent();
                collection.Add(item);
                buffer.InvokeAfterPushingNewItemEvent();
            });
            // Now items should be getting pushed to the collection asynchronously

            return buffer;
        }
    }
}