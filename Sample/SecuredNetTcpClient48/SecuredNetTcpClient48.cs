using System;
using System.Security.Cryptography.X509Certificates;
using System.ServiceModel;
using System.ServiceModel.Channels;
using System.ServiceModel.Description;
using System.ServiceModel.Security;
using System.Xml;
using Contract;

namespace SecuredNetTcpClient48
{
    static class SecuredNetTcpClient48
    {
        static void Main()
        {
            TransportSecurityBindingElement security = 
                SecurityBindingElement.CreateCertificateOverTransportBindingElement();
            
            // Comms between a .NET Framework 4.8 client and a .NET 5.0 CoreWCF server only works if the following
            // option is enabled but this option appears to not be available in .NET 5.0
            //security.EnableUnsecuredResponse = true;
            
            var ssl = new SslStreamSecurityBindingElement();
            var encoding = new BinaryMessageEncodingBindingElement();
            var transport = new TcpTransportBindingElement
            {
                TransferMode = TransferMode.Streamed,
            };
            
            Binding binding = new CustomBinding(security, ssl, encoding, transport);
            
            var store = new X509Store(StoreName.TrustedPeople, StoreLocation.LocalMachine);
            store.Open(OpenFlags.ReadOnly);

            X509Certificate2Collection certs = store.Certificates.Find(
                X509FindType.FindBySubjectName, "Temporary WCF Certificate", true
                );

            if (certs.Count == 0)
            {
                throw new Exception("Unable to find WCF certs");
            }
            
            var remoteAddress = new EndpointAddress(new Uri("net.tcp://127.0.0.1:44444/IEchoService"), EndpointIdentity.CreateX509CertificateIdentity(certs[0]));
            var channelFactory = new ChannelFactory<IEchoService>(binding, remoteAddress);

            channelFactory.Endpoint.Behaviors.Find<ClientCredentials>().ClientCertificate.SetCertificate(
                StoreLocation.LocalMachine, 
                StoreName.TrustedPeople, 
                X509FindType.FindBySubjectName, 
                "Temporary WCF Certificate" 
                );
            channelFactory.Endpoint.Behaviors.Find<ClientCredentials>().ServiceCertificate.Authentication.CertificateValidationMode =
                X509CertificateValidationMode.PeerOrChainTrust;

            IEchoService channel = channelFactory.CreateChannel();

            var result = channel.Echo("From 4.8 Client");
            
            Console.WriteLine($"result={result}");
        }
    }
}