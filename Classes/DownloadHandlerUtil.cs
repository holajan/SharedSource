using System;
using System.IO;
using System.Web;
using System.Threading.Tasks;

namespace IMP.Shared
{
    /// <summary>
    /// Helper class for writing file content to the HTTP response.
    /// </summary>
    internal static class DownloadHandlerUtil
    {
        #region constants
        private const string HexDigits = "0123456789ABCDEF";
        #endregion

        #region action methods
        /// <summary>
        /// Writes the file content to the response.
        /// </summary>
        /// <param name="response">The HTTP response.</param>
        /// <param name="fileContent">The byte array to send to the response.</param>
        /// <param name="fileDownloadName">Suggested file name for download.</param>
        /// <param name="isAttachment"><c>true</c> if file is attachment; <c>false</c> if file is inline.</param>
        /// <param name="contentType">The content type to use for the response.</param>
        public static void WriteFileToResponse(HttpResponse response, byte[] fileContent, string fileDownloadName, bool isAttachment, string contentType)
        {
            WriteFileToResponseInternal(response, fileContent, fileDownloadName, isAttachment, contentType);
        }

        /// <summary>
        /// Writes the file content to the response.
        /// </summary>
        /// <param name="response">The HTTP response.</param>
        /// <param name="fileContent">The byte array to send to the response.</param>
        /// <param name="fileDownloadName">Suggested file name for download.</param>
        /// <param name="isAttachment"><c>true</c> if file is attachment; <c>false</c> if file is inline.</param>
        public static void WriteFileToResponse(HttpResponse response, byte[] fileContent, string fileDownloadName, bool isAttachment)
        {
            WriteFileToResponseInternal(response, fileContent, fileDownloadName, isAttachment, null);
        }

        /// <summary>
        /// Writes the file content to the response.
        /// </summary>
        /// <param name="response">The HTTP response.</param>
        /// <param name="fileContent">The byte array to send to the response.</param>
        /// <param name="fileDownloadName">Suggested file name for download.</param>
        public static void WriteFileToResponse(HttpResponse response, byte[] fileContent, string fileDownloadName)
        {
            WriteFileToResponseInternal(response, fileContent, fileDownloadName, null, null);
        }

        /// <summary>
        /// Writes the stream to the response.
        /// </summary>
        /// <param name="response">The HTTP response.</param>
        /// <param name="fileStream">The stream to send to the response.</param>
        /// <param name="fileDownloadName">Suggested file name for download.</param>
        /// <param name="isAttachment"><c>true</c> if file is attachment; <c>false</c> if file is inline.</param>
        /// <param name="contentType">The content type to use for the response.</param>
        public static void WriteFileToResponse(HttpResponse response, Stream fileStream, string fileDownloadName, bool isAttachment, string contentType)
        {
            WriteFileToResponseInternal(response, fileStream, fileDownloadName, isAttachment, contentType);
        }

        /// <summary>
        /// Writes the stream to the response.
        /// </summary>
        /// <param name="response">The HTTP response.</param>
        /// <param name="fileStream">The stream to send to the response.</param>
        /// <param name="fileDownloadName">Suggested file name for download.</param>
        /// <param name="isAttachment"><c>true</c> if file is attachment; <c>false</c> if file is inline.</param>
        public static void WriteFileToResponse(HttpResponse response, Stream fileStream, string fileDownloadName, bool isAttachment)
        {
            WriteFileToResponseInternal(response, fileStream, fileDownloadName, isAttachment, null);
        }

        /// <summary>
        /// Writes the stream to the response.
        /// </summary>
        /// <param name="response">The HTTP response.</param>
        /// <param name="fileStream">The stream to send to the response.</param>
        /// <param name="fileDownloadName">Suggested file name for download.</param>
        public static void WriteFileToResponse(HttpResponse response, Stream fileStream, string fileDownloadName)
        {
            WriteFileToResponseInternal(response, fileStream, fileDownloadName, null, null);
        }

        /// <summary>
        /// Writes the stream to the response.
        /// </summary>
        /// <param name="response">The HTTP response.</param>
        /// <param name="fileStream">The stream to send to the response.</param>
        /// <param name="fileDownloadName">Suggested file name for download.</param>
        /// <param name="isAttachment"><c>true</c> if file is attachment; <c>false</c> if file is inline.</param>
        /// <param name="contentType">The content type to use for the response.</param>
        public static Task WriteFileToResponseAsync(HttpResponse response, Stream fileStream, string fileDownloadName, bool isAttachment, string contentType)
        {
            return WriteFileToResponseAsyncInternal(response, fileStream, fileDownloadName, isAttachment, contentType);
        }

        /// <summary>
        /// Writes the stream to the response.
        /// </summary>
        /// <param name="response">The HTTP response.</param>
        /// <param name="fileStream">The stream to send to the response.</param>
        /// <param name="fileDownloadName">Suggested file name for download.</param>
        /// <param name="isAttachment"><c>true</c> if file is attachment; <c>false</c> if file is inline.</param>
        public static Task WriteFileToResponseAsync(HttpResponse response, Stream fileStream, string fileDownloadName, bool isAttachment)
        {
            return WriteFileToResponseAsyncInternal(response, fileStream, fileDownloadName, isAttachment, null);
        }

        /// <summary>
        /// Writes the stream to the response.
        /// </summary>
        /// <param name="response">The HTTP response.</param>
        /// <param name="fileStream">The stream to send to the response.</param>
        /// <param name="fileDownloadName">Suggested file name for download.</param>
        public static Task WriteFileToResponseAsync(HttpResponse response, Stream fileStream, string fileDownloadName)
        {
            return WriteFileToResponseAsyncInternal(response, fileStream, fileDownloadName, null, null);
        }

        /// <summary>
        /// Gets string with MIME protocol Content-Disposition header.
        /// </summary>
        /// <param name="fileDownloadName">Suggested file name for download.</param>
        /// <param name="isAttachment"><c>true</c> if file is attachment; <c>false</c> if file is inline.</param>
        /// <returns>String with MIME protocol Content-Disposition header.</returns>
        public static string GetContentDispositionHeader(string fileDownloadName, bool isAttachment)
        {
            if (fileDownloadName == null)
            {
                throw new ArgumentNullException("fileDownloadName");
            }

            foreach (char ch in fileDownloadName)
            {
                if (ch > '\x007f')
                {
                    //RFC 2231 scheme is needed if the filename contains non-ASCII characters
                    return CreateRfc2231HeaderValue(fileDownloadName, isAttachment);
                }
            }

            //Use RFC 2184 (http://www.apps.ietf.org/rfc/rfc2183.html).
            //The filename will be escaped if, for example, the filename has a space in it then either the whole filename should be quoted (and quotes in the string escaped).
            var disposition = new System.Net.Mime.ContentDisposition() { FileName = fileDownloadName, Inline = !isAttachment };
            return disposition.ToString();
        }

        /// <summary>
        /// Gets string with the HTTP MIME type for file eg. "text/html".
        /// </summary>
        /// <param name="extension">File extension.</param>
        /// <returns>String with the HTTP MIME type eg. "text/html".</returns>
        public static string GetContentType(string extension)
        {
            if (extension == null)
            {
                throw new ArgumentNullException("extension");
            }

            switch (extension.ToLowerInvariant())
            {
                case ".jpg":
                case ".jpeg":
                    return "image/jpeg";
                case ".png":
                    return "image/png";
                case ".gif":
                    return "image/gif";
                case ".bmp":
                    return "image/bmp";
                case ".txt":
                    return "text/plain";
                case ".htm":
                case ".html":
                    return "text/html";
                case ".css":
                    return "text/css";
                case ".xml":
                    return "text/xml";
                case ".doc":
                case ".dot":
                    return "application/msword";
                case ".docx":
                    return "application/vnd.openxmlformats-officedocument.wordprocessingml.document";
                case ".dotx":
                    return "application/vnd.openxmlformats-officedocument.wordprocessingml.template";
                case ".xls":
                case ".xlt":
                    return "application/vnd.ms-excel";
                case ".xlsx":
                    return "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet";
                case ".xltx":
                    return "application/vnd.openxmlformats-officedocument.spreadsheetml.template";
                case ".ppt":
                    return "application/vnd.ms-powerpoint";
                case ".pptx":
                    return "application/vnd.openxmlformats-officedocument.presentationml.presentation";
                case ".pdf":
                    return "application/pdf";
                case ".zip":
                    return "application/x-zip-compressed";
                case ".gz":
                    return "application/x-gzip";
                case ".z":
                    return "application/x-compress";
                case ".rar":
                case ".arj":
                    return "application/x-compressed";
                case ".ics":
                    return "text/v-calendar";
            }

            return "application/octet-stream ";
        }
        #endregion

        #region private member functions
        private static void WriteFileToResponseInternal(HttpResponse response, byte[] fileContent, string fileDownloadName, bool? isAttachment, string contentType)
        {
            if (response == null)
            {
                throw new ArgumentNullException("response");
            }
            if (fileContent == null)
            {
                throw new ArgumentNullException("fileContent");
            }
            if (fileDownloadName == null)
            {
                throw new ArgumentNullException("fileDownloadName");
            }

            if (isAttachment == null)
            {
                isAttachment = IsAttachment(new FileInfo(fileDownloadName).Extension);
            }
            if (contentType == null)
            {
                contentType = GetContentType(new FileInfo(fileDownloadName).Extension);
            }

            response.AppendHeader("content-disposition", GetContentDispositionHeader(fileDownloadName, isAttachment.Value));
            response.ContentType = contentType;

            try
            {
                response.OutputStream.Write(fileContent, 0, fileContent.Length);
            }
            finally
            {
                response.Flush();
            }
        }

        private static void WriteFileToResponseInternal(HttpResponse response, Stream fileStream, string fileDownloadName, bool? isAttachment, string contentType)
        {
            if (response == null)
            {
                throw new ArgumentNullException("response");
            }
            if (fileStream == null)
            {
                throw new ArgumentNullException("fileStream");
            }
            if (fileDownloadName == null)
            {
                throw new ArgumentNullException("fileDownloadName");
            }

            if (isAttachment == null)
            {
                isAttachment = IsAttachment(new FileInfo(fileDownloadName).Extension);
            }
            if (contentType == null)
            {
                contentType = GetContentType(new FileInfo(fileDownloadName).Extension);
            }

            response.AppendHeader("content-disposition", GetContentDispositionHeader(fileDownloadName, isAttachment.Value));
            response.ContentType = contentType;

            try
            {
                using (fileStream)
                {
                    fileStream.CopyTo(response.OutputStream);
                }
            }
            finally
            {
                response.Flush();
            }
        }

        private static async Task WriteFileToResponseAsyncInternal(HttpResponse response, Stream fileStream, string fileDownloadName, bool? isAttachment, string contentType)
        {
            if (response == null)
            {
                throw new ArgumentNullException("response");
            }
            if (fileStream == null)
            {
                throw new ArgumentNullException("fileStream");
            }
            if (fileDownloadName == null)
            {
                throw new ArgumentNullException("fileDownloadName");
            }

            if (isAttachment == null)
            {
                isAttachment = IsAttachment(new FileInfo(fileDownloadName).Extension);
            }
            if (contentType == null)
            {
                contentType = GetContentType(new FileInfo(fileDownloadName).Extension);
            }

            response.AppendHeader("content-disposition", GetContentDispositionHeader(fileDownloadName, isAttachment.Value));
            response.ContentType = contentType;

            try
            {
                using (fileStream)
                {
                    await fileStream.CopyToAsync(response.OutputStream);
                }
            }
            finally
            {
                response.Flush();
            }
        }

        private static bool IsAttachment(string extension)
        {
            switch (extension.ToLowerInvariant())
            {
                case ".jpg":
                case ".jpeg":
                    return false;
                case ".png":
                    return false;
                case ".gif":
                    return false;
                case ".bmp":
                    return false;
                case ".txt":
                    return false;
                case ".htm":
                case ".html":
                    return false;
                case ".css":
                    return false;
            }

            return true;
        }

        private static string CreateRfc2231HeaderValue(string filename, bool isAttachment)
        {
            //Returns the entire filename encoded using the scheme specified in RFC 2231 (http://www.apps.ietf.org/rfc/rfc2231.html), which uses %20 to represent a space.
            //eg. Content-Disposition: attachment;filename*=UTF-8''Q1%20Report.pdf
            var sb = new System.Text.StringBuilder((isAttachment ? "attachment;" : "") + "filename*=UTF-8''");
            foreach (byte num in System.Text.Encoding.UTF8.GetBytes(filename))
            {
                if (IsByteValidHeaderValueCharacter(num))
                {
                    sb.Append((char)num);
                }
                else
                {
                    sb.Append('%');
                    sb.Append(HexDigits[num >> 4]);
                    sb.Append(HexDigits[num % 0x10]);
                }
            }
            return sb.ToString();
        }

        private static bool IsByteValidHeaderValueCharacter(byte num)
        {
            if ((0x30 <= num) && (num <= 0x39))
            {
                return true;
            }
            if ((0x61 <= num) && (num <= 0x7a))
            {
                return true;
            }
            if ((0x41 <= num) && (num <= 0x5a))
            {
                return true;
            }
            switch (num)
            {
                case 0x3a:
                case 0x5f:
                case 0x7e:
                case 0x24:
                case 0x26:
                case 0x21:
                case 0x2b:
                case 0x2d:
                case 0x2e:
                    return true;
            }
            return false;
        }
        #endregion
    }
}