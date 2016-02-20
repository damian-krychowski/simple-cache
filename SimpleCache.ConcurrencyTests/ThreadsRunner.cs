using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace SimpleCache.ConcurrencyTests
{
    internal class ThreadsRunner<TResult>
    {
        internal class Sequence
        {
            private readonly ThreadsRunner<TResult> _runner;
            private readonly int _sequenceIndex;

            public Sequence(
                ThreadsRunner<TResult> runner,
                int sequenceIndex)
            {
                _runner = runner;
                _sequenceIndex = sequenceIndex;
            }

            public Sequence Threads(int count)
            {
                _runner._sequences[_sequenceIndex].Count = count;
                return this;
            }

            public ThreadsRunner<TResult> StoreResult(Action<int, TResult> storeActionResult)
            {
                _runner._sequences[_sequenceIndex].StoreActionResult = storeActionResult;
                return _runner;
            }

            public ThreadsRunner<TResult> StoreResult(ConcurrentDictionary<int, TResult> resultsDictionary)
            {
                _runner._sequences[_sequenceIndex].StoreActionResult =
                    (index, result) => resultsDictionary.TryAdd(index, result);

                return _runner;
            }
        }

        private abstract class SequenceDescription
        {
            protected readonly ThreadsRunner<TResult> _runner;

            public int Count { get; set; }
            public Action<int, TResult> StoreActionResult { get; set; }

            protected SequenceDescription(ThreadsRunner<TResult> runner)
            {
                _runner = runner;
                Count = 1;
            }

            public abstract IEnumerable<Task> ToTasks(); 
        }

        private class UnindexedSequenceDescription : SequenceDescription
        {
            public Func<TResult> Action { get; set; }

            public UnindexedSequenceDescription(ThreadsRunner<TResult> runner) : base(runner)
            {
            }

            public override IEnumerable<Task> ToTasks()
            {
                for (int i = 0; i < Count; i++)
                {
                    var threadIndex = i;

                    yield return new Task(() =>
                    {
                        _runner._semaphore.WaitOne();
                        var result = Action();
                        StoreActionResult(threadIndex, result);
                    });
                }
            }
        }

        private class IndexedSequenceDescription : SequenceDescription
        {
            public Func<int, TResult> Action { get; set; }

            public IndexedSequenceDescription(ThreadsRunner<TResult> runner) : base(runner)
            {
            }

            public override IEnumerable<Task> ToTasks()
            {
                for (int i = 0; i < Count; i++)
                {
                    var threadIndex = i;

                    yield return new Task(() =>
                    {
                        _runner._semaphore.WaitOne();
                        var result = Action(threadIndex);
                        StoreActionResult(threadIndex, result);
                    });
                }
            }
        }

        private readonly Dictionary<int, SequenceDescription> _sequences = new Dictionary<int, SequenceDescription>(); 
        private Semaphore _semaphore;
        private Task[] _startedTasks;

        public Sequence Run(Func<TResult> action)
        {
            var sequenceIndex = GetNextSequenceIndex();
            var sequence = new Sequence(this, sequenceIndex);
            _sequences.Add(sequenceIndex, new UnindexedSequenceDescription(this) {Action = action});

            return sequence;
        }

        public Sequence Run(Func<int, TResult> action)
        {
            var sequenceIndex = GetNextSequenceIndex();
            var sequence = new Sequence(this, sequenceIndex);
            _sequences.Add(sequenceIndex, new IndexedSequenceDescription(this) { Action = action });

            return sequence;
        }

        private int GetNextSequenceIndex()
        {
            if (_sequences.Keys.Any())
            {
                return _sequences.Keys.Max() + 1;
            }

            return 0;
        }

        private int GetThreadsCount()
        {
            return _sequences.Aggregate(0, (seed, item) => seed + item.Value.Count);
        }

        public void StartAndWaitAll()
        {
            StartWithoutWaiting();
            WaitAll();
        }

        public void StartWithoutWaiting()
        {
            _startedTasks = _sequences.Values
               .SelectMany(seq => seq.ToTasks())
               .ToArray();

            _semaphore = new Semaphore(0, GetThreadsCount());

            foreach (var task in _startedTasks)
            {
                task.Start();
            }

            _semaphore.Release(GetThreadsCount());
        }

        public void WaitAll()
        {
            Task.WaitAll(_startedTasks);
        }
    }
}
