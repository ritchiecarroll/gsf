//******************************************************************************************************
//  SharedTimerScheduler.cs - Gbtc
//
//  Copyright � 2016, Grid Protection Alliance.  All Rights Reserved.
//
//  Licensed to the Grid Protection Alliance (GPA) under one or more contributor license agreements. See
//  the NOTICE file distributed with this work for additional information regarding copyright ownership.
//  The GPA licenses this file to you under the MIT License (MIT), the "License"; you may
//  not use this file except in compliance with the License. You may obtain a copy of the License at:
//
//      http://opensource.org/licenses/MIT
//
//  Unless agreed to in writing, the subject software distributed under the License is distributed on an
//  "AS-IS" BASIS, WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied. Refer to the
//  License for the specific language governing permissions and limitations.
//
//  Code Modification History:
//  ----------------------------------------------------------------------------------------------------
//  11/10/2016 - Stephen C. Wills
//       Generated original version of source code.
//
//******************************************************************************************************

using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.ComponentModel;
using System.Text;
using System.Threading;
using GSF.Diagnostics;

namespace GSF.Threading
{
    /// <summary>
    /// Represents a timer manager which is the scheduler of <see cref="SharedTimer"/>.
    /// <see cref="SharedTimer"/> with the same scheduler will use the same ThreadPool thread
    /// to process all <see cref="SharedTimer"/>s in series when they have a common interval.
    /// </summary>
    public sealed class SharedTimerScheduler : IDisposable
    {
        #region [ Members ]

        /// <summary>
        /// Represents a scheduler timer with of a common fire rate.
        /// </summary>
        private class SharedTimerInstance : IDisposable
        {
            #region [ Members ]

            // Fields
            private ConcurrentQueue<WeakAction<DateTime>> m_additionalQueueItems;

            private readonly LinkedList<WeakAction<DateTime>> m_callbacks;
            private readonly Timer m_timer;
            private readonly int m_interval;
            private readonly SharedTimerScheduler m_parentTimer;
            private readonly object m_syncRunning;
            private readonly object m_syncStats;
            private bool m_disposed;

            /// <summary>
            /// A counter of the number of times an interval has been skipped
            /// because the callbacks did not complete before the timer fired again
            /// since the last time it was reset
            /// </summary>
            private long m_skippedIntervals;

            /// <summary>
            /// The total CPU time spent processing the timer events.
            /// since the last time it was reset
            /// </summary>
            private double m_elapsedWorkerTime;
            /// <summary>
            /// The total number of times the timer events have fired 
            /// since the last time it was reset
            /// </summary>
            private int m_elapsedIntervals;

            /// <summary>
            /// The total number of callbacks that have occurred.
            /// </summary>
            private int m_sumOfCallbacks;

            /// <summary>
            /// The count of the number of timer callbacks that exists in this factory.
            /// </summary>
            private int m_sharedTimersCount;

            #endregion

            #region [ Constructors ]

            public SharedTimerInstance(SharedTimerScheduler parentTimer, int interval)
            {
                if (parentTimer == null)
                    throw new ArgumentNullException(nameof(parentTimer));

                if (interval <= 0)
                    throw new ArgumentOutOfRangeException(nameof(interval));

                m_additionalQueueItems = new ConcurrentQueue<WeakAction<DateTime>>();
                m_syncRunning = new object();
                m_syncStats = new object();
                m_parentTimer = parentTimer;
                m_interval = interval;
                m_callbacks = new LinkedList<WeakAction<DateTime>>();
                m_timer = new Timer(Callback, null, interval, interval);
            }

            #endregion

            #region [ Methods ]

            public WeakAction<DateTime> RegisterCallback(Action<DateTime> callback)
            {
                if (m_disposed)
                    throw new ObjectDisposedException(GetType().FullName);

                WeakAction<DateTime> weakAction = new WeakAction<DateTime>(callback);
                m_additionalQueueItems.Enqueue(weakAction);
                return weakAction;
            }

            private void Callback(object state)
            {
                if (m_disposed)
                    return;

                ShortTime fireTime = ShortTime.Now;

                bool lockTaken = false;

                try
                {
                    Monitor.TryEnter(m_syncRunning, 0, ref lockTaken);

                    if (!lockTaken)
                    {
                        lock (m_syncStats)
                        {
                            m_skippedIntervals++;
                        }

                        return;
                    }

                    DateTime fireTimeDatetime = fireTime.UtcTime;
                    int loopCount = 0;

                    LinkedListNode<WeakAction<DateTime>> timerAction = m_callbacks.First;

                    while (timerAction != null)
                    {
                        if (m_disposed)
                            return;

                        // Since removing the linked list item will invalidate the "Next" property, go ahead and store it;
                        LinkedListNode<WeakAction<DateTime>> nextNode = timerAction.Next;

                        try
                        {
                            if (!timerAction.Value.TryInvoke(fireTimeDatetime))
                                m_callbacks.Remove(timerAction);
                        }
                        catch (Exception ex)
                        {
                            m_parentTimer.m_log.Publish(MessageLevel.Warning, MessageFlags.BugReport, "Shared Timer Factory Exception", "Code that uses SharedTimerFactory should handle its own exceptions.", null, ex);
                        }

                        loopCount++;
                        timerAction = nextNode;
                    }

                    lock (m_syncStats)
                    {
                        m_sharedTimersCount = m_callbacks.Count;
                        m_sumOfCallbacks += loopCount;
                        m_elapsedIntervals++;
                        m_elapsedWorkerTime += fireTime.ElapsedMilliseconds();
                    }

                    WeakAction<DateTime> newCallbacks;

                    while (m_additionalQueueItems.TryDequeue(out newCallbacks))
                        m_callbacks.AddLast(newCallbacks);
                }
                finally
                {
                    if (lockTaken)
                        Monitor.Exit(m_syncRunning);
                }
            }

            public void Dispose()
            {
                m_disposed = true;
                m_timer.Dispose();
            }

            public void ResetStats()
            {
                lock (m_syncStats)
                {
                    m_skippedIntervals = 0;
                    m_elapsedIntervals = 0;
                    m_elapsedWorkerTime = 0;
                    m_elapsedIntervals = 0;
                    m_sumOfCallbacks = 0;
                }
            }

            public string StatusMessage()
            {
                lock (m_syncStats)
                {
                    double averageCpuTime = 0;
                    if (m_elapsedIntervals > 0)
                    {
                        averageCpuTime = m_elapsedWorkerTime / m_elapsedIntervals;
                    }
                    return $"Interval: {m_interval} Skipped Intervals: {m_skippedIntervals} Elapsed Intervals: {m_elapsedIntervals} Average CPU Time: {(averageCpuTime).ToString("N2")}ms Sum of Callbacks: {m_sumOfCallbacks} Shared Timers: { m_sharedTimersCount}";
                }
            }

            #endregion
        }

        // Fields
        private readonly Dictionary<int, SharedTimerInstance> m_schedulesByInterval;
        private readonly object m_syncRoot;

        /// <summary>
        /// Since there won't be many shared timers, it will be better to not make this publisher a static instance. 
        /// This will provide the initialization stack so it will be easier to distinguish this instance of StaticTimer 
        /// from other instances.
        /// </summary>
        private readonly LogPublisher m_log;

        private ScheduledTask m_reportStatus;
        private bool m_disposed;

        #endregion

        #region [ Constructors ]

        /// <summary>
        /// Creates a new instance of the <see cref="SharedTimerScheduler"/> class.
        /// </summary>
        public SharedTimerScheduler()
        {
            m_syncRoot = new object();
            m_log = Logger.CreatePublisher(typeof(SharedTimerScheduler), MessageClass.Component);
            m_schedulesByInterval = new Dictionary<int, SharedTimerInstance>();
            m_reportStatus = new ScheduledTask();
            m_reportStatus.Running += ReportStatus;
            m_reportStatus.Start(60 * 1000);
        }

        #endregion

        #region [ Properties ]

        /// <summary>
        /// Gets if this class has been disposed.
        /// </summary>
        public bool IsDisposed => m_disposed;

        #endregion

        #region [ Methods ]

        /// <summary>
        /// Creates a <see cref="SharedTimer"/> using the current <see cref="SharedTimerScheduler"/>.
        /// </summary>
        /// <param name="interval">The interval of the timer, default is 100</param>
        /// <returns>A shared timer instance that fires at the given interval.</returns>
        public SharedTimer CreateTimer(int interval = 100)
        {
            return new SharedTimer(this);
        }

        /// <summary>
        /// Registers the given callback with the timer running at the given interval.
        /// </summary>
        /// <param name="interval">The interval at which to run the timer.</param>
        /// <param name="callback">The action to be performed when the timer is triggered.</param>
        /// <returns>The weak reference callback that will be executed when this timer fires. To unregister
        /// the callback, call <see cref="WeakAction.Clear"/></returns>
        [EditorBrowsable(EditorBrowsableState.Never)]
        internal WeakAction<DateTime> RegisterCallback(int interval, Action<DateTime> callback)
        {
            if (callback == null)
                throw new ArgumentNullException(nameof(callback));

            if (interval <= 0)
                throw new ArgumentOutOfRangeException(nameof(interval));

            if (m_disposed)
                throw new ObjectDisposedException(GetType().FullName);

            lock (m_syncRoot)
            {
                if (m_disposed)
                    throw new ObjectDisposedException(GetType().FullName);

                SharedTimerInstance instance;

                if (!m_schedulesByInterval.TryGetValue(interval, out instance))
                {
                    instance = new SharedTimerInstance(this, interval);
                    m_schedulesByInterval.Add(interval, instance);
                }

                return instance.RegisterCallback(callback);
            }
        }

        /// <summary>
        /// Performs application-defined tasks associated with freeing, releasing, or resetting unmanaged resources.
        /// </summary>
        /// <filterpriority>2</filterpriority>
        public void Dispose()
        {
            // Causes the status to be generated one last time before clearing everything
            m_reportStatus.Dispose();

            lock (m_syncRoot)
            {
                m_disposed = true;

                foreach (SharedTimerInstance instance in m_schedulesByInterval.Values)
                    instance.Dispose();

                m_schedulesByInterval.Clear();
            }

            m_log.Publish(MessageLevel.Warning, "Timer Disposed");
        }

        private void ReportStatus(object sender, EventArgs<ScheduledTaskRunningReason> e)
        {
            m_reportStatus.Start(60 * 1000);

            StringBuilder status = new StringBuilder();

            lock (m_syncRoot)
            {
                status.AppendLine("Shared Timer Factory Status");

                foreach (var item in m_schedulesByInterval)
                {
                    status.AppendLine(item.Value.StatusMessage());
                    item.Value.ResetStats();
                }
            }

            m_log.Publish(MessageLevel.Info, MessageFlags.SystemHealth, "Shared Timer Factory Status", status.ToString());
        }

        #endregion
    }
}