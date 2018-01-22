using System;
using System.Collections.Generic;
using System.Linq;

namespace CallbackYielder
{
    public partial class Buffer<TItem>
    {
        public event EventHandler BeforePushingNewItem;
        public event EventHandler AfterPushingNewItem;
        public event EventHandler YieldingItem;
        public event EventHandler StoppingBuffer;

        public void StopBufferingNewItems() => StoppingBuffer?.Invoke(this, null); 
        internal void InvokeBeforePushingNewItemEvent() => BeforePushingNewItem?.Invoke(this, null);
        internal void InvokeAfterPushingNewItemEvent() => AfterPushingNewItem?.Invoke(this, null);
    }
}