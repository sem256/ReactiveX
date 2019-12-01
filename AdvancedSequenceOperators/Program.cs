using Autofac;
using System;
using System.Reactive.Linq;
using System.Reactive.Subjects;

namespace AdvancedSequenceOperators
{
    public class Actor
    {
        protected EventBroker broker;

        public Actor(EventBroker broker)
        {
            this.broker = broker;
        }
    }

    public class FootballPlayer : Actor
    {
        public string Name { get; set; }
        public int Goals { get; set; }

        public FootballPlayer(EventBroker broker, string name ) : base(broker)
        {
            Name = name;
            broker.OfType<PlayerScoreEvent>()
                .Where(x => !x.Name.Equals(name))
                .Subscribe(pe =>
                {
                    Console.WriteLine($"{name}: Nice {pe.Name}, {pe.GoalScored} goal.");
                });

            broker.OfType<PlayerSentOffEvent>()
                .Where(x => !x.Name.Equals(name))
                .Subscribe(pe =>
                {
                    Console.WriteLine($"{name}: see you {pe.Name}.");
                });
        }

        public void Scored()
        {
            Goals++;
            broker.Publish(new PlayerScoreEvent() { Name = Name, GoalScored = Goals });
        }

        public void AssaultReferee()
        {
            broker.Publish(new PlayerSentOffEvent() { Name = Name, Reason = "Reason" });
        }
    }

    public class FootballCoach : Actor
    {
        public FootballCoach(EventBroker broker) : base(broker)
        {
            broker.OfType<PlayerScoreEvent>()
                .Subscribe(pe =>
                {
                    if(pe.GoalScored < 3)
                    {
                        Console.WriteLine($"Coach: well done {pe.Name}!");
                    }
                });
            broker.OfType<PlayerSentOffEvent>()
                .Subscribe(pe =>
                {
                    if(pe.Reason == "violence")
                    {
                        Console.WriteLine($"Coach: how could you, {pe.Name}!");
                    }
                });
        }
    }

    public class EventBroker : IObservable<PlayerEvent>
    {
        private Subject<PlayerEvent> subscriptions = new Subject<PlayerEvent>();
        
        public IDisposable Subscribe(IObserver<PlayerEvent> observer)
        {
            return subscriptions.Subscribe(observer);
        }

        public void Publish(PlayerEvent pe)
        {
            subscriptions.OnNext(pe);
        }
    }

    public class PlayerEvent
    {
        public string Name { get; set; }
    }

    public class PlayerScoreEvent : PlayerEvent
    {
        public int GoalScored { get; set; }
    }

    public class PlayerSentOffEvent : PlayerEvent
    {
        public string Reason { get; set; }
    }

    class Program
    {
        static void Main(string[] args)
        {
            var cb = new ContainerBuilder();
            cb.RegisterType<EventBroker>().SingleInstance();
            cb.RegisterType<FootballCoach>();
            cb.Register((c, p) =>
            {
                return new FootballPlayer(
                    c.Resolve<EventBroker>(),
                    p.Named<string>("name"));
            });

            using(var c = cb.Build())
            {
                var coach = c.Resolve<FootballCoach>();
                var player1 = c.Resolve<FootballPlayer>(new NamedParameter("name", "Jon"));
                var player2 = c.Resolve<FootballPlayer>(new NamedParameter("name", "Mark"));

                player1.Scored();
                player1.Scored();
                player1.Scored();
                player1.AssaultReferee();
                player2.Scored();
            }

            Console.ReadKey();
        }
    }
}
