using System;

namespace SimpleCache.ConcurrencyTests.ThreadsRunner
{
    internal class UnindexedActionSequence : IActionSequence
    {
        private int _threadsCount;
        private readonly IThreadsRunner _threadsRunner;
        private readonly Action _action;

        public UnindexedActionSequence(
            IThreadsRunner threadsRunner,
            Action action)
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
                PerformAction = (index) => _action(),
                ThreadsCount = _threadsCount
            };
        } 
    }
}