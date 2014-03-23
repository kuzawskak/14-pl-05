using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Components;
using System.Text;
using System.Collections.Generic;
using System.Linq;
using CommunicationXML;
using CommunicationNetwork;
using System.Threading;

namespace UnitTests.CommunicationNetwork
{
    [TestClass]
    public class ClientTests
    {
        delegate bool memcmp(byte[] d1, byte[] d2);
        [TestMethod]
        public void TestWork() {
            NetworkListener.ConnectionHandler ch = (d, cc) => { cc.Send(d); }; 
            byte[] data = new Register(NodeType.ComputationalNode, 1, new List<string>() { "test1", "test2" }).GetXmlData();
            byte[] bytes = null;
            NetworkClient nc = null;
            memcmp mcmp = (d1, d2) => { if (d1.Length != d2.Length) return false;
                                        for (int i = 0; i < d1.Length; ++i)
                                            if (d1[i] != d1[i])
                                                return false;
                                        return true; 
                                        };

            NetworkListener nl = null;

            // *** Listener is not started ***
            nl = new NetworkListener(22222, ch);
            Assert.IsNotNull(nl);
            
            Thread t = new Thread(nl.Start);
            t.Start();

            // test for valid port firstly
            nc = new NetworkClient("localhost", 22222);

            bytes = nc.Work(data);
            Assert.IsNotNull(bytes);
            Assert.IsTrue(mcmp(bytes, data));

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

            nl.Stop();
            t.Abort();
            t.Join();

            // *** listener is not started ***
            nl = new NetworkListener(22222, ch);
            Assert.IsNotNull(nl);

            // test for valid port firstly
            nc = new NetworkClient("localhost", 22222);

            bytes = nc.Work(data);
            Assert.IsNull(bytes);

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
