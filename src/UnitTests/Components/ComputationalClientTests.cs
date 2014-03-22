using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Components;

namespace UnitTests.Components
{
    [TestClass]
    public class ComputationalClientTests
    {
        [TestMethod]
        public void CCConstructorAndRegister()
        {
            //Arrange
            string ip_address = "127.0.0.1";
            int port = 22;

            //string problem_data_filepath = "CCTest.xml";
            string problem_type = "dvrp";
            //Act
            ComputationalClient cc = new ComputationalClient(ip_address, port, null,problem_type);
            //Assert
            Assert.IsNotNull(cc);
          
            //Assert.IsTrue(cc.registerProblem(problem_data_filepath));


            

           
        }
    }
}
