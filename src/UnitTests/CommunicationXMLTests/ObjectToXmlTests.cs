using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using CommunicationXML;
using System.Diagnostics;
using System.Collections.Generic;

namespace UnitTests.CommunicationXMLTests
{
    [TestClass]
    public class ObjectToXmlTests
    {
        [TestMethod]
        public void RegisterResponseToXmlTest()
        {
            //Arrange
            RegisterResponse rr = new RegisterResponse(0, DateTime.Now);

            //Act
            byte[] data = rr.GetXmlData();

            //Assert
            Assert.IsNotNull(data);

            Debug.WriteLine(StringToBytesConverter.GetString(data));
        }

        [TestMethod]
        public void RegisterToXmlTest()
        {
            //Arrange
            Register r = new Register(NodeType.ComputationalNode, 2, new List<string>() { "abc", "def"});

            //Act
            byte[] data = r.GetXmlData();

            //Assert
            Assert.IsNotNull(data);

            Debug.WriteLine(StringToBytesConverter.GetString(data));
        }
    }
}
