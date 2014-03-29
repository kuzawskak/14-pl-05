using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Components;
using System.Threading.Tasks;
using System.Net.Sockets;
using CommunicationXML;
using System.Collections.Generic;
using CommunicationNetwork;

namespace UnitTests.CommunicationServer
{
    [TestClass]
    public class ServerTests
    {
        [TestMethod]
        public void ServerRegisterMsgTest()
        {
            int port = 22222;
            Server srv = new Server(port, new TimeSpan(0, 0, 10));
            Task t = Task.Factory.StartNew(srv.Start);
            byte[] data = new Register(NodeType.ComputationalNode, 1, new List<string>() { "test1", "test2" }).GetXmlData();
            byte[] bytes = null;
            XMLParser parser = null;
            // *** Valid port and address ***
            NetworkClient nc = new NetworkClient("localhost", port);
            NetworkClient nc2 = new NetworkClient("localhost", port);

            Assert.IsNotNull(srv);
            Assert.IsNotNull(nc);
            Assert.IsNotNull(nc2);

            // first cli
            bytes = nc.Work(data);

            Assert.IsNotNull(bytes);

            parser = new XMLParser(bytes);

            Assert.IsNotNull(parser);
            Assert.AreEqual(parser.MessageType, MessageTypes.RegisterResponse);

            // first cli, trying to connect second time
            bytes = nc.Work(data);

            Assert.IsNotNull(bytes);

            parser = new XMLParser(bytes);

            Assert.IsNotNull(parser);
            Assert.AreEqual(parser.MessageType, MessageTypes.RegisterResponse);

            // second cli
            bytes = nc2.Work(data);

            Assert.IsNotNull(bytes);

            parser = new XMLParser(bytes);

            Assert.IsNotNull(parser);
            Assert.AreEqual(parser.MessageType, MessageTypes.RegisterResponse);

            srv.Stop();
            t.Wait();

            // *** Valid port and address (srv is stopped)
            bytes = nc.Work(data);

            Assert.IsNull(bytes);

            // *** invalid port number ***
            srv = new Server(-1, new TimeSpan(0, 0, 10));
            t = Task.Factory.StartNew(srv.Start);

            bytes = nc.Work(data);
          
            Assert.IsNull(bytes);

            srv.Stop();
            t.Wait();
        }
    }
}
