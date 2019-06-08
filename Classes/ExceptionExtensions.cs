using System;

namespace IMP.Shared
{
    internal static class ExceptionExtensions
    {
        #region action methods
        public static Exception SetAsyncStackTrace(this Exception ex, string asyncStackTrace)
        {
            if (ex == null)
            {
                throw new ArgumentNullException("ex");
            }

            ex.Data.Add("AsyncStackTrace", asyncStackTrace);
            return ex;
        }

        public static string FormatExceptionInfo(this Exception ex)
        {
            if (ex == null)
            {
                throw new ArgumentNullException("ex");
            }

            var sb = new System.Text.StringBuilder();

            sb.Append(ex.GetType().FullName);
            if (!string.IsNullOrEmpty(ex.Message))
            {
                sb.Append(": ");
                sb.Append(ex.Message);
            }
            var detail = TryGetExceptionDetail(ex);
            if (detail != null)
            {
                sb.Append(Environment.NewLine);
                sb.Append("---> ");
                sb.Append(detail.FormatExceptionInfo());
                sb.Append(Environment.NewLine);
                sb.Append("   --- End of remote service exception stack trace ---");
            }
            if (ex.InnerException != null)
            {
                sb.Append(Environment.NewLine);
                sb.Append("---> ");
                sb.Append(ex.InnerException.FormatExceptionInfo());
                sb.Append(Environment.NewLine);
                sb.Append("   --- End of inner exception stack trace ---");
            }

            string stackTrace = (string)ex.Data["AsyncStackTrace"] ?? ex.StackTrace;
            if (stackTrace != null)
            {
                sb.Append(Environment.NewLine);
                sb.Append(stackTrace);
            }

            return sb.ToString();
        }

        public static string FormatExceptionInfo(this System.ServiceModel.ExceptionDetail detail)
        {
            var sb = new System.Text.StringBuilder();

            sb.Append(detail.Type);
            if (!string.IsNullOrEmpty(detail.Message))
            {
                sb.Append(": ");
                sb.Append(detail.Message);
            }
            if (detail.InnerException != null)
            {
                sb.Append(Environment.NewLine);
                sb.Append("---> ");
                sb.Append(detail.InnerException.FormatExceptionInfo());
                sb.Append(Environment.NewLine);
                sb.Append("   --- End of inner exception stack trace ---");
            }
            if (detail.StackTrace != null)
            {
                sb.Append(Environment.NewLine);
                sb.Append(detail.StackTrace);
            }

            return sb.ToString();
        }
        #endregion

        #region private member functions
        private static System.ServiceModel.ExceptionDetail TryGetExceptionDetail(Exception ex)
        {
            if (!(ex is System.ServiceModel.FaultException))
            {
                return null;
            }

            Type exceptionType = ex.GetType();
            while (exceptionType != null)
            {
                if (exceptionType.IsGenericType && exceptionType.GetGenericTypeDefinition() == typeof(System.ServiceModel.FaultException<>))
                {
                    var property = exceptionType.GetProperty("Detail");
                    return property.GetValue(ex, null) as System.ServiceModel.ExceptionDetail;
                }

                exceptionType = exceptionType.BaseType;
            }
            return null;
        }
        #endregion
    }
}