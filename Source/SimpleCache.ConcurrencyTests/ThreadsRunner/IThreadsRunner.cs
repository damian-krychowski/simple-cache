using System;

namespace SimpleCache.ConcurrencyTests.ThreadsRunner
{
    public interface IThreadsRunner
    {
        IFunctionSequence<TResult> PlanExecution<TResult>(Func<TResult> function);
        IFunctionSequence<TResult> PlanExecution<TResult>(Func<int, TResult> function);
        IActionSequence PlanExecution(Action action);
        IActionSequence PlanExecution(Action<int> action);

        void StartAndWaitAll();
        IThreadsRunnerAwaiter StartWithoutWaiting();
    }
}