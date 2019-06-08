using System;
using System.Web;
using System.Web.Routing;
using Microsoft.IdentityModel.Claims;

namespace FileAccessWeb
{
    public class DownloadHandler : IHttpHandler
    {
        #region action methods
        /// <summary>
        /// Enables processing of HTTP Web requests by a custom HttpHandler that implements the System.Web.IHttpHandler interface.
        /// </summary>
        /// <param name="context">An System.Web.HttpContext object</param>
        public void ProcessRequest(HttpContext context)
        {
            context.Response.AddHeader("cache-control", "must-revalidate");

            if (!context.User.Identity.IsAuthenticated)
            {
                context.Response.StatusCode = 403;
                context.Response.StatusDescription = "Forbidden";
                return;
            }

            var routeData = RouteTable.Routes.GetRouteData(new HttpContextWrapper(context));
            string file = (string)routeData.Values["name"];

            if (string.IsNullOrWhiteSpace(file))
            {
                context.Response.StatusCode = 400;
                context.Response.StatusDescription = "Bad Request";
                return;
            }

            var claimsIdentity = (IClaimsIdentity)context.User.Identity;
            string katedra = claimsIdentity.Name;

            string directory = context.Server.MapPath("~/App_Data/" + katedra);

            string fileName = System.IO.Path.Combine(directory, file);
            if (!System.IO.File.Exists(fileName))
            {
                context.Response.StatusCode = 404;
                context.Response.StatusDescription = "Not Found";
                return;
            }

            bool isAttachment;
            string contentType = GetContentType(new System.IO.FileInfo(fileName).Extension, out isAttachment);

            context.Response.AppendHeader("content-disposition", string.Format("{0}filename={1}", isAttachment ? "attachment;" : "", EncodeFileNameForMimeHeader(file)));
            context.Response.ContentType = contentType;

            try
            {
                context.Response.WriteFile(fileName);
            }
            finally
            {
                context.Response.Flush();
            }
        }

        /// <summary>
        /// Gets a value indicating whether another request can use the System.Web.IHttpHandler instance.
        /// </summary>
        public bool IsReusable
        {
            get { return true; }
        }
        #endregion

        #region private member functions
        private static string EncodeFileNameForMimeHeader(string fileName)
        {
            var builder = new System.Text.StringBuilder(fileName.Length);
            foreach (char ch in fileName)
            {
                int num = Convert.ToInt32(ch);
                if ((((num >= 0x41) && (num <= 90)) || ((num >= 0x61) && (num <= 0x7a))) || (((num >= 0x30) && (num <= 0x39)) || ((num == 0x20) || (num == 0x2e))))
                {
                    builder.Append(ch);
                }
                else
                {
                    char[] chars = new char[] { ch };
                    foreach (byte num2 in System.Text.Encoding.UTF8.GetBytes(chars))
                    {
                        builder.Append("%");
                        builder.Append(num2.ToString("X", System.Globalization.CultureInfo.InvariantCulture));
                    }
                }
            }
            return builder.ToString();
        }

        private static string GetContentType(string Extension, out bool isAttachment)
        {
            isAttachment = true;
            switch (Extension.ToLowerInvariant())
            {
                case ".jpg":
                case ".jpeg":
                    isAttachment = false;
                    return "image/jpeg";
                case ".png":
                    isAttachment = false;
                    return "image/png";
                case ".gif":
                    isAttachment = false;
                    return "image/gif";
                case ".bmp":
                    isAttachment = false;
                    return "image/bmp";
                case ".txt":
                    isAttachment = false;
                    return "text/plain";
                case ".htm":
                case ".html":
                    isAttachment = false;
                    return "text/html";
                case ".css":
                    isAttachment = false;
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
    }
}