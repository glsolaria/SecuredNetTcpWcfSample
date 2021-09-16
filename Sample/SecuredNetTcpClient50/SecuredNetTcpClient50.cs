﻿using System;
using System.Security.Cryptography.X509Certificates;
using System.ServiceModel;
using System.ServiceModel.Channels;
using System.ServiceModel.Security;
using System.Xml;
using Contract;

namespace SecuredNetTcpClient50
{
    static class SecuredNetTcpClient50
    {
        static void Main()
        {
            Console.WriteLine("Client Starting ...");
            
            var encoding = new BinaryMessageEncodingBindingElement
            {
                CompressionFormat = CompressionFormat.None,
                ReaderQuotas = XmlDictionaryReaderQuotas.Max,
            };
            
            var transport = new TcpTransportBindingElement
            {
                TransferMode = TransferMode.Streamed,
                MaxBufferPoolSize = 524288,
                MaxReceivedMessageSize = Convert.ToInt32(Math.Pow(2, 31) - 1),
            };
            
            var ssl = new SslStreamSecurityBindingElement();
            var security = SecurityBindingElement.CreateCertificateOverTransportBindingElement(); 
            
            var binding = new CustomBinding(security, ssl, encoding, transport);
            
            var store = new X509Store(StoreName.TrustedPeople, StoreLocation.LocalMachine);
            store.Open(OpenFlags.ReadOnly);

            X509Certificate2Collection certs = store.Certificates.Find(
                X509FindType.FindBySubjectName, "Temporary WCF Certificate", true
                );

            if (certs.Count == 0)
            {
                throw new Exception("Unable to find WCF certs");
            }

            var securedAddress = new EndpointAddress(
                new Uri("net.tcp://127.0.0.1:44444/IEchoService"), new X509CertificateEndpointIdentity(certs[0])
                );
            var channelFactory = new ChannelFactory<IEchoService>(binding, securedAddress);
            
            channelFactory.Credentials.ClientCertificate.SetCertificate(
                StoreLocation.LocalMachine, 
                StoreName.TrustedPeople, 
                X509FindType.FindBySubjectName, 
                "Temporary WCF Certificate"
                );
            channelFactory.Credentials.ServiceCertificate.Authentication.CertificateValidationMode = 
                X509CertificateValidationMode.PeerOrChainTrust;

            IEchoService channel = channelFactory.CreateChannel();

            var result = channel.Echo("From 5.0 Client");
            
            Console.WriteLine($"result={result}");
        }
    }
}