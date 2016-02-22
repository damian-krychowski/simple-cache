using System.Collections.Generic;
using System.Threading.Tasks;

namespace SimpleCache.ConcurrencyTests.ThreadsRunner
{
    internal abstract class SequenceDescription
    {
        public int ThreadsCount { get; set; }
        public abstract IEnumerable<Task> ToTasks();
    }
}