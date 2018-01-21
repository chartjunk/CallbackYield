using System.Collections.Generic;
using System.Threading.Tasks;
using System.Timers;

namespace CallbackYielder
{
    public class StopIf<TItem>
    {
        private readonly Buffer<TItem> _bufferBuilder;

        public StopIf(Buffer<TItem> bufferBuilder)
        {
            _bufferBuilder = bufferBuilder;
        }

        public Buffer<TItem> NoPushSince(int seconds)
        {
            var timer = new Timer(seconds * 1000);
            var isEnumerationStarted = false;
            void ResetTimer()
            {
                if(isEnumerationStarted)
                    timer.Stop();
                    timer.Start();
            }
            _bufferBuilder.DoOnBufferActions.Add(buffer => buffer.BeforePushingNewItem += (e, a) => ResetTimer());
            _bufferBuilder.DoOnBufferActions.Add(buffer => buffer.YieldingItem += (e, a) => isEnumerationStarted = true);
            _bufferBuilder.DoOnBufferActions.Add(buffer => timer.Elapsed += (e, a) =>
            {
                timer.Stop();
                buffer.StopBufferingNewItems();
            });
            _bufferBuilder.DoOnBufferActions.Add(buffer => buffer.Disposing += (e, a) => timer.Dispose());
            return _bufferBuilder;
        }

        public Buffer<TItem> PushedItemAmountIs(int amount)
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