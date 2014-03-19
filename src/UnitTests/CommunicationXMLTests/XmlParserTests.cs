using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using CommunicationXML;
using System.Collections.Generic;

namespace UnitTests.CommunicationXMLTests
{
    [TestClass]
    public class XmlParserTests
    {
        [TestMethod]
        public void RegisterParseTest()
        {
            //Arrange
            Register register = new Register(NodeType.TaskManager, 120, new List<string>() { "abc", "def" });
            byte[] data = register.GetXmlData();

            //Act
            XMLParser parser = new XMLParser(data);

            //Assert
            Assert.IsNotNull(parser);
            Assert.AreEqual(MessageTypes.Register, parser.MessageType);
            Register result = (Register)parser.Message;
            Assert.AreEqual(result.ParallelThreads, register.ParallelThreads);
            Assert.AreEqual(result.Type, register.Type);
            
        }

        [TestMethod]
        public void RegisterResponseParseTest()
        {
            //Arrange
            RegisterResponse registerResponse = new RegisterResponse(120, new TimeSpan(12,12,12));
            byte[] data = registerResponse.GetXmlData();

            //Act
            XMLParser parser = new XMLParser(data);

            //Assert
            Assert.IsNotNull(parser);
            Assert.AreEqual(MessageTypes.RegisterResponse, parser.MessageType);
            RegisterResponse result = (RegisterResponse)parser.Message;
            Assert.AreEqual(registerResponse.Id, result.Id);
            Assert.AreEqual(registerResponse.Timeout.Hour, result.Timeout.Hour);
            Assert.AreEqual(registerResponse.Timeout.Minute, result.Timeout.Minute);
            Assert.AreEqual(registerResponse.Timeout.Second, result.Timeout.Second);
            Assert.AreEqual(registerResponse.Timeout.Millisecond, result.Timeout.Millisecond);
        }

        [TestMethod]
        public void SolutionRequestParseTest()
        {
            //Arrange
            SolutionRequest sr = new SolutionRequest(123);
            byte[] data = sr.GetXmlData();

            //Act
            XMLParser parser = new XMLParser(data);

            //Assert
            Assert.IsNotNull(parser);
            Assert.AreEqual(MessageTypes.SolutionRequest, parser.MessageType);
            Assert.AreEqual(sr.Id, ((SolutionRequest)parser.Message).Id);
        }
    }
}
