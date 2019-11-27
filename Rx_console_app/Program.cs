using System;
using System.Collections.Immutable;
using System.Reactive.Disposables;
using System.Reactive.Subjects;

namespace Rx_console_app
{
    public class Market : IObservable<float>
    {
        private ImmutableHashSet<IObserver<float>> observers =
            ImmutableHashSet<IObserver<float>>.Empty;

        public IDisposable Subscribe(IObserver<float> observer)
        {
            observers = observers.Add(observer);
            return Disposable.Create(() =>
            {
                observers = observers.Remove(observer);
            });
        }

        public void Publish(float price)
        {
            foreach (var o in observers)
            {
                o.OnNext(price);
            }
        }
    }

    class Program
    {
        static void Main(string[] args)
        {
            var market1 = new Market();
            var sub = market1.Inspect("market");
            market1.Publish(123.4f);


            // Implement extention methods
            var market = new Subject<float>();
            var marketConsumer = new Subject<float>();

            market.Subscribe(marketConsumer);

            marketConsumer.Inspect("market consumer");

            market.OnNext(1, 2, 3, 4);
            market.OnCompleted();
        }
    }
}
