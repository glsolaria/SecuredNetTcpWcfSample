using System;
using Contract;

namespace Contract
{
    public class EchoService : IEchoService
    {
        public string Echo(string text)
        {
            Console.WriteLine($"Received text={text}");
            return text;
        }

        public string ComplexEcho(EchoMessage text)
        {
            throw new System.NotImplementedException();
        }

        public string FailEcho(string text)
        {
            throw new System.NotImplementedException();
        }

        public string EchoForPermission(string text)
        {
            throw new System.NotImplementedException();
        }
    }
}