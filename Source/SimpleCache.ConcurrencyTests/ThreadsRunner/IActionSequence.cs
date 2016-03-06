namespace SimpleCache.ConcurrencyTests.ThreadsRunner
{
    public interface IActionSequence
    {
        IThreadsRunner Threads(int threadsCount);
    }
}