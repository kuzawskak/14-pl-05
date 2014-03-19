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

        [TestMethod]
        public void DivideProblemParseTest()
        {
            //Arrange
            DivideProblem dp = new DivideProblem("name", 123, new byte[] { 1, 2, 3 }, 123);
            byte[] data = dp.GetXmlData();

            //Act
            XMLParser parser = new XMLParser(data);

            //Assert
            Assert.IsNotNull(parser);
            Assert.AreEqual(MessageTypes.DivideProblem, parser.MessageType);
            DivideProblem result = (DivideProblem)parser.Message;
            Assert.AreEqual(dp.Id, result.Id);
            Assert.AreEqual(dp.ProblemType, result.ProblemType);
            Assert.AreEqual(dp.ComputationalNodes, result.ComputationalNodes);
            Assert.AreEqual(dp.Data.Length, result.Data.Length);
            
        }

        [TestMethod]
        public void SolveRequestResponseParseTest()
        {
            //Arrange
            SolveRequestResponse srr = new SolveRequestResponse(123);
            byte[] data = srr.GetXmlData();

            //Act
            XMLParser parser = new XMLParser(data);

            //Assert
            Assert.IsNotNull(parser);
            Assert.AreEqual(MessageTypes.SolveRequestResponse, parser.MessageType);
            Assert.AreEqual(srr.Id, ((SolveRequestResponse)parser.Message).Id);
        }

        [TestMethod]
        public void SolveRequestWithoutTimeoutParseTest()
        {
            //Arrange
            SolveRequest sr = new SolveRequest("name", new byte[] { 1, 2 });
            byte[] data = sr.GetXmlData();

            //Act
            XMLParser parser = new XMLParser(data);

            //Assert
            Assert.IsNotNull(parser);
            Assert.AreEqual(MessageTypes.SolveRequest, parser.MessageType);
            SolveRequest result = (SolveRequest)parser.Message;
            Assert.IsNull(result.SolvingTimeout);
            Assert.AreEqual(sr.ProblemType, result.ProblemType);
            Assert.AreEqual(sr.Data.Length, result.Data.Length);
        }

        [TestMethod]
        public void SolveRequestWithTimeoutParseTest()
        {
            //Arrange
            SolveRequest sr = new SolveRequest("name", new byte[] { 1, 2 }, 123);
            byte[] data = sr.GetXmlData();

            //Act
            XMLParser parser = new XMLParser(data);

            //Assert
            Assert.IsNotNull(parser);
            Assert.AreEqual(MessageTypes.SolveRequest, parser.MessageType);
            SolveRequest result = (SolveRequest)parser.Message;
            Assert.AreEqual(sr.ProblemType, result.ProblemType);
            Assert.AreEqual(sr.Data.Length, result.Data.Length);
            Assert.AreEqual(sr.SolvingTimeout, result.SolvingTimeout);
        }

        [TestMethod]
        public void SolvePartialProblemsParseTest()
        {
            //Arrange
            SolvePartialProblems spp = new SolvePartialProblems("name", 123, new byte[] { 1, 2, 3 }, null, new List<PartialProblem>() {
                new PartialProblem(123, new byte[]{ 1, 2, 3, 4}),
                new PartialProblem(321, new byte[]{ 4, 3, 2, 1})
            });

            byte[] data = spp.GetXmlData();

            //Act
            XMLParser parser = new XMLParser(data);

            //Assert
            Assert.IsNotNull(parser);
            Assert.AreEqual(MessageTypes.SolvePartialProblems, parser.MessageType);
            SolvePartialProblems result = (SolvePartialProblems)parser.Message;
            Assert.AreEqual(spp.CommonData.Length, result.CommonData.Length);
            Assert.AreEqual(spp.Id, result.Id);
            Assert.AreEqual(spp.PartialProblems.Count, result.PartialProblems.Count);
            Assert.AreEqual(spp.ProblemType, result.ProblemType);
            Assert.IsNull(result.SolvingTimeout);
            PartialProblem p = spp.PartialProblems[0];
            PartialProblem rp = spp.PartialProblems[0];
            Assert.AreEqual(p.TaskId, rp.TaskId);
            Assert.AreEqual(p.Data.Length, rp.Data.Length);

        }

        [TestMethod]
        public void StatusParseTest()
        {
            //Arrange
            Status s = new Status(123, new List<ComputationalThread>() { 
            new ComputationalThread(ComputationalThreadState.Busy, 100, 300, 200, "name"),
            new ComputationalThread(ComputationalThreadState.Idle, 200, null, null, "name2")});

            byte[] data = s.GetXmlData();

            //Act
            XMLParser parser = new XMLParser(data);

            //Assert
            Assert.IsNotNull(parser);
            Assert.AreEqual(MessageTypes.Status, parser.MessageType);
            Status rs = (Status)parser.Message;
            Assert.AreEqual(s.Id, rs.Id);
            Assert.AreEqual(s.Threads.Count, rs.Threads.Count);
            ComputationalThread t = s.Threads[0];
            ComputationalThread rt = rs.Threads[0];
            Assert.AreEqual(t.HowLong, rt.HowLong);
            Assert.AreEqual(t.ProblemInstanceId, rt.ProblemInstanceId);
            Assert.AreEqual(t.State, rt.State);
            Assert.AreEqual(t.TaskId, rt.TaskId);
        }
    }
}
