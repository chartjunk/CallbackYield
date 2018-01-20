using System;
using System.Collections;
using System.Collections.Concurrent;
using System.Collections.Generic;

namespace CallbackYielder
{
    public class CallbackYielderBufferBuilder<TItem>
    {
        private readonly Action<Action<TItem>> _createCallbackMethod;
        internal List<Action<CallbackYielderBuffer<TItem>>> DoOnBufferActions { get; } = new List<Action<CallbackYielderBuffer<TItem>>>();

        public CallbackYielderBufferBuilderStopAfterCondition<TItem> StopAfter 
            => new CallbackYielderBufferBuilderStopAfterCondition<TItem>(this); 

        public Func<BlockingCollection<TItem>> InstantiateBufferCollection { get; set; } =
            () => new BlockingCollection<TItem>();

        public CallbackYielderBufferBuilder(Action<Action<TItem>> createCallbackMethod)
        {
            _createCallbackMethod = createCallbackMethod;
        }
        
        public CallbackYielderBuffer<TItem> Build()
        {
            var collection = InstantiateBufferCollection();
            var buffer = new CallbackYielderBuffer<TItem>(collection.GetConsumingEnumerable);
            buffer.StoppingBuffer += (e, a) => collection.CompleteAdding();
            DoOnBufferActions.ForEach(action => action(buffer));

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