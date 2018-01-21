using System;
using System.Collections.Generic;
using System.Linq;

namespace CallbackYielder
{
    public partial class Buffer<TItem> : IDisposable
    {
        public event EventHandler BeforePushingNewItem;
        public event EventHandler AfterPushingNewItem;
        public event EventHandler YieldingItem;
        public event EventHandler StoppingBuffer;
        public event EventHandler Disposing;

        public void StopBufferingNewItems() => StoppingBuffer?.Invoke(this, null); 

        public void Dispose() => Disposing?.Invoke(this, null);

        internal void InvokeBeforePushingNewItemEvent() => BeforePushingNewItem?.Invoke(this, null);
        internal void InvokeAfterPushingNewItemEvent() => AfterPushingNewItem?.Invoke(this, null);
    }
}