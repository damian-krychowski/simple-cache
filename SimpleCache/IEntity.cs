using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SimpleCache
{
    public interface IEntity
    {
        Guid Id { get; }
    }

    public interface IEntity<out TKey> where TKey: struct 
    {
        TKey Id { get; }
    }
}
