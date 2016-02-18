using System;
using System.Dynamic;
using System.Linq.Expressions;
using System.Net.Mime;

namespace SimpleCache.Builder
{
    public interface ICacheIndexDefinition
    {
        Type IndexType { get; }
        Expression IndexOn { get; }
    }
}