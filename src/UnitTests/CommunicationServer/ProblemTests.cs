using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Components;
using System.Text;
using System.Collections.Generic;
using System.Linq;
using CommunicationXML;

namespace UnitTests.CommunicationServer
{
    [TestClass]
    public class ProblemTests
    {
        [TestMethod]
        public void ProblemSetSolutionsTest()
        {
            //Arrange
            Problem p1 = new Problem("problem1", Encoding.ASCII.GetBytes("test test test"), 5000000);
            byte[] commonData1 = Encoding.ASCII.GetBytes("aaaaaaaaaaaaaaaaaaaaaaaaa");
            List<CommunicationXML.PartialProblem> partialProblems1 = new List<CommunicationXML.PartialProblem>();
            partialProblems1.Add(new CommunicationXML.PartialProblem(1, Encoding.ASCII.GetBytes("zzzzzzz")));
            partialProblems1.Add(new CommunicationXML.PartialProblem(2, Encoding.ASCII.GetBytes("yyyyyyy")));
            partialProblems1.Add(new CommunicationXML.PartialProblem(3, Encoding.ASCII.GetBytes("xxxxxxx")));
            p1.SetPartialProblems(commonData1, partialProblems1);

            Problem p2 = new Problem("problem2", Encoding.ASCII.GetBytes("test test test"), 5000000);
            byte[] commonData2 = Encoding.ASCII.GetBytes("aaaaaaaaaaaaaaaaaaaaaaaaa");
            List<CommunicationXML.PartialProblem> partialProblems2 = new List<CommunicationXML.PartialProblem>();
            partialProblems2.Add(new CommunicationXML.PartialProblem(1, Encoding.ASCII.GetBytes("zzzzzzz")));
            partialProblems2.Add(new CommunicationXML.PartialProblem(2, Encoding.ASCII.GetBytes("yyyyyyy")));
            p2.SetPartialProblems(commonData2, partialProblems2);

            var l1 = p1.GetPartialProblemListToSolve(2);
            var l2 = p2.GetPartialProblemListToSolve(3);

            var s1 = new List<Solution>();
            s1.Add(new Solution(1, false, SolutionType.Partial, 100000, Encoding.ASCII.GetBytes("1111")));
            s1.Add(new Solution(2, false, SolutionType.Ongoing, 200000, Encoding.ASCII.GetBytes("yyyyyyy")));

            var s2 = new List<Solution>();
            s2.Add(new Solution(false, SolutionType.Final, 1000000, Encoding.ASCII.GetBytes("final")));

            //Act
            p1.SetSolutions(s1);
            p2.SetSolutions(s2);

            //Assert
            Assert.AreEqual(ProblemStatus.Solved, p2.Status);
            Assert.IsNotNull(p2.Data);
            Assert.IsTrue(p2.Data.SequenceEqual(Encoding.ASCII.GetBytes("final")));
            Assert.IsFalse(p2.TimeoutOccured);
            Assert.AreEqual<ulong>(1000000, p2.ComputationsTime);
            Assert.AreEqual(ProblemStatus.Divided, p1.Status);
            Assert.IsNotNull(p1.Data);
            Assert.IsTrue(p1.Data.SequenceEqual(Encoding.ASCII.GetBytes("test test test")));
            Assert.IsFalse(p1.TimeoutOccured);
            Assert.IsNotNull(p1.PartialProblems);
            Assert.AreEqual(3, p1.PartialProblems.Count);
            Assert.AreEqual(1, p1.PartialProblems.Where(x => x.PartialProblemStatus == PartialProblemStatus.Solved).Count());
            Assert.AreEqual(1, p1.PartialProblems.Where(x => x.PartialProblemStatus == PartialProblemStatus.Sended).Count());
            Assert.IsTrue(Encoding.ASCII.GetBytes("1111").SequenceEqual(p1.PartialProblems.Where(x => x.PartialProblemStatus == PartialProblemStatus.Solved).First().Data));
        }

        [TestMethod]
        public void ProblemSetPartialProblemsTest()
        {
            //Arrange
            Problem p = new Problem("problem1", Encoding.ASCII.GetBytes("test test test"), 5000000);
            byte[] commonData = Encoding.ASCII.GetBytes("aaaaaaaaaaaaaaaaaaaaaaaaa");
            List<CommunicationXML.PartialProblem> partialProblems = new List<CommunicationXML.PartialProblem>();
            partialProblems.Add(new CommunicationXML.PartialProblem(1, Encoding.ASCII.GetBytes("zzzzzzz")));
            partialProblems.Add(new CommunicationXML.PartialProblem(2, Encoding.ASCII.GetBytes("yyyyyyy")));

            //Act
            p.SetPartialProblems(commonData, partialProblems);

            //Assert
            Assert.IsNotNull(p.CommonData);
            Assert.AreEqual(commonData, p.CommonData);
            Assert.IsNotNull(p.Data);
            Assert.IsNotNull(p.PartialProblems);
            Assert.IsNotNull(p.SolvingTimeout);
            Assert.AreEqual(2, p.PartialProblems.Count);
            Assert.AreEqual(Encoding.ASCII.GetBytes("yyyyyyy").Length, p.PartialProblems.Where(x => x.TaskId == 2).First().Data.Length);
            Assert.IsTrue(Encoding.ASCII.GetBytes("yyyyyyy").SequenceEqual(p.PartialProblems.Where(x => x.TaskId == 2).First().Data));
            Assert.AreEqual(ProblemStatus.Divided, p.Status);
        }

        [TestMethod]
        public void ProblemGetPartialProblemListToSolveTest()
        {
            //Arrange
            Problem p1 = new Problem("problem1", Encoding.ASCII.GetBytes("test test test"), 5000000);
            byte[] commonData1 = Encoding.ASCII.GetBytes("aaaaaaaaaaaaaaaaaaaaaaaaa");
            List<CommunicationXML.PartialProblem> partialProblems1 = new List<CommunicationXML.PartialProblem>();
            partialProblems1.Add(new CommunicationXML.PartialProblem(1, Encoding.ASCII.GetBytes("zzzzzzz")));
            partialProblems1.Add(new CommunicationXML.PartialProblem(2, Encoding.ASCII.GetBytes("yyyyyyy")));
            partialProblems1.Add(new CommunicationXML.PartialProblem(3, Encoding.ASCII.GetBytes("xxxxxxx")));
            p1.SetPartialProblems(commonData1, partialProblems1);

            Problem p2 = new Problem("problem2", Encoding.ASCII.GetBytes("test test test"), 5000000);
            byte[] commonData2 = Encoding.ASCII.GetBytes("aaaaaaaaaaaaaaaaaaaaaaaaa");
            List<CommunicationXML.PartialProblem> partialProblems2 = new List<CommunicationXML.PartialProblem>();
            partialProblems2.Add(new CommunicationXML.PartialProblem(1, Encoding.ASCII.GetBytes("zzzzzzz")));
            partialProblems2.Add(new CommunicationXML.PartialProblem(2, Encoding.ASCII.GetBytes("yyyyyyy")));
            p2.SetPartialProblems(commonData2, partialProblems2);

            //Act
            var l1 = p1.GetPartialProblemListToSolve(2);
            var l2 = p2.GetPartialProblemListToSolve(3);

            //Assert
            Assert.IsNotNull(l1);
            Assert.IsNotNull(l2);
            Assert.AreEqual(2, l1.Count);
            Assert.AreEqual(2, l2.Count);
            Assert.AreEqual(ProblemStatus.WaitingForPartialSolutions, p2.Status);
            Assert.AreEqual(ProblemStatus.Divided, p1.Status);
            Assert.AreEqual(3, p1.PartialProblems.Count);
            Assert.AreEqual(2, p2.PartialProblems.Count);
            Assert.AreEqual(2, p1.PartialProblems.Where(x => x.PartialProblemStatus == PartialProblemStatus.Sended).Count());
            Assert.AreEqual(1, p1.PartialProblems.Where(x => x.PartialProblemStatus == PartialProblemStatus.New).Count());
            Assert.AreEqual(0, p2.PartialProblems.Where(x => x.PartialProblemStatus == PartialProblemStatus.New).Count());
            Assert.IsTrue(p1.PartialProblems.Where(x => x.TaskId == l1[0].TaskId).First().Data.SequenceEqual(l1[0].Data));
        }
    }
}
