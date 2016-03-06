using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace SimpleCache.ConcurrencyTests.ThreadsRunner
{
    internal class FunctionSequenceDescription : SequenceDescription
    {
        public Action<int> PerformAndStoreAction { get; set; }

        public override IEnumerable<Task> ToTasks()
        {
            for (int i = 0; i < ThreadsCount; i++)
            {
                var threadIndex = i;
                yield return new Task(() => PerformAndStoreAction(threadIndex));
            }
        }
    }
}