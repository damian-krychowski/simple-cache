using System;

namespace SimpleCache.Exceptions
{
    public class IndexNotFoundException: Exception
    {
        public IndexNotFoundException()
        {
        }

        public IndexNotFoundException(string message)
            : base(message)
        {
        }

        public IndexNotFoundException(string message, Exception inner)
            : base(message, inner)
        {
        }
    }
}
