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
            registerResponse = new RegisterResponse((ulong)0, new TimeSpan());

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
            status = new Status();

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

        [TestMethod]
        public void SolutionRequestConstructorTest()
        {
            //Arrange
            SolutionRequest solutionRequest;

            //Act
            solutionRequest = new SolutionRequest(0);

            //Assert
            Assert.IsNotNull(solutionRequest);
        }

        [TestMethod]
        public void PartialProblemConstructorTest()
        {
            //Arrange
            byte[] data = new byte[0];

            //Act
            PartialProblem partialProblem = new PartialProblem(0, data);

            //Assert
            Assert.IsNotNull(partialProblem);
        }

        [TestMethod]
        public void SolvePartialProblemsConstructorTest()
        {
            //Arrange
            byte[] data = new byte[0];
            PartialProblem[] partialProblems = new PartialProblem[]{ new PartialProblem(0, (byte[])data.Clone()),
                new PartialProblem(1, (byte[])data.Clone()) };

            //Act
            SolvePartialProblems solvePartialProblems = new SolvePartialProblems("name",
                0, data, null, partialProblems);

            //Assert
            Assert.IsNotNull(solvePartialProblems);
            
        }

        [TestMethod]
        public void SolutionConstructorTest()
        {
            //Arrange
            byte[] data = new byte[0];

            //Act
            Solution solution = new Solution(0, true, SolutionType.Final, 120, data);

            //Assert
            Assert.IsNotNull(solution);
        }

        [TestMethod]
        public void SolutionsConstructorTest()
        {
            //Arrange
            byte[] commonData = new byte[0];
            Solution[] solutionsArray = new Solution[]{new Solution(0, true, SolutionType.Ongoing, 120, new byte[0])};

            //Act
            Solutions solutions = new Solutions("name", 0, commonData, solutionsArray);

            //Assert
            Assert.IsNotNull(solutions);
        }
    }
}
