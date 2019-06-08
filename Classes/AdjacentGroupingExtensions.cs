using System;
using System.Linq;
using System.Collections.Generic;

namespace IMP.Shared
{
    internal interface IAdjacentGroupingSource<TSource> : IEnumerable<TSource> { }

    internal static class AdjacentGroupingExtensions
    {
        #region member types definition
        private class AdjacentGroupingSource<TSource> : IAdjacentGroupingSource<TSource>
        {
            #region member varible and default property initialization
            private IEnumerable<TSource> source;
            #endregion

            #region constructors and destructors
            internal AdjacentGroupingSource(IEnumerable<TSource> source)
            {
                this.source = source;
            }
            #endregion

            #region action methods
            public IEnumerator<TSource> GetEnumerator()
            {
                return source.GetEnumerator();
            }

            System.Collections.IEnumerator System.Collections.IEnumerable.GetEnumerator()
            {
                return source.GetEnumerator();
            }
            #endregion
        }

        private class Grouping<TKey, TElement> : IGrouping<TKey, TElement>
        {
            #region member varible and default property initialization
            public TKey Key { get; private set; }

            internal List<TElement> Elements { get; private set; }
            #endregion

            #region constructors and destructors
            internal Grouping(TKey key)
            {
                this.Key = key;
                this.Elements = new List<TElement>();
            }
            #endregion

            #region action methods
            public IEnumerator<TElement> GetEnumerator()
            {
                return this.Elements.GetEnumerator();
            }

            System.Collections.IEnumerator System.Collections.IEnumerable.GetEnumerator()
            {
                return this.Elements.GetEnumerator();
            }
            #endregion
        }
        #endregion

        #region action methods
        public static IAdjacentGroupingSource<TSource> WithAdjacentGrouping<TSource>(this IEnumerable<TSource> source)
        {
            if (source == null)
            {
                throw new ArgumentNullException("source");
            }

            return new AdjacentGroupingSource<TSource>(source);
        }

        public static IEnumerable<IGrouping<TKey, TSource>> GroupBy<TSource, TKey>(this IAdjacentGroupingSource<TSource> source, Func<TSource, TKey> keySelector, IEqualityComparer<TKey> comparer = null)
        {
            if (source == null)
            {
                throw new ArgumentNullException("source");
            }
            if (keySelector == null)
            {
                throw new ArgumentNullException("keySelector");
            }

            return AdjacentGroupingEnumerable(source, keySelector, comparer);
        }
        #endregion

        #region private member functions
        private static IEnumerable<IGrouping<TKey, TSource>> AdjacentGroupingEnumerable<TSource, TKey>(IAdjacentGroupingSource<TSource> source, Func<TSource, TKey> keySelector, IEqualityComparer<TKey> comparer)
        {
            if (comparer == null)
            {
                comparer = EqualityComparer<TKey>.Default;
            }

            using (var en = source.GetEnumerator())
            {
                if (en.MoveNext())
                {
                    var currentGroup = new Grouping<TKey, TSource>(keySelector(en.Current));
                    currentGroup.Elements.Add(en.Current);

                    while (en.MoveNext())
                    {
                        //Test whether current element starts a new group
                        TKey newKey = keySelector(en.Current);

                        if (!comparer.Equals(newKey, currentGroup.Key))
                        {
                            //Yield the previous group and start next one
                            yield return currentGroup;
                            currentGroup = new Grouping<TKey, TSource>(newKey);
                        }

                        //Add element to the current group
                        currentGroup.Elements.Add(en.Current);
                    }

                    //Yield the last group of sequence
                    yield return currentGroup;
                }
            }
        }
        #endregion
    }
}