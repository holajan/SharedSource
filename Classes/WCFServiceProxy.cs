using System;
using System.ServiceModel;
using TestClient.ServiceReference;

namespace TestClient
{
    /// <summary>
    /// Třída pro inizializaci WCFService služby
    /// </summary>
    internal class WCFServiceProxy
    {
        #region constants
        private const string cServiceName = "TestService.svc";
        #endregion
        
        #region member varible and default property initialization
        private Uri ServiceUri;
        #endregion

        #region constructors and destructors
        /// <summary>
        /// Inizializace a předání adresy pro službu
        /// </summary>
        /// <param name="serviceBaseUri">Base adresa služby</param>
        public WCFServiceProxy(Uri serviceBaseUri, string ServiceName)
        {
            if (serviceBaseUri == null)
            {
                throw new ArgumentNullException("serviceBaseUri");
            }

            this.ServiceUri = GetServiceUri(serviceBaseUri, ServiceName);
        }

        /// <summary>
        /// Inizializace a předání adresy pro službu
        /// </summary>
        /// <param name="serviceBaseUri">Base adresa služby</param>
        public WCFServiceProxy(Uri serviceBaseUri)
        {
            if (serviceBaseUri == null)
            {
                throw new ArgumentNullException("serviceBaseUri");
            }

            this.ServiceUri = GetServiceUri(serviceBaseUri, cServiceName);
        }
        #endregion

        #region action methods
        internal string GetServiceInfo()
        {
            using (var service = this.Service)
            {
                return service.GetInfo();
            }
        }
        #endregion

        #region property getters/setters
        private TestServiceClient Service
        {
            get
            {
                if (this.ServiceUri == null)
                {
                    throw new InvalidOperationException("ServiceBaseUri is not set");
                }

                var address = new System.ServiceModel.EndpointAddress(this.ServiceUri);
                var binding = new WSHttpBinding();

                return new TestServiceClient(binding, address);
            }
        }
        #endregion

        #region private member functions
        private static Uri GetServiceUri(Uri serviceBaseUri, string serviceName)
        {
            if (serviceBaseUri == null)
            {
                return null;
            }

            string url = serviceBaseUri.ToString();

            if (!url.EndsWith("/", StringComparison.Ordinal))
            {
                url = url + "/";
            }

            return new Uri(url + serviceName, UriKind.Absolute);
        }
        #endregion
    }
}

namespace TestClient.ServiceReference
{
    #region TestServiceClient class
    internal partial class TestServiceClient : IDisposable
    {
        #region action methods
        public void Dispose()
        {
            try
            {
                if (this.State != CommunicationState.Faulted && this.State != CommunicationState.Closed)
                {
                    this.Close();
                }
                else
                {
                    this.Abort();
                }
            }
            catch (CommunicationException)
            {
                this.Abort();
            }
            catch (TimeoutException)
            {
                this.Abort();
            }
            catch (System.Exception)
            {
                this.Abort();
                throw;
            }
        }
        #endregion
    }
    #endregion
}
