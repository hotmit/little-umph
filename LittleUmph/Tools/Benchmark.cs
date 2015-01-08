using System;
using System.Collections;
using System.Collections.Generic;
using System.Text;
using System.Diagnostics;
#if NET35_OR_GREATER
using System.Linq;
#endif

namespace LittleUmph
{
    public class Benchmark
    {
        /// <summary>
        /// List of the all the trials ran so far. Key=StartTime, Value=Duration in ticks.
        /// </summary>
        /// <value>
        /// The trial run duration.
        /// </value>
        public List<long> Trials { get; private set; }
        long _startTime, _endTime;

        /// <summary>
        /// Initializes a new instance of the <see cref="Benchmark" /> class.
        /// </summary>
        public Benchmark()
        {
            Reset();
        }

        /// <summary>
        /// Resets the time and the trails
        /// </summary>
        private void Reset()
        {
            Trials = new List<long>();
            _startTime = 0;
            _endTime = 0;
        }

        /// <summary>
        /// Runs one trial.
        /// </summary>
        /// <param name="action">The action.</param>
        public void Run(Action<Stopwatch> action)
        {
            if (_startTime == 0)
            {
                _startTime = DateTime.Now.Ticks;
            }

            var start = DateTime.Now;
            TimeSpan duration = Tmr.Benchmark(action);

            Trials.Add(duration.Ticks);
        }

        /// <summary>
        /// Runs one trial.
        /// Record the trial to the stat only if the function return true.
        /// </summary>
        /// <param name="action">The action.</param>
        public void Run(Func<Stopwatch, bool> action)
        {
            if (_startTime == 0)
            {
                _startTime = DateTime.Now.Ticks;
            }

            var start = DateTime.Now;
            var result = false;
            TimeSpan duration = Tmr.Benchmark(sw =>
            {
                result = action(sw);
            });

            if (result)
            {
                Trials.Add(duration.Ticks);
            }
        }

        /// <summary>
        /// Gets the total time of all the trials add together.
        /// Not including the time not running in between.
        /// </summary>
        /// <value>
        /// The total.
        /// </value>
        public TimeSpan Total
        {
            get
            {
                long ticks = 0;
                foreach (var t in Trials)
                {
                    ticks += t;
                }
                return new TimeSpan(ticks);
            }
        }

        /// <summary>
        /// Gets the average.
        /// </summary>
        /// <value>
        /// The average.
        /// </value>
        public TimeSpan Average
        {
            get
            {
                var average = Total.Ticks / Trials.Count;
                return new TimeSpan(average);
            }
        }

        /// <summary>
        /// Gets the shortest trial ran.
        /// </summary>
        /// <value>
        /// The min.
        /// </value>
        public TimeSpan Min
        {
            get
            {
                long ticks = 0;
                foreach (var t in Trials)
                {
                    if (t < ticks)
                    {
                        ticks = t;
                    }
                }
                return new TimeSpan(ticks);
            }
        }

        /// <summary>
        /// Gets the longest trial ran.
        /// </summary>
        /// <value>
        /// The max.
        /// </value>
        public TimeSpan Max
        {
            get
            {
                long ticks = 0;
                foreach (var t in Trials)
                {
                    if (t > ticks)
                    {
                        ticks = t;
                    }
                }
                return new TimeSpan(ticks);
            }
        }

        /// <summary>
        /// Number of trials run in one second (counted from the first trial).
        /// </summary>
        /// <value>
        /// The frequency.
        /// </value>
        public double Frequency
        {
            get
            {
                long endTime = _endTime == 0 ? DateTime.Now.Ticks : _endTime;
                long diff = endTime - _startTime;
                TimeSpan timeSpan = new TimeSpan(diff);
                return Trials.Count / (double)timeSpan.TotalSeconds;
            }
        }

        /// <summary>
        /// Stops and set the end time. 
        /// This affect the Freq value.
        /// </summary>
        public void Stop()
        {
            _endTime = DateTime.Now.Ticks;
        }
    }
}
