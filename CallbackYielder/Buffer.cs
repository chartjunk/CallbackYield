using System;
using System.Collections;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Threading;

namespace CallbackYielder
{
    public partial class Buffer<TItem>
    {
        private readonly Action<Func<TItem, bool>> _createCallbackMethod;
        private Action _doFinally;
        internal List<Action<Buffer<TItem>>> DoOnBufferActions { get; } = new List<Action<Buffer<TItem>>>();

        public StopIf<TItem> StopIf 
            => new StopIf<TItem>(this); 

        public Func<BlockingCollection<TItem>> InstantiateBufferCollection { get; set; } =
            () => new BlockingCollection<TItem>();

        public Buffer<TItem> Finally(Action doFinally)
        {
            _doFinally = doFinally;
            return this;
        }

        public Buffer(Action<Func<TItem, bool>> createCallbackMethod)
        {
            _createCallbackMethod = createCallbackMethod;
        }
        
        public IEnumerable<TItem> Enumerate()
        {
            var collection = InstantiateBufferCollection();
            this.StoppingBuffer += (e, a) =>
            {
                collection.CompleteAdding();
                _doFinally();
            };
            foreach (var action in DoOnBufferActions)
                action(this);

            _createCallbackMethod(item =>
            {
                if (collection.IsAddingCompleted)
                    return false;

                this.InvokeBeforePushingNewItemEvent();
                collection.Add(item);
                this.InvokeAfterPushingNewItemEvent();
                return true;
            });
            // Now items should be getting pushed to the collection asynchronously

            return collection.GetConsumingEnumerable().Select(i =>
            {
                YieldingItem?.Invoke(this, null);
                return i;
            });
        }
    }
}