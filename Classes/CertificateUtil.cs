using System;
using System.Collections.Generic;
using System.Security.Cryptography.X509Certificates;

namespace IMP.Cryptography
{
    internal static class CertificateUtil
    {
        #region action methods
        public static X509Certificate2 GetValidCertificate(StoreName storeName, StoreLocation storeLocation, X509FindType findType, object findValue)
        {
            if (findType == X509FindType.FindByThumbprint)
            {
                findValue = TrimCertThumbprint((string)findValue);
            }

            //Get certificate store
            var store = new X509Store(storeName, storeLocation);
            X509Certificate2Collection certificates = null;
            try
            {
                store.Open(OpenFlags.ReadOnly);

                //Select a certificate from the certificate store
                certificates = store.Certificates.Find(findType, findValue, true);
                if (certificates.Count == 1)
                {
                    if (certificates[0].Verify())
                    {
                        return new X509Certificate2(certificates[0]);
                    }

                    throw new InvalidOperationException(string.Format("Requested certificate was found, but X.509 chain validation failed.\r\nFind parameters: StoreName = '{0}', StoreLocation = '{1}', FindType = '{2}', FindValue = '{3}'", storeName, storeLocation, findType, findValue));
                }
                else if (certificates.Count == 0)
                {
                    //Try get invalid certificates
                    foreach (X509Certificate2 certificate in certificates)
                    {
                        certificate.Reset();
                    }
                    certificates = store.Certificates.Find(findType, findValue, false);

                    if (certificates.Count == 0)
                    {
                        throw new InvalidOperationException(string.Format("Cannot find requested certificate.\r\nFind parameters: StoreName = '{0}', StoreLocation = '{1}', FindType = '{2}', FindValue = '{3}'", storeName, storeLocation, findType, findValue));
                    }
                    else
                    {
                        throw new InvalidOperationException(string.Format("Cannot find valid requested certificate ({4} invalid certificates was found).\r\nFind parameters: StoreName = '{0}', StoreLocation = '{1}', FindType = '{2}', FindValue = '{3}'", storeName, storeLocation, findType, findValue, certificates.Count));
                    }
                }
                else
                {
                    throw new InvalidOperationException(string.Format("More than one certificates was found ({4}).\r\nFind parameters: StoreName = '{0}', StoreLocation = '{1}', FindType = '{2}', FindValue = '{3}'", storeName, storeLocation, findType, findValue, certificates.Count));
                }
            }
            finally
            {
                if (certificates != null)
                {
                    foreach (X509Certificate2 certificate in certificates)
                    {
                        certificate.Reset();
                    }
                }

                store.Close();
            }
        }

        public static X509Certificate2Collection SelectStoreCertificate(StoreName storeName, StoreLocation storeLocation, string message, string title, X509SelectionFlag X509SelectionFlag = X509SelectionFlag.SingleSelection)
        {
            //Get certificate store
            var store = new X509Store(storeName, storeLocation);
            store.Open(OpenFlags.ReadOnly);

            var certCollection = store.Certificates.Find(X509FindType.FindByTimeValid, DateTime.Now, false);
            var selection = X509Certificate2UI.SelectFromCollection(certCollection, title, message, X509SelectionFlag);
            
            store.Close();

            if (selection.Count > 0)
            {
                return selection;
            }

            return null;
        }

        public static string TrimCertThumbprint(string certThumbprint)
        {
            string thumbprint = certThumbprint.Replace(" ", "").ToUpperInvariant();
            if (thumbprint[0] == 8206)
            {
                thumbprint = thumbprint.Substring(1);
            }

            return thumbprint;
        }
        #endregion
    }
}
