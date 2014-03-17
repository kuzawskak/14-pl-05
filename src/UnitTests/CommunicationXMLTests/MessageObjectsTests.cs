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
    }
}
