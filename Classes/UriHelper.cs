using System;

namespace IMP.Shared
{
    internal static class UriHelper
    {
        #region action methods
        public static Uri GetUri(Uri baseUri, Uri relativeOrAbsoluteUri)
        {
            if (baseUri == null)
            {
                throw new ArgumentNullException("baseUri");
            }

            if (relativeOrAbsoluteUri == null)
            {
                return baseUri;
            }

            if (relativeOrAbsoluteUri.IsAbsoluteUri)
            {
                return relativeOrAbsoluteUri;
            }
            return GetUri(baseUri, relativeOrAbsoluteUri.OriginalString);
        }

        public static Uri GetUri(Uri baseUri, string path)
        {
            if (baseUri == null)
            {
                throw new ArgumentNullException("baseUri");
            }

            if (string.IsNullOrEmpty(path))
            {
                return baseUri;
            }

            if (path.StartsWith("/", StringComparison.Ordinal) || path.StartsWith(@"\", StringComparison.Ordinal) || path.StartsWith("~", StringComparison.Ordinal))
            {
                int startIndex = 1;
                while (startIndex < path.Length)
                {
                    if ((path[startIndex] != '/') && (path[startIndex] != '\\'))
                    {
                        break;
                    }
                    startIndex++;
                }
                path = path.Substring(startIndex);
            }

            if (!baseUri.AbsoluteUri.EndsWith("/", StringComparison.Ordinal))
            {
                baseUri = new Uri(baseUri.AbsoluteUri + "/");
            }
            return new Uri(baseUri, path);
        }

        public static Uri GetUri(string baseUri, string path)
        {
            if (baseUri == null)
            {
                throw new ArgumentNullException("baseUri");
            }

            return GetUri(new Uri(baseUri), path);
        }
        #endregion
    }
}