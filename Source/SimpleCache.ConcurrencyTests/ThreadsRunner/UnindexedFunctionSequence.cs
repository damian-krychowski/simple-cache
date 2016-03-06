using System;
using System.Collections.Concurrent;

namespace SimpleCache.ConcurrencyTests.ThreadsRunner
{
    internal class UnindexedFunctionSequence<TResult> : IFunctionSequence<TResult>
    {
        private readonly IThreadsRunner _threadsRunner;
        private int _threadsCount;
        private Action<int> _performAndStoreAction;
        private readonly Func<TResult> _function;

        public UnindexedFunctionSequence(
            IThreadsRunner threadsRunner,
            Func<TResult> function)
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
            _performAndStoreAction = (index) => storeActionResult(index, _function());
            return _threadsRunner;
        }

        public IThreadsRunner StoreResult(ConcurrentDictionary<int, TResult> resultDictionary)
        {
            _performAndStoreAction = (index) => resultDictionary.TryAdd(index, _function());
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