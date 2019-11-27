using Rx_console_app;
using System;
using System.Reactive.Disposables;
using System.Reactive.Linq;
using System.Reactive.Subjects;
using System.Threading;

namespace FundamentalSequenceOperators
{
    public class Market
    {
        public float Price { get; set; }

        public void ChangedPrice(float price)
        {
            PriceChanged.Invoke(this, price);
        }

        public event EventHandler<float> PriceChanged;
    }

    class Program
    {
        public static IObservable<string> BlokingCollection()
        {
            var sub = new ReplaySubject<string>();
            sub.OnNext("1");
            sub.OnCompleted();
            Thread.Sleep(3000);

            return sub;
        }

        public static IObservable<string> NonBloking()
        {
            return Observable.Create<string>(x =>
            {
                x.OnNext("1");
                x.OnCompleted();
                Thread.Sleep(3000);

                return Disposable.Empty;
            });
        }

        static void Main(string[] args)
        {
            var market = new Market();
            var obs = Observable.FromEventPattern<float>(
                x => market.PriceChanged += x,
                x => market.PriceChanged -= x);

            obs.Subscribe(x => Console.WriteLine($"{x.EventArgs} _ {x.Sender}"));

            market.ChangedPrice(1);
            market.ChangedPrice(2);
            market.ChangedPrice(3.5f);

            Console.ReadKey();

            BlokingCollection().Inspect("bloking");
            NonBloking().Inspect("non bloking");
        }
    }
}
