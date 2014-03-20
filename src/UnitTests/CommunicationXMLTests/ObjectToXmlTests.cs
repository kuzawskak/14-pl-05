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
        public void StringToByteConverterTest()
        {
            //Arrange
            string line = "some random <string>with simple </string> pseudo xml parts";
            string result;
            byte[] data;

            //Act
            data = StringToBytesConverter.GetBytes(line);
            result = StringToBytesConverter.GetString(data);

            //Assert
            Assert.IsTrue(line.Equals(result));
            
        }

        [TestMethod]
        public void RegisterResponseToXmlTest()
        {
            //Arrange
            RegisterResponse rr = new RegisterResponse(0, new TimeSpan(1,1,1));

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

        [TestMethod]
        public void SolutionRequestToXmlTest()
        {
            //Arrange
            SolutionRequest sr = new SolutionRequest(123);

            //Act
            byte[] data = sr.GetXmlData();

            //Assert
            Assert.IsNotNull(data);

            Debug.WriteLine(StringToBytesConverter.GetString(data));
        }

        [TestMethod]
        public void DivideProblemToXmlTest()
        {
            //Arrange
            DivideProblem dp = new DivideProblem("abc", 100, new byte[] { 1, 2, 3, 4 }, 100);
            byte[] data;

            //Act
            data = dp.GetXmlData();

            //Assert
            Assert.IsNotNull(data);

            Debug.WriteLine(StringToBytesConverter.GetString(data));

        }

        [TestMethod]
        public void SolveRequestResponseTest()
        {
            //Arrange
            SolveRequestResponse srr = new SolveRequestResponse(123);

            //Act
            byte[] data = srr.GetXmlData();

            //Assert
            Assert.IsNotNull(data);

            Debug.WriteLine(StringToBytesConverter.GetString(data));
        }

        [TestMethod]
        public void SolveRequestWithoutTimeoutTest()
        {
            //Arrange
            SolveRequest sr = new SolveRequest("name", new byte[] { 1, 2, 3 });

            //Act
            byte[] data = sr.GetXmlData();

            //Assert
            Assert.IsNotNull(data);

            Debug.WriteLine(StringToBytesConverter.GetString(data));
            
        }

        [TestMethod]
        public void SolveRequestWithTimeoutTest()
        {
            //Arrange
            SolveRequest sr = new SolveRequest("name", new byte[] { 1, 2, 3 }, 123);

            //Act
            byte[] data = sr.GetXmlData();

            //Assert
            Assert.IsNotNull(data);

            Debug.WriteLine(StringToBytesConverter.GetString(data));

        }

        [TestMethod]
        public void SolvePartialProblemsWithoutTimeoutTest()
        {
            //Arrange
            SolvePartialProblems spp = new SolvePartialProblems("name", 123, new byte[] { 1, 2, 3 }, null, new List<PartialProblem>() {
                new PartialProblem(123, new byte[]{ 1, 2, 3, 4}),
                new PartialProblem(321, new byte[]{ 4, 3, 2, 1})
            });

            //Act
            byte[] data = spp.GetXmlData();

            //Assert
            Assert.IsNotNull(data);

            Debug.WriteLine(StringToBytesConverter.GetString(data));
        }

        [TestMethod]
        public void SolvePartialProblemsWithTimeoutTest()
        {
            //Arrange
            SolvePartialProblems spp = new SolvePartialProblems("name", 123, new byte[] { 1, 2, 3 }, null, new List<PartialProblem>() {
                new PartialProblem(123, new byte[]{ 1, 2, 3, 4}),
                new PartialProblem(321, new byte[]{ 4, 3, 2, 1})
            });

            //Act
            byte[] data = spp.GetXmlData();

            //Assert
            Assert.IsNotNull(data);

            Debug.WriteLine(StringToBytesConverter.GetString(data));
        }

        [TestMethod]
        public void StatusWithNullsTest()
        {
            //Arrange
            Status s = new Status(123, new List<ComputationalThread>() { 
            new ComputationalThread(ComputationalThreadState.Busy, 100, 300, 200, "name"),
            new ComputationalThread(ComputationalThreadState.Idle, 200, null, null, "name2")});

            //Act
            byte[] data = s.GetXmlData();

            //Assert
            Assert.IsNotNull(data);

            Debug.WriteLine(StringToBytesConverter.GetString(data));
            
        }

    }
}
