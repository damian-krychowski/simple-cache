using System;

namespace SimpleCache.ConcurrencyTests.ThreadsRunner
{
    internal class IndexedActionSequence : IActionSequence
    {
        private int _threadsCount;
        private readonly IThreadsRunner _threadsRunner;
        private readonly Action<int> _action;

        public IndexedActionSequence(
            IThreadsRunner threadsRunner,
            Action<int> action)
        {
            _threadsRunner = threadsRunner;
            _action = action;
        }

        public IThreadsRunner Threads(int threadsCount)
        {
            _threadsCount = threadsCount;
            return _threadsRunner;
        }

        public Func<ActionSequenceDescription> ToDescriptionFactory()
        {
            return () => new ActionSequenceDescription
            {
                PerformAction = (index) => _action(index),
                ThreadsCount = _threadsCount
            };
        }
    }
}