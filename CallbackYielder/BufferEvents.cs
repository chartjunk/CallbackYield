﻿using System;
using System.Collections;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;

namespace CallbackYielder
{
    public partial class Buffer<TItem>
    {
        private readonly Action<Action<TItem>> _createCallbackMethod;
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

        public Buffer(Action<Action<TItem>> createCallbackMethod)
        {
            _createCallbackMethod = createCallbackMethod;
        }
        
        public IEnumerable<TItem> Enumerate()
        {
            var collection = InstantiateBufferCollection();
            this.StoppingBuffer += (e, a) => collection.CompleteAdding();
            this.Disposing += (e, a) => _doFinally();
            foreach (var action in DoOnBufferActions)
                action(this);

            _createCallbackMethod(item =>
            {
                this.InvokeBeforePushingNewItemEvent();
                collection.Add(item);
                this.InvokeAfterPushingNewItemEvent();
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