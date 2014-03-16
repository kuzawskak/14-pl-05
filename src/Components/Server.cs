using CommunicationXML;
using CommunicationNetwork;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Components
{
    public class Server
    {
        private int port;
        private TimeSpan timeout;
        private Listener listener;
        private List<ComputationalNode> computationalNodes;
        private List<TaskManager> taskManagers;
        private List<Problem> problems;
        private object lockObj;

        public Server(int port, TimeSpan timeout)
        {
            this.port = port;
            this.timeout = timeout;
            listener = new Listener(port, ConnectionHandler);
            computationalNodes = new List<ComputationalNode>();
            taskManagers = new List<TaskManager>();
            lockObj = new object();
        }

        /// <summary>
        /// Włączenie serwera.
        /// </summary>
        public void Start()
        {
            Task.Factory.StartNew(BackgroundWork);

            listener.Start();
        }

        /// <summary>
        /// Klasa nasłuchująca wywołuje handler w wątku.
        /// </summary>
        /// <param name="data">Odebrane dane w postaci binarnej.</param>
        /// <param name="ctx">Kontekst połączenia. Można wysłać przez niego odpowiedź do klienta który wysłal dane</param>
        private void ConnectionHandler(byte[] data, ConnectionContext ctx)
        {
            XMLParser parser = new XMLParser(data);

            lock (lockObj)
            {
                switch (parser.MessageType)
                {
                    case MessageTypes.Register:
                        ctx.Send(RegisterNewNode(parser.Message).XMLData);
                        break;

                    case MessageTypes.Status:
                        MessageObject taskData = UpdateAndGiveData(parser.Message);
                        if (taskData != null)
                            ctx.Send(taskData.XMLData);
                        break;

                    case MessageTypes.SolveRequest:
                        ctx.Send(RegisterNewProblem(parser.Message).XMLData);
                        break;

                    case MessageTypes.SolutionRequest:
                        ctx.Send(SendSolution(parser.Message).XMLData);
                        break;

                    case MessageTypes.SolvePartialProblems:
                        GetDividedProblem(parser.Message);
                        break;

                    case MessageTypes.Solutions:
                        GetSolutions(parser.Message);
                        break;
                }
            }
        }

        /// <summary>
        /// Wysyłanie rozwiązania do CC.
        /// </summary>
        /// <param name="obj"></param>
        private Solutions SendSolution(MessageObject obj)
        {
            Solutions response = null;
            /* FOR: Albert
            SolutionRequest request = obj as SolutionRequest;

            Problem p = problems.Find(x => x.Id == request.Id);

            // TODO: Co jesli nie znaleziono takiego problemu?
            if (p != null)
            {
                List<Solution> solutions = new List<Solution>();

                if(p.Status == ProblemStatus.Solved)
                {
                    solutions.Add(new Solution(p.TimeoutOccured, SolutionTypes.Final, p.ComputationsTime, p.Data));
                }
                else
                {
                    foreach (var s in p.PartialProblems)
                    {
                        solutions.Add(
                            new Solution(s.TaskId, s.TimeoutOccured, 
                                s.PartialProblemStatus == PartialProblemStatuses.Solved ? 
                                SolutionTypes.Partial : SolutionTypes.Ongoing, s.ComputationsTime, s.Data));
                    }
                }

                response = new Solutions(p.Id, p.ProblemType, p.CommonData, solutions);
            }*/

            return response;
        }

        /// <summary>
        /// Pobiera częściowe i całkowite rozwiązania.
        /// </summary>
        /// <param name="obj"></param>
        private void GetSolutions(MessageObject obj)
        {
            Solutions solutions = obj as Solutions;

            // TODO: Aktualizować osttni czas i wątki CN/TM? (NIE?)
            /* FOR: Albert
            Problem p = problems.Find(x => x.Id == solutions.Id && x.ProblemType == solutions.ProblemType);

            if (p != null)
            {
                p.SetSolutions(solutions.Solutions);
                // TODO: Aktualizacaja wątków odpowiedniego CN/TM? Zaktualizują sie przy następnym Status?
            }*/
        }

        /// <summary>
        /// Odbiera podzielone dane od TM.
        /// </summary>
        /// <param name="obj"></param>
        private void GetDividedProblem(MessageObject obj)
        {
            // TODO: Czy zmienić informację o wolnych wątkach TM?
            SolvePartialProblems partialProblems = obj as SolvePartialProblems;

            /* FOR: Albert :: te wszystkie pola w klasie :P
            Problem p = problems.Find(x => x.Id == partialProblems.Id && x.ProblemType == partialProblems.ProblemType);

            if (p != null && p.Status == ProblemStatus.WaitingForDivision)
            {
                p.SetPartialProblems(partialProblems.CommonData, partialProblems.PartialProblems);
            }*/
        }

        /// <summary>
        /// Aktualizuje informację o TM i CN. Sprawdza czy jest dla nich nowe zadanie i jeśli znajdzie, odsyła.
        /// </summary>
        /// <param name="obj"></param>
        /// <returns></returns>
        private MessageObject UpdateAndGiveData(MessageObject obj)
        {
            MessageObject response = null;
            Status status = obj as Status;

            Node node = taskManagers.Find(item => item.Id == status.Id);
            if (node == null)
                node = computationalNodes.Find(item => item.Id == status.Id);

            if (node != null)
            {
                node.Update(status.Threads);

                if (node.GetAvailableThreads() > 0)
                {
                    if (node.GetType() == typeof(TaskManager))
                    {
                        Problem partialP = problems.Find(t => t.Status == ProblemStatus.PartiallySolved);

                        if (partialP != null)
                        {
                            partialP.Status = ProblemStatus.WaitingForSolutions;
                            /* FOR: Albert
                            List<Solution> solutions = new List<Solution>();

                            foreach (var pp in partialP.PartialProblems)
                            {
                                solutions.Add(new Solution(pp.TaskId, pp.TimeoutOccures, SolutionTypes.Partial, pp.ComputationsTime, pp.Data));
                            }

                            response = new Solutions(partialP.Id, partialP.ProblemType, partialP.CommonData, solutions);
                            return response;*/
                        }

                        Problem newP = problems.Find(t => t.Status == ProblemStatus.New);

                        if (newP != null)
                        {
                            newP.Status = ProblemStatus.WaitingForDivision;
                            ulong computationalNodesCount = (ulong)computationalNodes.Sum(t => t.GetAvailableThreads());
                            response = new DivideProblem(newP.ProblemType, newP.Id, newP.Data, computationalNodesCount);
                            // TODO: Update statusu TM? Nie wiemy który wątek...
                            return response;
                        }

                        // TODO: Wysyłanie Solutions do połączenia.
                    }
                    else    // ComputationalNode
                    {
                        Problem dividedP = problems.Find(t => t.Status == ProblemStatus.Divided);

                        if (dividedP != null)
                        {
                            /* FOR: Albert: Konstruktor (po odkomentowaniu podkreśli GetPartialProblem... => trzeba odkomentować w Problem.cs
                            response =
                                new SolvePartialProblems(dividedP.Id, dividedP.ProblemType,
                                    dividedP.CommonData, dividedP.SolvingTimeout,
                                    dividedP.GetPartialProblemListToSolve(node.GetAvailableThreads()));*/

                            // TODO: Update statusu wątków CN? Ale nie wiemy który wątek zacznie wykonywać obliczenia...
                            return response;
                        }
                    }
                }
            }

            return response;
        }

        /// <summary>
        /// Dodawanie nowego problemu do listy.
        /// </summary>
        /// <param name="obj"></param>
        /// <returns></returns>
        private SolveRequestResponse RegisterNewProblem(MessageObject obj)
        {
            SolveRequest req = obj as SolveRequest;
            SolveRequestResponse response = null;

            /* FOR: Albert :: pola ProblemType, Data, SolvingTimeout w SolveRequest
            Problem p =  new Problem(req.ProblemType, req.Data, req.SolvingTimeout);
            problems.Add(p);
            response = new SolveRequestResponse(p.Id);*/

            return response;
        }

        /// <summary>
        /// Usuwanie nieaktywnych CN i TM
        /// </summary>
        private void BackgroundWork()
        {
            while (true)
            {
                System.Threading.Thread.Sleep(timeout.Milliseconds);

                lock (lockObj)
                {
                    // TODO: jeśli dany CN i TM coś obliczał to zmienić (cofnąć) status zadania.
                    // TODO: w przypadku CN trzeba wycofać odpowiedni podzbiór zadań!!!
                    // Jeśli TM nie odpowiada (dzielenie zadania) to zmienic status z WaitingForDivision na New
                    // Jeśli CN nie odpowiada to zmienić status partial problems z Sended na New i problemu z WaitingForPartialSolution na Divided (jeśli trzeba)
                    // Jeśli TM nie odpowiada (łączenie rozwiązania) to zmiana statusu z WaitingForSolution na PartiallySolved
                    computationalNodes.RemoveAll(x => x.LastTime < DateTime.Now - timeout);
                    taskManagers.RemoveAll(x => x.LastTime < DateTime.Now - timeout);
                }
            }
        }

        /// <summary>
        /// Rejestracja nowego Computational Node lub Task Manager
        /// </summary>
        /// <param name="obj"></param>
        /// <returns></returns>
        private RegisterResponse RegisterNewNode(MessageObject obj)
        {
            Register reg = obj as Register;
            RegisterResponse response;

            switch (reg.Type)
            {
                case NodeType.TaskManager:
                    TaskManager tm = new TaskManager(reg.SolvableProblems, reg.ParallelThreads);
                    taskManagers.Add(tm);
                    response = new RegisterResponse(tm.Id, timeout);
                    break;

                case NodeType.ComputationalNode:
                    ComputationalNode cn = new ComputationalNode(reg.SolvableProblems, reg.ParallelThreads);
                    computationalNodes.Add(cn);
                    response = new RegisterResponse(cn.Id, timeout);
                    break;

                default:
                    response = null;
                    break;
            }

            return response;
        }
    }
}
