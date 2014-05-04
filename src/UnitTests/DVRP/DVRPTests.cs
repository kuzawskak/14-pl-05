using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using DVRP;
using System.IO;

namespace UnitTests
{
    /// <summary>
    /// Klasa testująca podstawowe działania TaskSolvera DVRP
    /// </summary>
    [TestClass]
    public class DVRPTests
    {
        private const int ThreadNumber = 10;

        [TestMethod]
        public void ConstructorTest1()
        {
            //Arrange
            MemoryStream ms = new MemoryStream();
            System.IO.File.OpenRead("DVRP\\Problems\\problem1.vrp").CopyTo(ms);
            byte[] data = ms.ToArray();

            //Act
            DVRP.DVRP dvrp = new DVRP.DVRP(data);

            //Assert
            Assert.IsNotNull(dvrp);
        }

        [TestMethod]
        public void ConstructorTest2()
        {
            //Arrange
            MemoryStream ms = new MemoryStream();
            System.IO.File.OpenRead("DVRP\\Problems\\problem2.vrp").CopyTo(ms);
            byte[] data = ms.ToArray();

            //Act
            DVRP.DVRP dvrp = new DVRP.DVRP(data);

            //Assert
            Assert.IsNotNull(dvrp);
        }

        [TestMethod]
        public void ConstructorTest3()
        {
            //Arrange
            MemoryStream ms = new MemoryStream();
            System.IO.File.OpenRead("DVRP\\Problems\\problem3.vrp").CopyTo(ms);
            byte[] data = ms.ToArray();

            //Act
            DVRP.DVRP dvrp = new DVRP.DVRP(data);

            //Assert
            Assert.IsNotNull(dvrp);
        }

        [TestMethod]
        public void ConstructorTest4()
        {
            //Arrange
            MemoryStream ms = new MemoryStream();
            System.IO.File.OpenRead("DVRP\\Problems\\problem4.vrp").CopyTo(ms);
            byte[] data = ms.ToArray();

            //Act
            DVRP.DVRP dvrp = new DVRP.DVRP(data);

            //Assert
            Assert.IsNotNull(dvrp);
        }

        [TestMethod]
        public void ConstructorTest5()
        {
            //Arrange
            MemoryStream ms = new MemoryStream();
            System.IO.File.OpenRead("DVRP\\Problems\\problem5.vrp").CopyTo(ms);
            byte[] data = ms.ToArray();

            //Act
            DVRP.DVRP dvrp = new DVRP.DVRP(data);

            //Assert
            Assert.IsNotNull(dvrp);
        }

        [TestMethod]
        public void ConstructorTest6()
        {
            //Arrange
            MemoryStream ms = new MemoryStream();
            System.IO.File.OpenRead("DVRP\\Problems\\problem6.vrp").CopyTo(ms);
            byte[] data = ms.ToArray();

            //Act
            DVRP.DVRP dvrp = new DVRP.DVRP(data);

            //Assert
            Assert.IsNotNull(dvrp);
        }

        [TestMethod]
        public void DivideTest1()
        {
            //Arrange
            MemoryStream ms = new MemoryStream();
            System.IO.File.OpenRead("DVRP\\Problems\\problem1.vrp").CopyTo(ms);
            byte[] data = ms.ToArray();
            DVRP.DVRP dvrp = new DVRP.DVRP(data);

            //Act
            var divided = dvrp.DivideProblem(ThreadNumber);

            //Assert
            Assert.IsNotNull(divided);
        }

        [TestMethod]
        public void DivideTest2()
        {
            //Arrange
            MemoryStream ms = new MemoryStream();
            System.IO.File.OpenRead("DVRP\\Problems\\problem2.vrp").CopyTo(ms);
            byte[] data = ms.ToArray();
            DVRP.DVRP dvrp = new DVRP.DVRP(data);

            //Act
            var divided = dvrp.DivideProblem(ThreadNumber);

            //Assert
            Assert.IsNotNull(divided);
        }

        [TestMethod]
        public void DivideTest3()
        {
            //Arrange
            MemoryStream ms = new MemoryStream();
            System.IO.File.OpenRead("DVRP\\Problems\\problem3.vrp").CopyTo(ms);
            byte[] data = ms.ToArray();
            DVRP.DVRP dvrp = new DVRP.DVRP(data);

            //Act
            var divided = dvrp.DivideProblem(ThreadNumber);

            //Assert
            Assert.IsNotNull(divided);
        }

        [TestMethod]
        public void DivideTest4()
        {
            //Arrange
            MemoryStream ms = new MemoryStream();
            System.IO.File.OpenRead("DVRP\\Problems\\problem4.vrp").CopyTo(ms);
            byte[] data = ms.ToArray();
            DVRP.DVRP dvrp = new DVRP.DVRP(data);

            //Act
            var divided = dvrp.DivideProblem(ThreadNumber);

            //Assert
            Assert.IsNotNull(divided);
        }

        [TestMethod]
        public void DivideResultsTest1()
        {
            //Arrange
            MemoryStream ms = new MemoryStream();
            System.IO.File.OpenRead("DVRP\\Problems\\problem1.vrp").CopyTo(ms);
            byte[] data = ms.ToArray();
            DVRP.DVRP dvrp = new DVRP.DVRP(data);

            //Act
            var divided = dvrp.DivideProblem(ThreadNumber);

            //Assert
            Assert.AreEqual(ThreadNumber, divided.GetLength(0));
        }

        [TestMethod]
        public void DivideResultsTest2()
        {
            //Arrange
            MemoryStream ms = new MemoryStream();
            System.IO.File.OpenRead("DVRP\\Problems\\problem2.vrp").CopyTo(ms);
            byte[] data = ms.ToArray();
            DVRP.DVRP dvrp = new DVRP.DVRP(data);

            //Act
            var divided = dvrp.DivideProblem(ThreadNumber);

            //Assert
            Assert.AreEqual(ThreadNumber, divided.GetLength(0));
        }

        [TestMethod]
        public void DivideResultsTest3()
        {
            //Arrange
            MemoryStream ms = new MemoryStream();
            System.IO.File.OpenRead("DVRP\\Problems\\problem3.vrp").CopyTo(ms);
            byte[] data = ms.ToArray();
            DVRP.DVRP dvrp = new DVRP.DVRP(data);

            //Act
            var divided = dvrp.DivideProblem(ThreadNumber);

            //Assert
            Assert.AreEqual(ThreadNumber, divided.GetLength(0));
        }

        [TestMethod]
        public void DivideResultsTest4()
        {
            //Arrange
            MemoryStream ms = new MemoryStream();
            System.IO.File.OpenRead("DVRP\\Problems\\problem4.vrp").CopyTo(ms);
            byte[] data = ms.ToArray();
            DVRP.DVRP dvrp = new DVRP.DVRP(data);

            //Act
            var divided = dvrp.DivideProblem(ThreadNumber);

            //Assert
            Assert.AreEqual(ThreadNumber, divided.GetLength(0));
        }
    }
}
