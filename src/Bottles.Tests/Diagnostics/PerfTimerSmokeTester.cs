using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Bottles.Diagnostics;
using NUnit.Framework;

namespace Bottles.Tests.Diagnostics
{
    [TestFixture]
    public class PerfTimerSmokeTester
    {
        [Test]
        public void try_it_out()
        {
            var timer = new PerfTimer();
            timer.Start("Watch me go!");

            Thread.Sleep(50);
            timer.Mark("Here, here!");

            var t1 = Task.Factory.StartNew(() => {
                timer.Record("#1", () => {
                    Thread.Sleep(25);
                });
            });

            var t2 = Task.Factory.StartNew(() =>
            {
                timer.Record("#2", () =>
                {
                    Thread.Sleep(80);
                });
            });

            var t3 = Task.Factory.StartNew(() =>
            {
                timer.Record("#3", () =>
                {
                    Thread.Sleep(40);
                });
            });

            var t4 = Task.Factory.StartNew(() =>
            {
                timer.Record("#4", () =>
                {
                    Thread.Sleep(5);
                });
            });

            Task.WaitAll(t1, t2, t3, t4);

            timer.Stop();

            timer.DisplayTimings(x => x.Finished);
        }

        [Test]
        public void try_it_in_full_bottle_run()
        {
            var b1 = new FakeBootstrapper("A", 100);
            b1.Activators.Add(new FakeActivator("A1", 50));
            b1.Activators.Add(new FakeActivator("A2", 75));
            b1.Activators.Add(new FakeActivator("A3", 20));
            b1.Activators.Add(new FakeActivator("A4", 250));

            var b2 = new FakeBootstrapper("B", 65);
            b2.Activators.Add(new FakeActivator("B1", 33));
            b2.Activators.Add(new FakeActivator("B2", 44));
            b2.Activators.Add(new FakeActivator("B3", 55));
            b2.Activators.Add(new FakeActivator("B4", 80));

            PackageRegistry.LoadPackages(x => {
                x.Loader(new FakeLoader("L1", 40));
                x.Loader(new FakeLoader("L2", 30));

                x.Bootstrapper(b1);
                x.Bootstrapper(b2);

                x.Bootstrap("Some work", log => {
                    Thread.Sleep(111);
                    return new IActivator[0];
                });

                x.Continue("Crazy Stuff!", () => Thread.Sleep(33));
            });


            PackageRegistry.Diagnostics.Timer.DisplayTimings(x => x.Finished);
        }

        [Test]
        public void try_it_in_full_bottle_run_with_default_ordering()
        {
            var b1 = new FakeBootstrapper("A", 100);
            b1.Activators.Add(new FakeActivator("A1", 50));
            b1.Activators.Add(new FakeActivator("A2", 75));
            b1.Activators.Add(new FakeActivator("A3", 20));
            b1.Activators.Add(new FakeActivator("A4", 250));

            var b2 = new FakeBootstrapper("B", 65);
            b2.Activators.Add(new FakeActivator("B1", 33));
            b2.Activators.Add(new FakeActivator("B2", 44));
            b2.Activators.Add(new FakeActivator("B3", 55));
            b2.Activators.Add(new FakeActivator("B4", 80));

            PackageRegistry.LoadPackages(x =>
            {
                x.Loader(new FakeLoader("L1", 40));
                x.Loader(new FakeLoader("L2", 30));

                x.Bootstrapper(b1);
                x.Bootstrapper(b2);

                x.Bootstrap("Some work", log =>
                {
                    Thread.Sleep(111);
                    return new IActivator[0];
                });

                x.Continue("Crazy Stuff!", () => Thread.Sleep(33));
            });


            PackageRegistry.Diagnostics.Timer.DisplayTimings();
        }
    }

    public class FakeLoader : IPackageLoader
    {
        private readonly string _description;
        private readonly int _time;

        public FakeLoader(string description, int time)
        {
            _description = description;
            _time = time;
        }

        public IEnumerable<IPackageInfo> Load(IPackageLog log)
        {
            Thread.Sleep(_time);
            return new IPackageInfo[0];
        }

        public override string ToString()
        {
            return "Loader: " + _description;
        }
    }

    public class FakeBootstrapper : IBootstrapper
    {
        private readonly string _description;
        private readonly int _time;
        public readonly IList<IActivator> Activators = new List<IActivator>(); 

        public FakeBootstrapper(string description, int time)
        {
            _description = description;
            _time = time;
        }

        public IEnumerable<IActivator> Bootstrap(IPackageLog log)
        {
            Thread.Sleep(_time);
            return Activators;
        }

        public override string ToString()
        {
            return "Bootstrapper: " + _description;
        }
    }

    public class FakeActivator : IActivator
    {
        private readonly string _description;
        private readonly int _time;

        public FakeActivator(string description, int time)
        {
            _description = description;
            _time = time;
        }

        public void Activate(IEnumerable<IPackageInfo> packages, IPackageLog log)
        {
            Thread.Sleep(_time);
        }

        public override string ToString()
        {
            return "Activator: "+_description;
        }
    }
}