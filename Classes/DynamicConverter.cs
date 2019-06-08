using System;
using System.Linq.Expressions;

namespace IMP.Shared
{
    internal static class DynamicConverter<TTo>
    {
        #region member types definition
        private static class ConverterFrom<TFrom>
        {
            #region member varible and default property initialization
            internal static readonly Func<TFrom, TTo> s_Converter = CreateExpression<TFrom, TTo>(value => Expression.Convert(value, typeof(TTo)));
            #endregion

            #region private member functions
            /// <summary>
            /// Create a function delegate representing an operation
            /// </summary>
            /// <typeparam name="T">The parameter type</typeparam>
            /// <typeparam name="TResult">The return type</typeparam>
            /// <param name="body">Body factory</param>
            /// <returns>Compiled function delegate</returns>
            private static Func<T, TResult> CreateExpression<T, TResult>(Func<ParameterExpression, Expression> body)
            {
                var param = Expression.Parameter(typeof(T), "value");
                try
                {
                    return Expression.Lambda<Func<T, TResult>>(body(param), param).Compile();
                }
                catch (Exception ex)
                {
                    string msg = ex.Message;    //avoid capture of ex itself
                    return _ => { throw new InvalidOperationException(msg); };
                }
            }
            #endregion
        }
        #endregion

        #region action methods
        /// <summary>
        /// Performs a conversion between the given types; this will throw
        /// an InvalidOperationException if the type T does not provide a suitable cast, or for
        /// Nullable&lt;TInner&gt; if TInner does not provide this cast.
        /// </summary>
        public static TTo Convert<TFrom>(TFrom valueToConvert)
        {
            return ConverterFrom<TFrom>.s_Converter(valueToConvert);
        }
        #endregion
    }
}