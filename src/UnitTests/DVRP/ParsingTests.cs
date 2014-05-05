using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.IO;
using DVRP;

namespace UnitTests
{
    /// <summary>
    /// Klasa testująca parsowanie problemów
    /// </summary>
    [TestClass]
    public class ParsingTests
    {
        [TestMethod]
        public void LoadTest1()
        {
            //Arrange
            FileStream file = System.IO.File.OpenRead("DVRP\\Problems\\problem1.vrp");

            //Act
            TestFileParser parser = new TestFileParser(file);

            //Assert
            Assert.IsNotNull(parser);
        }

        [TestMethod]
        public void LoadTest2()
        {
            //Arrange
            FileStream file = System.IO.File.OpenRead("DVRP\\Problems\\problem2.vrp");

            //Act
            TestFileParser parser = new TestFileParser(file);

            //Assert
            Assert.IsNotNull(parser);
        }

        [TestMethod]
        public void LoadTest3()
        {
            //Arrange
            FileStream file = System.IO.File.OpenRead("DVRP\\Problems\\problem3.vrp");

            //Act
            TestFileParser parser = new TestFileParser(file);

            //Assert
            Assert.IsNotNull(parser);
        }

        [TestMethod]
        public void LoadTest4()
        {
            //Arrange
            FileStream file = System.IO.File.OpenRead("DVRP\\Problems\\problem4.vrp");

            //Act
            TestFileParser parser = new TestFileParser(file);

            //Assert
            Assert.IsNotNull(parser);
        }

        [TestMethod]
        public void LoadTest5()
        {
            //Arrange
            FileStream file = System.IO.File.OpenRead("DVRP\\Problems\\problem5.vrp");

            //Act
            TestFileParser parser = new TestFileParser(file);

            //Assert
            Assert.IsNotNull(parser);
        }

        [TestMethod]
        public void LoadTest6()
        {
            //Arrange
            FileStream file = System.IO.File.OpenRead("DVRP\\Problems\\problem6.vrp");

            //Act
            TestFileParser parser = new TestFileParser(file);

            //Assert
            Assert.IsNotNull(parser);
        }

        [TestMethod]
        public void ParamsTest1()
        {
            //Arrange
            FileStream file = System.IO.File.OpenRead("DVRP\\Problems\\problem1.vrp");

            //Act
            TestFileParser parser = new TestFileParser(file);

            //Assert
            Assert.AreEqual(1, parser.NumDepots);
            Assert.AreEqual(1, parser.NumCapacities);
            Assert.AreEqual(12, parser.NumVisits);
            Assert.AreEqual(13, parser.NumLocations);
            Assert.AreEqual(12, parser.NumVehicles);
            Assert.AreEqual(100, parser.Capacities);
        }

        [TestMethod]
        public void ParamsTest2()
        {
            //Arrange
            FileStream file = System.IO.File.OpenRead("DVRP\\Problems\\problem2.vrp");

            //Act
            TestFileParser parser = new TestFileParser(file);

            //Assert
            Assert.AreEqual(1, parser.NumDepots);
            Assert.AreEqual(1, parser.NumCapacities);
            Assert.AreEqual(13, parser.NumVisits);
            Assert.AreEqual(14, parser.NumLocations);
            Assert.AreEqual(13, parser.NumVehicles);
            Assert.AreEqual(100, parser.Capacities);
        }

        [TestMethod]
        public void ParamsTest3()
        {
            //Arrange
            FileStream file = System.IO.File.OpenRead("DVRP\\Problems\\problem3.vrp");

            //Act
            TestFileParser parser = new TestFileParser(file);

            //Assert
            Assert.AreEqual(1, parser.NumDepots);
            Assert.AreEqual(1, parser.NumCapacities);
            Assert.AreEqual(14, parser.NumVisits);
            Assert.AreEqual(15, parser.NumLocations);
            Assert.AreEqual(14, parser.NumVehicles);
            Assert.AreEqual(100, parser.Capacities);
        }

        [TestMethod]
        public void ParamsTest4()
        {
            //Arrange
            FileStream file = System.IO.File.OpenRead("DVRP\\Problems\\problem4.vrp");

            //Act
            TestFileParser parser = new TestFileParser(file);

            //Assert
            Assert.AreEqual(1, parser.NumDepots);
            Assert.AreEqual(1, parser.NumCapacities);
            Assert.AreEqual(50, parser.NumVisits);
            Assert.AreEqual(51, parser.NumLocations);
            Assert.AreEqual(50, parser.NumVehicles);
            Assert.AreEqual(160, parser.Capacities);
        }

        [TestMethod]
        public void ParamsTest5()
        {
            //Arrange
            FileStream file = System.IO.File.OpenRead("DVRP\\Problems\\problem5.vrp");

            //Act
            TestFileParser parser = new TestFileParser(file);

            //Assert
            Assert.AreEqual(1, parser.NumDepots);
            Assert.AreEqual(1, parser.NumCapacities);
            Assert.AreEqual(71, parser.NumVisits);
            Assert.AreEqual(72, parser.NumLocations);
            Assert.AreEqual(50, parser.NumVehicles);
            Assert.AreEqual(30000, parser.Capacities);
        }

        [TestMethod]
        public void ParamsTest6()
        {
            //Arrange
            FileStream file = System.IO.File.OpenRead("DVRP\\Problems\\problem6.vrp");

            //Act
            TestFileParser parser = new TestFileParser(file);

            //Assert
            Assert.AreEqual(1, parser.NumDepots);
            Assert.AreEqual(1, parser.NumCapacities);
            Assert.AreEqual(75, parser.NumVisits);
            Assert.AreEqual(76, parser.NumLocations);
            Assert.AreEqual(50, parser.NumVehicles);
            Assert.AreEqual(1445, parser.Capacities);
        }
    }
}
