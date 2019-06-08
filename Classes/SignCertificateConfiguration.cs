using System;
using System.Security.Cryptography.X509Certificates;

namespace IMP.Cryptography
{
    internal class SignCertificateConfigurationSection : System.Configuration.ConfigurationSection
    {
        #region constants
        private const string cSignCertificateElementName = "signCertificate";
        #endregion

        #region member varible and default property initialization
        private System.Configuration.ConfigurationPropertyCollection properties;
        #endregion

        #region property getters/setters
        public X509Certificate2 SignCertificate
        {
            get
            {
                var element = this.SignCertificateReference;
                if (string.IsNullOrWhiteSpace(element.FindValue))
                {
                    throw new System.Configuration.ConfigurationErrorsException("Sign certificate configuration is missing.");
                }

                return CertificateUtil.GetValidCertificate(element.StoreName, element.StoreLocation, element.X509FindType, element.FindValue);
            }
        }
        #endregion

        #region private member functions
        protected override System.Configuration.ConfigurationPropertyCollection Properties
        {
            get
            {
                if (properties == null)
                {
                    properties = new System.Configuration.ConfigurationPropertyCollection 
                    { 
                        new System.Configuration.ConfigurationProperty(cSignCertificateElementName, 
                                        typeof(System.ServiceModel.Configuration.CertificateReferenceElement), null, 
                                        System.Configuration.ConfigurationPropertyOptions.IsRequired)
                    };
                }

                return properties;
            }
        }

        private System.ServiceModel.Configuration.CertificateReferenceElement SignCertificateReference
        {
            get { return (System.ServiceModel.Configuration.CertificateReferenceElement)this[cSignCertificateElementName]; }
        }
        #endregion
    }

    internal static class SignCertificateConfiguration
    {
        #region property getters/setters
        public static X509Certificate2 SignCertificate
        {
            get
            {
                var configuration = (SignCertificateConfigurationSection)System.Configuration.ConfigurationManager.GetSection("signCertificateConfiguration");
                if (configuration == null)
                {
                    throw new System.Configuration.ConfigurationErrorsException("Configuration section 'signCertificateConfiguration' not found.");
                }

                return configuration.SignCertificate;
            }
        }
        #endregion
    }
}