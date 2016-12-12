using System;
using System.ComponentModel;
using System.Threading;

namespace Picross.Helpers
{
    class ThreadHelper
    {
        private Action onCancel;
        private BackgroundWorker worker;
        public event EventHandler OnBeforeRun;
        public event EventHandler OnAfterRun;

        private bool running;

        public bool Running {
            get { return this.running; }
            private set {
                if (!this.running && value) {
                    this.running = value;
                    this.OnBeforeRun(this, new EventArgs());
                }
                else if (this.running && !value) {
                    this.running = value;
                    this.OnAfterRun(this, new EventArgs());
                }
            }
        }
        public bool Cancelling => this.worker.CancellationPending;

        public bool Stop() {
            if (!this.Running) return false;

            this.worker.CancelAsync();
            this.cleanUp();
            this.onCancel();
            return true;
        }

        public void Run<T>(Func<ThreadHelper, T> body, Action<T> onSuccess, Action onCancel) {
            this.onCancel = onCancel;

            var resultCatcher = new ResultCatcher<T>();

            this.worker = new BackgroundWorker();
            this.worker.DoWork += (o, e) => {
                resultCatcher.Result = body(this);
            };

            this.worker.RunWorkerCompleted += (o, e) => {
                onSuccess(resultCatcher.Result);
                this.cleanUp();
            };

            this.worker.RunWorkerAsync();
            this.Running = true;
        }

        private void cleanUp() {
            this.Running = false;
            this.onCancel = null;
            this.worker = null;
        }

        class ResultCatcher<T>
        {
            public T Result;
        }
    }

    // TODO: remove (if no longer necessary) (or take this one and delete the other... if only I know a way to kill it.
    // Then I could make the callback run on the gui thread possibly. With the result thing.
    class ThreadHelperOld
    {
        private Action onCancel;
        private Thread thread;

        public bool Running { get; private set; }

        public void Stop() {
            if (!this.Running) return;

            this.thread.Abort();
            this.cleanUp();
            this.onCancel();
        }

        public void Run<T>(Func<T> body, Action<T> onSuccess, Action onCancel) {
            if (this.Running) return;

            this.onCancel = onCancel;
            this.thread = new Thread(() => {
                T result = body();
                onSuccess(result);
                this.cleanUp();
            });

            this.thread.Start();
            this.Running = true;
        }

        private void cleanUp() {
            this.Running = false;
            this.onCancel = null;
            this.thread = null;
        }
    }
}
