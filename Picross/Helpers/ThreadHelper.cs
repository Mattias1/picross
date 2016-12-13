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

        public bool Cancel() {
            if (!this.Running) return false;

            this.worker.CancelAsync();
            this.onCancel();
            this.cleanUp();
            return true;
        }

        public void Run<T>(Func<ThreadHelper, T> body, Action<T> onSuccess, Action onCancel) {
            this.onCancel = onCancel;

            var resultCatcher = new ResultCatcher<T>();

            this.worker = new BackgroundWorker();
            this.worker.WorkerSupportsCancellation = true;

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
}
