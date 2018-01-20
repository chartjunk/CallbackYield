using System;

namespace CallbackYielder
{
    public static class CallbackYielderBuilder
    {
        public static CallbackYielderBufferBuilder<TItem> Buffer<TItem>(Action<Action<TItem>> createCallbackMethod)
            => new CallbackYielderBufferBuilder<TItem>(createCallbackMethod);
    }
}
