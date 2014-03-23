using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;
using CommunicationXML;
using CommunicationNetwork;

namespace RemoteTester
{
    /// <summary>
    /// testowanie NetworkClient
    /// </summary>
    class Program
    {
        static void Main(string[] args)
        {
            try
            {
                NetworkClient nc = new NetworkClient("localhost", 22222);
                if (nc == null)
                {
                    Console.WriteLine("RemoteTester: NetworkClient object equals to null");
                    return;
                }

                byte[] data = new Register(NodeType.ComputationalNode, 1, new List<string>() { "test1", "test2" }).GetXmlData();

                byte[] bytes = nc.Work(data);

                if (bytes != null)
                {
                    XMLParser parser = new XMLParser(bytes.ToArray());
                    Console.WriteLine((parser.Message as RegisterResponse).Id);
                    Console.WriteLine((parser.Message as RegisterResponse).Timeout);
                }
                else Console.WriteLine("RemoteTester: bytes equals to null");
            }
            catch (Exception e) {
                Console.WriteLine(e.Message);
            }
        }
    }
}
