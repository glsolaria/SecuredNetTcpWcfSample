using System;
using System.Security.Cryptography.X509Certificates;
using System.ServiceModel;
using System.ServiceModel.Channels;
using System.ServiceModel.Description;
using System.ServiceModel.Security;
using System.Xml;
using Contract;

namespace SecuredNetTcpServer48
{
    static class SecuredNetTcpServer48
    {
        static void Main(string[] args)
        {
            string uri = "net.tcp://127.0.0.1:44444/IEchoService";
            
            var host = new ServiceHost(typeof(EchoService));

            var security = SecurityBindingElement.CreateCertificateOverTransportBindingElement();
            var ssl = new SslStreamSecurityBindingElement();
            var encoding = new BinaryMessageEncodingBindingElement();
            var transport = new TcpTransportBindingElement
            {
                TransferMode = TransferMode.Streamed,
            };
            
            var binding = new CustomBinding(security, ssl, encoding, transport);

            ServiceEndpoint endpoint = host.AddServiceEndpoint(typeof(IEchoService), binding, uri);
            
            host.Credentials.ServiceCertificate.SetCertificate(
                StoreLocation.LocalMachine, 
                StoreName.TrustedPeople, 
                X509FindType.FindBySubjectName, 
                "Temporary WCF Certificate" 
                );
            host.Credentials.ClientCertificate.Authentication.CertificateValidationMode = 
                X509CertificateValidationMode.PeerOrChainTrust;
            
            var store = new X509Store(StoreName.TrustedPeople, StoreLocation.LocalMachine);
            store.Open(OpenFlags.ReadOnly);

            X509Certificate2Collection certs = store.Certificates.Find(
                X509FindType.FindBySubjectName, "Temporary WCF Certificate", true
                );

            if (certs.Count == 0)
            {
                throw new Exception("Unable to find WCF certs");
            }

            var secureEndpoint = new EndpointAddress(
                new Uri(uri), EndpointIdentity.CreateX509CertificateIdentity(certs[0])
                );

            endpoint.Address = secureEndpoint;
            
            host.Open();

            Console.WriteLine($"Listening on {uri}. Hit any key to stop");
            Console.ReadLine();
        }
    }
}