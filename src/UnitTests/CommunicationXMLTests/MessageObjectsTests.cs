using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using CommunicationXML;

namespace UnitTests.CommunicationXMLTests
{
    [TestClass]
    public class MessageObjectsTests
    {
        [TestMethod]
        public void RegisterConstructorTest()
        {
            //Arrange
            string[] problemNames = new string[] { "abc", "def", "ghi" };

            //Act
            Register register = new Register(NodeType.ComputationalNode, 2, problemNames);

            //Assert
            Assert.AreEqual(register.SolvableProblems.Count, problemNames.Length);
            Assert.IsTrue(register.SolvableProblems.Contains(problemNames[0]));
        }

        [TestMethod]
        public void RegisterResponseConstructorTest()
        {
            //Arrange
            RegisterResponse registerResponse;

            //Act
            registerResponse = new RegisterResponse((ulong)0, new DateTime());

            //Assert
            Assert.IsNotNull(registerResponse);
        }

        [TestMethod]
        public void ComputationalThreadConstructorTest1()
        {
            //Arrange
            ComputationalThread thread;

            //Act
            thread = new ComputationalThread();

            //Assert
            Assert.IsNotNull(thread);
        }

        [TestMethod]
        public void ComputationalThreadConstructorTest2()
        {
            //Arrange
            ComputationalThread thread;

            //Act
            thread = new ComputationalThread(ComputationalThreadState.Busy, 300, 1, 2, "type");

            //Assert
            Assert.IsNotNull(thread);
        }

        [TestMethod]
        public void StatusConstructorTest1()
        {
            //Arrange
            ComputationalThread [] threads = new ComputationalThread[] 
            { new ComputationalThread(), 
                new ComputationalThread(ComputationalThreadState.Busy, 1, 1, 1, "name") };

            //Act
            Status status = new Status(1, threads);

            //Assert
            Assert.IsNotNull(status);
        }

        [TestMethod]
        public void StatusConstructorTest2()
        {
            //Arrange
            Status status;

            //Act
            status = new Status(1);

            //Assret
            Assert.IsNotNull(status);
        }

        [TestMethod]
        public void SolveRequestConstructorTest1()
        {
            //Arrange
            SolveRequest request;

            //Act
            request = new SolveRequest("type", new byte[0], 1);

            //Assert
            Assert.IsNotNull(request);
        }

        [TestMethod]
        public void SolveRequestConstructorTest2()
        {
            //Arrange
            SolveRequest request;

            //Act
            request = new SolveRequest("type", new byte[0]);

            //Assert
            Assert.IsNotNull(request);
        }

        [TestMethod]
        public void SolveRequestConstructorTest3()
        {
            //Arrange
            SolveRequest request;

            //Act
            request = new SolveRequest();

            //Assert
            Assert.IsNotNull(request);
        }

        [TestMethod]
        public void SolveRequestResponseConstructorTest()
        {
            //Arrange
            SolveRequestResponse response;

            //Act
            response = new SolveRequestResponse(0);

            //Assert
            Assert.IsNotNull(response);
        }

        [TestMethod]
        public void DivideProblemConstructorTest()
        {
            //Arrange
            byte[] data = new byte[0];

            //Act
            DivideProblem divideProblem = new DivideProblem("name", 1, data, 1);

            //Assert
            Assert.IsNotNull(divideProblem);
        }
    }
}
