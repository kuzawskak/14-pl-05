using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Components;
using System.Text;
using System.Collections.Generic;
using System.Linq;
using CommunicationXML;
using CommunicationNetwork;

namespace UnitTests.CommunicationNetwork
{
    [TestClass]
    public class ClientTests
    {
        [TestMethod]
        public void TestWork() {
            byte[] data = new Register(NodeType.ComputationalNode, 1, new List<string>() { "test1", "test2" }).GetXmlData();
            byte[] bytes = null;
            NetworkClient nc = null;

            // test for valid port firstly
            nc = new NetworkClient("localhost", 22222);

            bytes = nc.Work(data);
            Assert.IsNotNull(bytes);
            XMLParser parser = new XMLParser(bytes.ToArray());
            Assert.IsNotNull((parser.Message as RegisterResponse).Id);
            Assert.IsNotNull((parser.Message as RegisterResponse).Timeout);

            // test if data is null
            Assert.IsNull(nc.Work(null));

            // test for invalid address
            nc = new NetworkClient(null, 22222);
            Assert.IsNull(nc.Work(data));

            // test for invalid port number (negative)
            nc = new NetworkClient("localhost", -1);
            Assert.IsNull(nc.Work(data));

            // test for invalid port (not listening port)
            nc = new NetworkClient("localhost", 22221);
            Assert.IsNull(nc.Work(data));
        }
    }
}
