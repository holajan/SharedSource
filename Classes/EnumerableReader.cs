using System;
using System.Collections.Generic;

namespace System.Collections.Generic
{
    internal static class EnumerableReaderExtensions
    {
        #region action methods
        public static EnumerableReader<TSource> GetEnumerableReader<TSource>(this IEnumerable<TSource> source)
        {
            return new EnumerableReader<TSource>(source);
        }
        #endregion
    }

    internal class EnumerableReader<TSource> : IDisposable
    {
        #region member varible and default property initialization
        private IEnumerator<TSource> enumerator;
        #endregion

        #region constructors and destructors
        internal EnumerableReader(IEnumerable<TSource> source)
        {
            if (source == null)
            {
                throw new ArgumentNullException("source");
            }

            var enumerator = source.GetEnumerator();
            bool isEmpty;
            try
            {
                isEmpty = !enumerator.MoveNext();
            }
            catch
            {
                enumerator.Dispose();
                throw;
            }

            if (isEmpty)
            {
                enumerator.Dispose();
                return;
            }

            this.enumerator = enumerator;
        }
        #endregion

        #region action methods
        public TSource GetPeekIf(Func<TSource, bool> predicate)
        {
            if (predicate == null)
            {
                throw new ArgumentNullException("predicate");
            }

            return (this.enumerator == null || !predicate(this.enumerator.Current)) ? default(TSource) : this.enumerator.Current;
        }

        public TResult GetPeekIf<TResult>(Func<TSource, bool> predicate, Func<TSource, TResult> resultSelector)
        {
            if (predicate == null)
            {
                throw new ArgumentNullException("predicate");
            }
            if (resultSelector == null)
            {
                throw new ArgumentNullException("resultSelector");
            }

            return (this.enumerator == null || !predicate(this.enumerator.Current)) ? default(TResult) : resultSelector(this.enumerator.Current);
        }

        public TResult GetPeek<TResult>(Func<TSource, TResult> resultSelector)
        {
            if (resultSelector == null)
            {
                throw new ArgumentNullException("resultSelector");
            }

            return (this.enumerator == null) ? default(TResult) : resultSelector(this.enumerator.Current);
        }

        public TSource ReadIf(Func<TSource, bool> predicate)
        {
            if (predicate == null)
            {
                throw new ArgumentNullException("predicate");
            }

            if (this.enumerator == null || !predicate(this.enumerator.Current))
            {
                return default(TSource);
            }

            TSource result = this.enumerator.Current;

            if (!this.enumerator.MoveNext())
            {
                Close();
            }

            return result;
        }

        public TResult ReadIf<TResult>(Func<TSource, bool> predicate, Func<TSource, TResult> resultSelector)
        {
            if (predicate == null)
            {
                throw new ArgumentNullException("predicate");
            }
            if (resultSelector == null)
            {
                throw new ArgumentNullException("resultSelector");
            }

            if (this.enumerator == null || !predicate(this.enumerator.Current))
            {
                return default(TResult);
            }

            TResult result = resultSelector(this.enumerator.Current);

            if (!this.enumerator.MoveNext())
            {
                Close();
            }

            return result;
        }

        public IEnumerable<TSource> ReadWhile(Func<TSource, bool> predicate)
        {
            if (predicate == null)
            {
                throw new ArgumentNullException("predicate");
            }

            return ReadWhileIterator(predicate);
        }

        public TSource SkipWhile(Func<TSource, bool> predicate)
        {
            if (predicate == null)
            {
                throw new ArgumentNullException("predicate");
            }

            if (this.enumerator == null)
            {
                return default(TSource);
            }

            while (predicate(this.enumerator.Current))
            {
                if (!this.enumerator.MoveNext())
                {
                    Close();
                    return default(TSource);
                }
            }

            return this.enumerator.Current;
        }

        public TResult SkipWhile<TResult>(Func<TSource, bool> predicate, Func<TSource, TResult> resultSelector)
        {
            if (predicate == null)
            {
                throw new ArgumentNullException("predicate");
            }
            if (resultSelector == null)
            {
                throw new ArgumentNullException("resultSelector");
            }

            if (this.enumerator == null)
            {
                return default(TResult);
            }

            while (predicate(this.enumerator.Current))
            {
                if (!this.enumerator.MoveNext())
                {
                    Close();
                    return default(TResult);
                }
            }

            return resultSelector(enumerator.Current);
        }

        public void Close()
        {
            if (this.enumerator != null)
            {
                this.enumerator.Dispose();
                this.enumerator = null;
            }
        }

        void IDisposable.Dispose()
        {
            Close();
        }
        #endregion

        #region property getters/setters
        public TSource Peek
        {
            get
            {
                if (this.enumerator == null)
                {
                    return default(TSource);
                }

                return this.enumerator.Current;
            }
        }

        public bool IsClosed
        {
            get { return this.enumerator == null; }
        }
        #endregion
    
        #region private member functions
        private IEnumerable<TSource> ReadWhileIterator(Func<TSource, bool> predicate)
        {
            if (this.enumerator == null)
            {
                yield break;
            }

            while (predicate(this.enumerator.Current))
            {
                yield return this.enumerator.Current;

                if (!this.enumerator.MoveNext())
                {
                    Close();
                    yield break;
                }
            }
        }
        #endregion
    }
}