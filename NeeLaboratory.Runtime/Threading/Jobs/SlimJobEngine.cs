using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.Threading;
using System.Threading.Tasks;

namespace NeeLaboratory.Threading.Jobs
{
    public class SlimJobEngine : IDisposable
    {
        private readonly object _lock = new();
        private readonly Queue<SlimJob> _queue = new();
        private bool _disposedValue;
        private readonly CancellationTokenSource _cancellationTokenSource = new();
        private Task? _task;

        public SlimJobEngine(string name)
        {
        }

        protected virtual void Dispose(bool disposing)
        {
            if (!_disposedValue)
            {
                if (disposing)
                {
                    _cancellationTokenSource.Cancel();

                    lock (_lock)
                    {
                        foreach (var job in _queue)
                        {
                            job.Abort();
                        }
                        _queue.Clear();
                    }
                }
                _disposedValue = true;
            }
        }

        public void Dispose()
        {
            Dispose(disposing: true);
            GC.SuppressFinalize(this);
        }

        public void Invoke(Action action)
        {
            Invoke(action, CancellationToken.None);
        }

        public void Invoke(Action action, CancellationToken cancellationToken)
        {
            var operation = InvokeAsync(action, cancellationToken);
            operation.Wait(cancellationToken);
        }

        public TResult? Invoke<TResult>(Func<TResult> action)
        {
            return Invoke(action, CancellationToken.None);
        }

        public TResult? Invoke<TResult>(Func<TResult> action, CancellationToken cancellationToken)
        {
            var operation = InvokeAsync(action, cancellationToken);
            operation.Wait(cancellationToken);
            return operation.Result;
        }

        public SlimJobOperation InvokeAsync(Action action)
        {
            return InvokeAsync(action, CancellationToken.None);
        }

        public SlimJobOperation InvokeAsync(Action action, CancellationToken cancellationToken)
        {
            var job = new SlimJob(action, cancellationToken);
            Enqueue(job);
            return new SlimJobOperation(job);
        }

        public SlimJobOperation<TResult> InvokeAsync<TResult>(Func<TResult> action)
        {
            return InvokeAsync(action, CancellationToken.None);
        }

        public SlimJobOperation<TResult> InvokeAsync<TResult>(Func<TResult> action, CancellationToken cancellationToken)
        {
            var job = new SlimJob<TResult>(action, cancellationToken);
            Enqueue(job);
            return new SlimJobOperation<TResult>(job);
        }


        protected void Enqueue(SlimJob job)
        {
            lock (_lock)
            {
                if (_disposedValue) return;
                _queue.Enqueue(job);

                if (_task is null)
                {
                    _task = Task.Run(() => Worker(_cancellationTokenSource.Token));
                }
            }
        }

        private void Worker(CancellationToken token)
        {
            while (true)
            {
                if (_disposedValue) throw new ObjectDisposedException(this.GetType().FullName);
                token.ThrowIfCancellationRequested();

                SlimJob job;

                lock (_lock)
                {
                    if (_queue.Count <= 0)
                    {
                        _task = null;
                        return;
                    }
                    job = _queue.Dequeue();
                }

                try
                {
                    job.Invoke();
                }
                catch (OperationCanceledException)
                {
                }
                catch (ObjectDisposedException)
                {
                }
                catch (Exception)
                {
                    throw;
                }
            }
        }
    }
}
