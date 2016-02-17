using System;
using System.Dynamic;
using System.Linq.Expressions;
using System.Net.Mime;

namespace SimpleCache.Builder
{
    public interface ICacheIndexDefinition2D
    {
        Type FirstIndexOn { get; }
        Type SecondIndexOn { get; }

        Expression IndexOnFirstProperty { get; }
        Expression IndexOnSecondProperty { get; }
    }

    public interface ICacheIndexDefinition1D
    {
        Type FirstIndexOn { get; }
        Expression IndexOnProperty { get; }
    }
}