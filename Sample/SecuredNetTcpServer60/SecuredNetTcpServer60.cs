using System;
using System.Security.Cryptography.X509Certificates;
using System.Xml;
using CoreWCF;
using CoreWCF.Channels;
using Microsoft.AspNetCore.Hosting;
using CoreWCF.Configuration;
using CoreWCF.Description;
using CoreWCF.Security;
using Microsoft.AspNetCore;
using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.DependencyInjection;
using Contract;

namespace SecuredNetTcpServer50
{
    class Startup
    {
        public void ConfigureServices(IServiceCollection services)
        {
            services.AddServiceModelServices();
        }
        
        public void Configure(IApplicationBuilder app)
        {
            app
                .UseServiceModel(builder =>
                {
                    void ConfigureSoapService<TService, TContract>(string serviceprefix) where TService : class
                    {
                        builder.AddService<TService>();
                    }

                    ConfigureSoapService<EchoService, IEchoService>(nameof(EchoService));
                    
                    Action<ServiceHostBase> serviceHost = host =>
                    {
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
                            new Uri("net.tcp://127.0.0.1:44444/IEchoService"), 
                            EndpointIdentity.CreateX509CertificateIdentity(certs[0])
                            );

                        var ssl = new SslStreamSecurityBindingElement();
                        var security = SecurityBindingElement.CreateCertificateOverTransportBindingElement();
                        var encoding = new BinaryMessageEncodingBindingElement();
                        var transport = new TcpTransportBindingElement
                        {
                            TransferMode = TransferMode.Streamed,
                        };
                        
                        Binding binding = new CustomBinding(security, ssl, encoding, transport);
                        
                        var secureServiceEndpoint = new ServiceEndpoint(
                            ContractDescription.GetContract<EchoService>(typeof(EchoService), new EchoService()), 
                            binding, 
                            secureEndpoint
                            );
                        
                        host.Description.Endpoints.Add(secureServiceEndpoint);

                        host.Credentials.ServiceCertificate.SetCertificate(
                            StoreLocation.LocalMachine, 
                            StoreName.TrustedPeople, 
                            X509FindType.FindBySubjectName, 
                            "Temporary WCF Certificate"
                            );
                        host.Credentials.ClientCertificate.Authentication.CertificateValidationMode = 
                            X509CertificateValidationMode.PeerOrChainTrust;
                    };
                    builder.ConfigureServiceHostBase<EchoService>(serviceHost);
                });
        }
    }
    
    static class SecuredNetTcpServer50
    {
        static void Main()
        {
            IWebHost host = WebHost
                .CreateDefaultBuilder()
                .UseKestrel()
                .UseNetTcp(44444)
                .UseStartup<Startup>()
                .Build();

            host.Run();
        }
    }
}