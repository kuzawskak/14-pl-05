using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Components;
using System.Threading.Tasks;
using System.Net.Sockets;
using CommunicationXML;
using System.Collections.Generic;

namespace UnitTests.CommunicationServer
{
    [TestClass]
    public class ServerTests
    {
        [TestMethod]
        public void ServerRegisterMsgTest()
        {
            // TODO: Przerobić i przetestować po testowaniu Listenera
            Server testServ = new Server(12345, new TimeSpan(0, 0, 10));
            Task t = Task.Factory.StartNew(testServ.Start);

            TcpClient c = new TcpClient("localhost", 12345);
            byte[] data = new Register(NodeType.ComputationalNode, 1, new List<string>() { "test1", "test2"}).GetXmlData();
            NetworkStream ns = c.GetStream();
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

            ns.Close();
            c.Close();

            testServ.Stop();

            Assert.IsNotNull(parser);
            Assert.AreEqual(parser.MessageType, MessageTypes.RegisterResponse);
            Assert.AreEqual((parser.Message as RegisterResponse).Id, 1);
        }
    }
}
