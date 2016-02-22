namespace SimpleCache.ConcurrencyTests.ThreadsRunner
{
    public static class ThreadsRunnerFactory
    {
        public static IThreadsRunner Create()
        {
            return new ThreadsRunner();
        }
    }
}