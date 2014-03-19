using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;
using CommunicationXML;

namespace RemoteTester
{
    /// <summary>
    /// PROGRAM TYLKO DO TESTOWANIA POŁĄCZEŃ!!!
    /// </summary>
    class Program
    {
        static void Main(string[] args)
        {
            TcpClient client = new TcpClient("localhost", 22222);

            byte[] data = new Register(NodeType.ComputationalNode, 1, new List<string>() { "test1", "test2" }).GetXmlData();

            NetworkStream ns = client.GetStream();
            ns.Write(data, 0, data.Length);

            List<byte> bytes = new List<byte>();
            byte[] buf = new byte[512];
            int len = 0;
            while ((len = ns.Read(buf, 0, 512)) != 0)
            {
                for (int i = 0; i < len; ++i)
                    bytes.Add(buf[i]);
            }

            XMLParser parser = new XMLParser(bytes.ToArray());
            Console.WriteLine((parser.Message as RegisterResponse).Id);
            Console.WriteLine((parser.Message as RegisterResponse).Timeout);

            ns.Close();
            client.Close();
        }
    }
}
