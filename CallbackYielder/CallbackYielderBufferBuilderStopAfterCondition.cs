using System.Collections.Generic;
using System.Threading.Tasks;
using System.Timers;

namespace CallbackYielder
{
    public class CallbackYielderBufferBuilderStopAfterCondition<TItem>
    {
        private readonly CallbackYielderBufferBuilder<TItem> _bufferBuilder;

        public CallbackYielderBufferBuilderStopAfterCondition(CallbackYielderBufferBuilder<TItem> bufferBuilder)
        {
            _bufferBuilder = bufferBuilder;
        }

        public CallbackYielderBufferBuilder<TItem> NoYieldSince(int seconds)
        {
            var timer = new Timer(seconds * 1000);
            var isEnumerationStarted = false;
            void ResetTimer()
            {
                if(!isEnumerationStarted)
                    timer.Stop();
                    timer.Start();
            }
            _bufferBuilder.DoOnBufferActions.Add(buffer => buffer.BeforePushingNewItem += (e, a) => ResetTimer());
            _bufferBuilder.DoOnBufferActions.Add(buffer => buffer.YieldingItem += (e, a) =>
            {
                isEnumerationStarted = true;
                ResetTimer();
            });
            _bufferBuilder.DoOnBufferActions.Add(buffer => timer.Elapsed += (e, a) =>
            {
                timer.Stop();
                buffer.StopBufferingNewItems();
            });
            _bufferBuilder.DoOnBufferActions.Add(buffer => buffer.Disposing += (e, a) => timer.Dispose());
            return _bufferBuilder;
        }

        public CallbackYielderBufferBuilder<TItem> PushedItemAmountIs(int amount)
        {
            int currentAmount = 0;
            _bufferBuilder.DoOnBufferActions.Add(buffer => buffer.AfterPushingNewItem += (e, a) =>
            {
                if (++currentAmount >= amount)
                    buffer.StopBufferingNewItems();
            });
            return _bufferBuilder;
        }
    }
}