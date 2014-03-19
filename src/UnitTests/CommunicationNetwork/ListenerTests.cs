using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using CommunicationNetwork;
using System.Net.Sockets;

namespace UnitTests.CommunicationNetwork
{
    [TestClass]
    public class ListenerTest
    {
        [TestMethod]
        public void TestConnection()
        {
            //Listener l = new Listener(13000, null);
            //l.Start();
            //TcpClient cli = new TcpClient("127.0.0.1", 13000);
            //NetworkStream ns = cli.GetStream();
            //Byte[] data = System.Text.Encoding.ASCII.GetBytes("random data");
            //ns.Write(data, 0, data.Length);

            //// String to store the response ASCII representation.
            //String responseData = String.Empty;

            //// Read the first batch of the TcpServer response bytesToConvert.
            //Int32 bytes = ns.Read(data, 0, data.Length);
            //responseData = System.Text.Encoding.ASCII.GetString(data, 0, bytes);
            //Console.WriteLine("Received: {0}", responseData);

            //Assert.IsNotNull(bytes);
            //ns.Close();
            //cli.Close();

            Assert.IsTrue(false);
        }
    }
}
