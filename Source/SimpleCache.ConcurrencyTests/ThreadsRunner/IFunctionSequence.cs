using System;
using System.Collections.Concurrent;

namespace SimpleCache.ConcurrencyTests.ThreadsRunner
{
    public interface IFunctionSequence<TResult>
    {
        IFunctionSequence<TResult> Threads(int threadsCount);
        IThreadsRunner StoreResult(Action<int, TResult> storeActionResult);
        IThreadsRunner StoreResult(ConcurrentDictionary<int, TResult> resultDictionary);
    }
}