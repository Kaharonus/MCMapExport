using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Threading;
using System.Threading.Tasks;

namespace MCMapExport.Common {
    public class Job<T> {
        public Job(Func<T> work) {
            Work = work;
        }

        private Func<T> Work { get; }
        public EventHandler<T> Callback { get; set; }
        public bool IsCompleted { get; private set; } = false;
        
        public async Task RunAsync() {
            var result = await Task.Run(() => Work());
            await Task.Run(() => Callback(this, result));
            IsCompleted = true;
        }
    }

    public class JobQueue<T> : IDisposable {
        public bool IsRunning { get; private set; } = true;
        public int WaitTime { get; set; } = 33;

        public int MaxThreadCount { get; set; } = Environment.ProcessorCount / 2;

        private Queue<Job<T>> _jobs = new();
        private List<Job<T>> _runningJobs = new();
        private readonly BackgroundWorker _backgroundWorker;

        public JobQueue() {
            _backgroundWorker = new BackgroundWorker();
            _backgroundWorker.DoWork += BackgroundWorker;
            _backgroundWorker.RunWorkerAsync();
        }

        private void BackgroundWorker(object? sender, DoWorkEventArgs args) {
            while (IsRunning) {
                if (_jobs.Count == 0) {
                    Thread.Sleep(WaitTime * 3);
                }

                
                while (_runningJobs.Count < MaxThreadCount && _jobs.TryDequeue(out var job)) {
                    _runningJobs.Add(job);
                    job.RunAsync();
                }
                _runningJobs.RemoveAll(x => x.IsCompleted);

                Thread.Sleep(WaitTime);
            }
        }


        public void Add(Job<T> job) {
            _jobs.Enqueue(job);
        }
        
        public async Task WaitUntilEmpty() {
            await Task.Run(async () => {
                while (IsRunning && (_jobs.Count != 0 || _runningJobs.Count != 0)) {
                    await Task.Delay(WaitTime);
                }
            });
        }


        public void Dispose() {
            IsRunning = false;
            _backgroundWorker.CancelAsync();
        }
    }
}