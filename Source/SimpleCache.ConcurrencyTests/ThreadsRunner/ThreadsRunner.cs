using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace SimpleCache.ConcurrencyTests.ThreadsRunner
{
    internal class ThreadsRunner : IThreadsRunner, IThreadsRunnerAwaiter
    {
        private readonly List<Func<FunctionSequenceDescription>> _functionDescriptionFactories=  new List<Func<FunctionSequenceDescription>>(); 
        private readonly List<Func<ActionSequenceDescription>> _actionDescriptionFactories = new List<Func<ActionSequenceDescription>>();

        private Task[] _startedTasks;

        public IFunctionSequence<TResult> PlanExecution<TResult>(Func<TResult> function)
        {
            var sequence = new UnindexedFunctionSequence<TResult>(this, function);

            _functionDescriptionFactories.Add(sequence.ToDescriptionFactory());

            return sequence;
        }

        public IFunctionSequence<TResult> PlanExecution<TResult>(Func<int, TResult> function)
        {
            var sequence = new IndexedFunctionSequence<TResult>(this, function);

            _functionDescriptionFactories.Add(sequence.ToDescriptionFactory());

            return sequence;
        }

        public IActionSequence PlanExecution(Action action)
        {
            var sequence = new UnindexedActionSequence(this, action);

            _actionDescriptionFactories.Add(sequence.ToDescriptionFactory());

            return sequence;
        }

        public IActionSequence PlanExecution(Action<int> action)
        {
            var sequence = new IndexedActionSequence(this, action);

            _actionDescriptionFactories.Add(sequence.ToDescriptionFactory());

            return sequence;
        }

        public void StartAndWaitAll()
        {
            StartWithoutWaiting();
            WaitAll();
        }

        public IThreadsRunnerAwaiter StartWithoutWaiting()
        {
            _startedTasks = GetPlannedTasks()
                .ToArray();
 
            foreach (var task in _startedTasks)
            {
                task.Start();
            }
            
            return this;
        }

        private IEnumerable<Task> GetPlannedTasks()
        {
            foreach (var funcDesc in _functionDescriptionFactories)
            {
                var desc = funcDesc();

                foreach (var task in desc.ToTasks())
                {
                    yield return task;
                }
            }

            foreach (var actionDesc in _actionDescriptionFactories)
            {
                var desc = actionDesc();

                foreach (var task in desc.ToTasks())
                {
                    yield return task;
                }
            }
        }

        public void WaitAll()
        {
            Task.WaitAll(_startedTasks);
        }
    }
}
