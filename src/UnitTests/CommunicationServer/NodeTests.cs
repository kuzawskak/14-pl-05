using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Components;
using System.Collections.Generic;
using CommunicationXML;
using System.Linq;

namespace UnitTests.CommunicationServer
{
    [TestClass]
    public class NodeTests
    {
        [TestMethod]
        public void NodeUpdateTest()
        {
            //Arrange
            Node node = new Node(new List<string>() { "problem1", "problem2" }, 2);
            node.TemporaryProblems.Add(
                new TempProblem(5, ProblemStatus.WaitingForPartialSolutions, 
                    new List<TempPartial>() { 
                        new TempPartial(1, PartialProblemStatus.Sended) }));
            node.TemporaryProblems.Add(
                new TempProblem(5, ProblemStatus.WaitingForPartialSolutions,
                    new List<TempPartial>() { 
                        new TempPartial(2, PartialProblemStatus.Sended) }));

            DateTime lastTime = node.LastTime;

            List<ComputationalThread> threads = new List<ComputationalThread>();
            threads.Add(new ComputationalThread(ComputationalThreadState.Idle, 1000, null, null, null));
            threads.Add(new ComputationalThread(ComputationalThreadState.Busy, 200000, 5, 1, "problem1"));

            //Act
            node.Update(threads);

            //Assert
            Assert.AreNotEqual<DateTime>(lastTime, node.LastTime);
            Assert.IsNotNull(node.Threads);
            Assert.IsNotNull(node.TemporaryProblems);
            Assert.AreEqual(node.TemporaryProblems.Count, 1);
            Assert.AreEqual(node.Threads.Count, 2);
            Assert.AreEqual<ulong>(node.TemporaryProblems[0].ProblemId, 5);
            Assert.AreEqual(node.TemporaryProblems[0].Status, ProblemStatus.WaitingForPartialSolutions);
            Assert.IsNotNull(node.TemporaryProblems[0].PartialProblems);
            Assert.AreEqual(node.TemporaryProblems[0].PartialProblems.Count, 1);
            Assert.AreEqual<ulong>(node.TemporaryProblems[0].PartialProblems[0].PartialId, 2);
            Assert.AreEqual(node.TemporaryProblems[0].PartialProblems[0].PartialStatus, PartialProblemStatus.Sended);
            Assert.AreEqual(node.Threads.Where(x => x.State == ComputationalThreadState.Busy).Count(), 1);
            Assert.AreEqual<ulong?>(node.Threads.Where(x => x.State == ComputationalThreadState.Busy).First().TaskId, 1);
            Assert.AreEqual(node.Threads.Where(x => x.State == ComputationalThreadState.Busy).First().ProblemType, "problem1");
            Assert.AreEqual<ulong?>(node.Threads.Where(x => x.State == ComputationalThreadState.Busy).First().ProblemInstanceId, 5);
            Assert.AreEqual<ulong>(node.Threads.Where(x => x.State == ComputationalThreadState.Busy).First().HowLong, 200000);
        }

        [TestMethod]
        public void NodeGetAvailableThreadsTest()
        {
            //Arrange
            List<ComputationalThread> threads;
            Node iNode = new Node(new List<string>() { "problem1", "problem2" }, 3);
            Node bNode = new Node(new List<string>() { "problem1", "problem2" }, 2);
            Node pNode = new Node(new List<string>() { "problem1", "problem2" }, 2);

            threads = new List<ComputationalThread>();
            threads.Add(new ComputationalThread(ComputationalThreadState.Busy, 1000, 5, 2, "problem1"));
            threads.Add(new ComputationalThread(ComputationalThreadState.Busy, 200000, 5, 1, "problem1"));
            bNode.Update(threads);

            threads = new List<ComputationalThread>();
            threads.Add(new ComputationalThread(ComputationalThreadState.Busy, 1000, 5, 2, "problem1"));
            threads.Add(new ComputationalThread(ComputationalThreadState.Idle, 100, null, null, null));
            pNode.Update(threads);

            //Act
            long iat = iNode.GetAvailableThreads();
            long bat = bNode.GetAvailableThreads();
            long pat = pNode.GetAvailableThreads();

            //Assert
            Assert.AreEqual(iNode.ParallelThreads, iNode.Threads.Count);
            Assert.AreEqual(iNode.Threads[0].State, ComputationalThreadState.Idle);
            Assert.IsNull(iNode.Threads[0].ProblemType);
            Assert.AreEqual(iat, 3);
            Assert.AreEqual(bat, 0);
            Assert.AreEqual(pat, 1);
        }
    }
}
