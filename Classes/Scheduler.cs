using System;

namespace IMP.Shared
{
    internal sealed class Scheduler : IDisposable
    {
        #region delegate and events
        /// <summary>
        /// Událost pro spuštění úlohy
        /// </summary>
        public event EventHandler RunTask;
        #endregion

        #region member varible and default property initialization
        private System.Timers.Timer SchedulerTimer;
        private TimeSpan m_StartTime;
        private TimeSpan m_StartDelay;
        private TimeSpan m_Interval;
        private bool m_RunOnStart;
        private bool m_Enabled;

        /// <summary>
        /// Plánovaný čas dalšího spuštění úlohy nebo čas, kdy byla spuštěna aktuálně běžící úloha. Pokud scheduler neběží, vrací vlastnost hodnotu <c>null</c>.
        /// </summary>
        /// <remarks>
        /// Vlastnost je změněna při naplánování prvního spuštění a pak vždy po dokončení úlohy. 
        /// </remarks>
        public DateTime? NextRunTime { get; private set; }
        #endregion

        #region constructors and destructors
        /// <summary>
        /// Konstruktor instance třídy Scheduler
        /// </summary>
        public Scheduler()
        {
            this.SchedulerTimer = new System.Timers.Timer();
            this.SchedulerTimer.Elapsed += new System.Timers.ElapsedEventHandler(SchedulerTimer_Elapsed);
            this.SchedulerTimer.AutoReset = false;

            m_Interval = TimeSpan.FromDays(1);
        }
        #endregion

        #region action methods
        /// <summary>
        /// Spuštění scheduleru a naplánování prvního spuštění úlohy dle nastavení <see cref="StartTime"/>, <see cref="StartDelay"/> a <see cref="RunOnStart"/>
        /// </summary>
        public void Start()
        {
            this.Enabled = true;
        }

        /// <summary>
        /// Zastavení scheduleru
        /// </summary>
        public void Stop()
        {
            this.Enabled = false;
        }

        /// <summary>
        /// Uvolnění prostředků používaných aktuálním objektem
        /// </summary>
        public void Dispose()
        {
            Stop();
            this.SchedulerTimer.Dispose();
        }
        #endregion

        #region property getters/setters
        /// <summary>
        /// Posun času spouštění úlohy od 0:00:00, musí být menší než Interval
        /// </summary>
        public TimeSpan StartTime
        {
            get { return m_StartTime; }
            set
            {
                if (value < TimeSpan.Zero)
                {
                    throw new ArgumentOutOfRangeException("value", "Invalid StartTime value.");
                }
                if (this.Enabled)
                {
                    throw new InvalidOperationException("Scheduler is already running.");
                }

                m_StartTime = value;
            }
        }

        /// <summary>
        /// Minimální doba po volání metody <see cref="Start"/>, po které může být provedeno první spuštění úlohy
        /// </summary>
        public TimeSpan StartDelay
        {
            get { return m_StartDelay; }
            set
            {
                if (value < TimeSpan.Zero)
                {
                    throw new ArgumentOutOfRangeException("value", "Invalid StartDelay value.");
                }
                if (this.Enabled)
                {
                    throw new InvalidOperationException("Scheduler is already running.");
                }

                m_StartDelay = value;
            }
        }

        /// <summary>
        /// Interval opakovaného spouštění úlohy
        /// </summary>
        public TimeSpan Interval
        {
            get { return m_Interval; }
            set
            {
                if (value <= TimeSpan.Zero)
                {
                    throw new ArgumentOutOfRangeException("value", "Invalid Interval value.");
                }
                if (this.Enabled)
                {
                    throw new InvalidOperationException("Scheduler is already running.");
                }

                m_Interval = value;
            }
        }

        /// <summary>
        /// <c>true</c> pro provedení prvního spuštění úlohy ihned po uplynutí <see cref="StartDelay"/> od okamžiku zavolání metody <see cref="Start"/>
        /// </summary>
        public bool RunOnStart
        {
            get { return m_RunOnStart; }
            set
            {
                if (this.Enabled)
                {
                    throw new InvalidOperationException("Scheduler is already running.");
                }

                m_RunOnStart = value;
            }
        }

        /// <summary>
        /// Vrací, zda scheduler běží
        /// </summary>
        public bool Enabled
        {
            get { return m_Enabled; }
            set
            {
                if (m_Enabled != value)
                {
                    if (value)
                    {
                        if (m_StartTime >= m_Interval)
                        {
                            throw new InvalidOperationException("StartTime must be smaller than Interval.");
                        }

                        //První spuštění v první násobek intervalu s posunem StartTime
                        this.NextRunTime = DateTime.Today.Add(m_StartTime);
                        Schedule(m_StartDelay);

                        if (m_RunOnStart)
                        {
                            this.SchedulerTimer.Interval = Math.Max(m_StartDelay.TotalMilliseconds, 50);
                        }
                        this.SchedulerTimer.Start();
                    }
                    else
                    {
                        this.SchedulerTimer.Stop();
                        this.NextRunTime = null;
                    }

                    m_Enabled = value;
                }
            }
        }
        #endregion

        #region private member functions
        private void Schedule(TimeSpan delay = default(TimeSpan))
        {
            DateTime now = DateTime.Now;
            long ticks = (now - this.NextRunTime.Value).Ticks;
            if (ticks >= 0)
            {
                this.NextRunTime = this.NextRunTime.Value.Add(TimeSpan.FromTicks(((ticks / m_Interval.Ticks) + 1) * m_Interval.Ticks));
            }

            TimeSpan dueTime = (this.NextRunTime.Value - now);
            if (dueTime < delay)
            {
                dueTime = delay;
            }
            this.SchedulerTimer.Interval = Math.Max(dueTime.TotalMilliseconds, 50);
        }

        private void SchedulerTimer_Elapsed(object sender, System.Timers.ElapsedEventArgs e)
        {
            try
            {
                var handler = RunTask;
                if (handler != null)
                {
                    handler(sender, EventArgs.Empty);
                }
            }
            finally
            {
                if (m_Enabled)
                {
                    //Naplánování dalšího spuštění
                    Schedule();
                    this.SchedulerTimer.Start();
                }
            }
        }
        #endregion
    }
}