using System;
using System.Collections.Concurrent;

namespace SimpleCache.ConcurrencyTests.ThreadsRunner
{
    internal class IndexedFunctionSequence<TResult> : IFunctionSequence<TResult>
    {
        private readonly IThreadsRunner _threadsRunner;
        private readonly Func<int, TResult> _function;
        private int _threadsCount;
        private Action<int> _performAndStoreAction;

        public IndexedFunctionSequence(
            IThreadsRunner threadsRunner,
            Func<int, TResult> function)
        {
            _threadsRunner = threadsRunner;
            _function = function;
        }

        public IFunctionSequence<TResult> Threads(int threadsCount)
        {
            _threadsCount = threadsCount;
            return this;
        }

        public IThreadsRunner StoreResult(Action<int, TResult> storeActionResult)
        {
            _performAndStoreAction = (index) => storeActionResult(index, _function(index));
            return _threadsRunner;
        }

        public IThreadsRunner StoreResult(ConcurrentDictionary<int, TResult> resultDictionary)
        {
            _performAndStoreAction = (index) => resultDictionary.TryAdd(index, _function(index));
            return _threadsRunner;
        }

        public Func<FunctionSequenceDescription> ToDescriptionFactory()
        {
            return () => new FunctionSequenceDescription
            {
                PerformAndStoreAction = _performAndStoreAction,
                ThreadsCount = _threadsCount
            };
        }
    }
}