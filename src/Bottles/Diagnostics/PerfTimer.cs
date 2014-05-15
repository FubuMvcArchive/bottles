using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using FubuCore.Util;
using FubuCore.Util.TextWriting;

namespace Bottles.Diagnostics
{
    public interface IPerfTimer
    {
        void Record(string text, Action action);
        T Record<T>(string text, Func<T> func);
    }

    public class PerfTimer : IPerfTimer
    {
        public static readonly string Started = "Started";
        public static readonly string Finished = "Finished";
        public static readonly string Marked = "Marked";

        private readonly Stopwatch _stopwatch = new Stopwatch();
        private readonly IList<Checkpoint> _checkpoints = new List<Checkpoint>();
        private string _description;

        public void Start(string description)
        {
            _description = description;
            

            _stopwatch.Reset();
            _checkpoints.Clear();

            _checkpoints.Add(new Checkpoint(Started, _description, 0));

            _stopwatch.Start();
            
        }

        public void Stop()
        {
            
            _stopwatch.Stop();
            add(Finished, _description);
        }

        public long TimeEllapsedInMilliseconds()
        {
            return _stopwatch.ElapsedMilliseconds;
        }

        private void add(string status, string text)
        {
            var checkpoint = new Checkpoint(status, text, _stopwatch.ElapsedMilliseconds);
            _checkpoints.Add(checkpoint);
        }

        public void Mark(string text)
        {
            add(Marked, text);
        }

        public void Record(string text, Action action)
        {
            add(Started, text);
            action();
            add(Finished, text);
        }

        public T Record<T>(string text, Func<T> func)
        {
            add(Started, text);
            try
            {
                return func();
            }
            finally
            {
                add(Finished, text);
            }
        }

        public IEnumerable<TimedStep> TimedSteps()
        {
            var steps = new Cache<string, TimedStep>(text => new TimedStep {Text = text});
            _checkpoints.Where(x => x.Status == Started).Each(x => { steps[x.Text].Start = x.Time; });

            _checkpoints.Where(x => x.Status == Finished).Each(x => { steps[x.Text].Finished = x.Time; });

            _checkpoints.Where(x => x.Status == Marked).Each(x => {
                var step = steps[x.Text];
                step.Start = step.Finished = x.Time;
            });

            return steps;
        }

        public void DisplayTimings<T>(Func<TimedStep, T> sort)
        {
            var ordered = TimedSteps().OrderBy(sort).ToArray();

            var writer = new FubuCore.Util.TextWriting.TextReport();
            writer.StartColumns(new Column(ColumnJustification.left, 0, 3), new Column(ColumnJustification.right, 0, 3),
                new Column(ColumnJustification.right, 0, 3) , new Column(ColumnJustification.right, 0, 3));
            writer.AddColumnData("Description", "Start", "Finish", "Duration");
            writer.AddDivider('-');

            ordered.Each(
                x => {
                    writer.AddColumnData(x.Text, x.Start.ToString(), x.Finished.ToString(), x.Duration().ToString());
                });

            writer.WriteToConsole();
        }

        public class Checkpoint
        {
            private readonly string _status;
            private readonly string _text;
            private readonly long _time;

            public Checkpoint(string status, string text, long time)
            {
                _status = status;
                _text = text;
                _time = time;
            }

            public string Status
            {
                get { return _status; }
            }

            public string Text
            {
                get { return _text; }
            }

            public long Time
            {
                get { return _time; }
            }
        }
    }

    public class TimedStep
    {
        public string Text { get; set; }

        public long? Start { get; set; }

        public long? Finished { get; set; }

        public long Duration()
        {
            if (Start.HasValue && Finished.HasValue)
            {
                return Finished.Value - Start.Value;
            }

            return 0;
        }
    }
}