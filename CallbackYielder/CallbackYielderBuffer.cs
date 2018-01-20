using System;
using System.Collections.Generic;
using System.Linq;

namespace CallbackYielder
{
    public class CallbackYielderBuffer<TItem> : IDisposable
    {
        private readonly Func<IEnumerable<TItem>> _enumerate;

        public event EventHandler BeforePushingNewItem;
        public event EventHandler AfterPushingNewItem;
        public event EventHandler YieldingItem;
        public event EventHandler StoppingBuffer;
        public event EventHandler Disposing;


        public CallbackYielderBuffer(Func<IEnumerable<TItem>> enumerate)
        {
            _enumerate = enumerate;
        }

        public void StopBufferingNewItems() => StoppingBuffer?.Invoke(this, null); 

        public IEnumerable<TItem> Enumerate() => _enumerate().Select(i =>
        {
            YieldingItem?.Invoke(this, null);
            return i;
        });

        public void Dispose() => Disposing?.Invoke(this, null);

        internal void InvokeBeforePushingNewItemEvent() => BeforePushingNewItem?.Invoke(this, null);
        internal void InvokeAfterPushingNewItemEvent() => AfterPushingNewItem?.Invoke(this, null);
    }
}