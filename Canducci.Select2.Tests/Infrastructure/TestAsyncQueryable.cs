using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Threading;
using System.Threading.Tasks;

namespace Canducci.Select2.Tests.Infrastructure
{
    internal sealed class TestAsyncQueryable<T> : IQueryable<T>, IAsyncEnumerable<T>
    {
        private readonly IQueryable<T> _query;

        public TestAsyncQueryable(IEnumerable<T> source)
            : this(source.AsQueryable())
        {
        }

        public TestAsyncQueryable(IQueryable<T> query)
        {
            _query = query ?? throw new ArgumentNullException(nameof(query));
            Provider = new TestAsyncQueryProvider(_query.Provider);
        }

        public Type ElementType => typeof(T);

        public Expression Expression => _query.Expression;

        public IQueryProvider Provider { get; }

        public IEnumerator<T> GetEnumerator()
        {
            return _query.GetEnumerator();
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }

        public IAsyncEnumerator<T> GetAsyncEnumerator(CancellationToken cancellationToken = default)
        {
            return new TestAsyncEnumerator<T>(_query.GetEnumerator());
        }
    }

    internal sealed class TestAsyncQueryProvider : IQueryProvider
    {
        private readonly IQueryProvider _provider;

        public TestAsyncQueryProvider(IQueryProvider provider)
        {
            _provider = provider ?? throw new ArgumentNullException(nameof(provider));
        }

        public IQueryable CreateQuery(Expression expression)
        {
            IQueryable query = _provider.CreateQuery(expression);
            Type wrapperType = typeof(TestAsyncQueryable<>).MakeGenericType(query.ElementType);
            return (IQueryable)Activator.CreateInstance(wrapperType, query);
        }

        public IQueryable<TElement> CreateQuery<TElement>(Expression expression)
        {
            return new TestAsyncQueryable<TElement>(_provider.CreateQuery<TElement>(expression));
        }

        public object Execute(Expression expression)
        {
            return _provider.Execute(expression);
        }

        public TResult Execute<TResult>(Expression expression)
        {
            return _provider.Execute<TResult>(expression);
        }
    }

    internal sealed class TestAsyncEnumerator<T> : IAsyncEnumerator<T>
    {
        private readonly IEnumerator<T> _enumerator;

        public TestAsyncEnumerator(IEnumerator<T> enumerator)
        {
            _enumerator = enumerator;
        }

        public T Current => _enumerator.Current;

        public ValueTask<bool> MoveNextAsync()
        {
            return new ValueTask<bool>(_enumerator.MoveNext());
        }

        public ValueTask DisposeAsync()
        {
            _enumerator.Dispose();
            return default;
        }
    }
}