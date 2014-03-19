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
        /// Zatrzymanie serwera
        /// </summary>
        public void Stop()
        {
            // TODO: Zamienic to na ustawienie flagi?
            Environment.Exit(0);
        }

        /// <summary>
        /// Handler wywoływany przez klasę nasłuchującą.
        /// </summary>
        /// <param name="data">Odebrane dane w postaci binarnej.</param>
        /// <param name="ctx">Kontekst połączenia. Można wysłać przez niego odpowiedź do klienta który wysłal dane</param>
        private void ConnectionHandler(byte[] data, ConnectionContext ctx)
        {
            XMLParser parser = new XMLParser(data);

            MessageObject response = null;

            switch (parser.MessageType)
            {
                case MessageTypes.Register:
                    response = RegisterNewNode(parser.Message);
                    break;

                case MessageTypes.SolveRequest:
                    response = RegisterNewProblem(parser.Message);
                    break;

                case MessageTypes.Status:
                    response = UpdateAndGiveData(parser.Message);
                    break;

                case MessageTypes.SolutionRequest:
                    response = SendSolution(parser.Message);
                    break;

                case MessageTypes.SolvePartialProblems:
                    GetDividedProblem(parser.Message);
                    break;

                case MessageTypes.Solutions:
                    GetSolutions(parser.Message);
                    break;
            }

            ctx.Send(response == null ? null : response.GetXmlData());
        }

        /// <summary>
        /// Wysyłanie rozwiązania do CC.
        /// </summary>
        /// <param name="obj"></param>
        private Solutions SendSolution(MessageObject obj)
        {
            Solutions response = null;
            SolutionRequest request = obj as SolutionRequest;

            lock (lockObj)
            {
                Problem p = problems.Find(x => x.Id == request.Id);

                if (p != null)
                {
                    List<Solution> solutions = new List<Solution>();

                    if (p.Status == ProblemStatus.Solved)
                    {
                        solutions.Add(new Solution(p.TimeoutOccured, SolutionType.Final, p.ComputationsTime, p.Data));
                    }
                    else
                    {
                        foreach (var s in p.PartialProblems)
                        {
                            solutions.Add(
                                new Solution(s.TaskId, s.TimeoutOccured,
                                    s.PartialProblemStatus == PartialProblemStatuses.Solved ?
                                    SolutionType.Partial : SolutionType.Ongoing, s.ComputationsTime, s.Data));
                        }
                    }

                    response = new Solutions(p.ProblemType, p.Id, p.CommonData, solutions);
                }
            }

            return response;
        }

        /// <summary>
        /// Pobiera częściowe i całkowite rozwiązania.
        /// </summary>
        /// <param name="obj"></param>
        private void GetSolutions(MessageObject obj)
        {
            Solutions solutions = obj as Solutions;

            // TODO: Aktualizować wątki CN/TM? (NIE?)
            // TODO: Aktualizować ostatni czas połączenia?
            lock (lockObj)
            {
                Problem p = problems.Find(x => x.Id == solutions.Id && x.ProblemType == solutions.ProblemType);

                if (p != null)
                {
                    p.SetSolutions(solutions.SolutionsList);
                }
            }
        }

        /// <summary>
        /// Odbiera podzielone dane od TM.
        /// </summary>
        /// <param name="obj"></param>
        private void GetDividedProblem(MessageObject obj)
        {
            // TODO: Czy zmienić informację o wolnych wątkach TM?
            // TODO: Czy robić update czasu ostatniej aktywności TM?
            SolvePartialProblems partialProblems = obj as SolvePartialProblems;

            lock (lockObj)
            {
                Problem p = problems.Find(x => x.Id == partialProblems.Id && x.ProblemType == partialProblems.ProblemType);

                if (p != null && p.Status == ProblemStatus.WaitingForDivision)
                {
                    p.SetPartialProblems(partialProblems.CommonData, partialProblems.PartialProblems);
                }
            }
        }

        /// <summary>
        /// Wycofywanie zgubionych danych. (Jeśli istaniją.)
        /// </summary>
        /// <param name="tempProblems">Problemy wysłane do TM/CN zapisane tymczasowow, które nie zostały zaktualizowne.</param>
        private void UpdateTemporaryProblems(List<Tuple<ulong, List<ulong>>> tempProblems)
        {
            foreach (var t in tempProblems)
            {
                Problem p = problems.Find(x => x.Id == t.Item1);

                if (p != null)
                {
                    switch (p.Status)
                    {
                        case ProblemStatus.WaitingForDivision:
                            p.Status = ProblemStatus.New;
                            break;

                        case ProblemStatus.Divided:
                            goto case ProblemStatus.WaitingForPartialSolutions;

                        case ProblemStatus.WaitingForPartialSolutions:
                            if (t.Item2 != null)
                            {
                                foreach (var ps in t.Item2)
                                {
                                    PartialProblem pp = p.PartialProblems.Find(x => x.TaskId == ps);

                                    if (pp != null && pp.PartialProblemStatus == PartialProblemStatuses.Sended)
                                        pp.PartialProblemStatus = PartialProblemStatuses.New;
                                }

                                p.Status = p.PartialProblems.Where(
                                    x => x.PartialProblemStatus == PartialProblemStatuses.New).Count() == 0 ?
                                    ProblemStatus.WaitingForPartialSolutions : ProblemStatus.Divided;
                            }
                            break;

                        case ProblemStatus.WaitingForSolutions:
                            p.Status = ProblemStatus.PartiallySolved;
                            break;
                    }                    
                }
            }

            // Czyszczenie listy.
            tempProblems.RemoveAll(x => true);
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

            lock (lockObj)
            {
                Node node = taskManagers.Find(item => item.Id == status.Id);
                if (node == null)
                    node = computationalNodes.Find(item => item.Id == status.Id);

                if (node != null)
                {
                    node.Update(status.Threads);
                    UpdateTemporaryProblems(node.TemporaryProblems);

                    if (node.GetAvailableThreads() > 0)
                    {
                        if (node.GetType() == typeof(TaskManager))
                        {
                            Problem partialP = problems.Find(t => t.Status == ProblemStatus.PartiallySolved);

                            if (partialP != null)
                            {
                                partialP.Status = ProblemStatus.WaitingForSolutions;

                                List<Solution> solutions = new List<Solution>();
                                node.TemporaryProblems.Add(new Tuple<ulong, List<ulong>>(partialP.Id, null));

                                foreach (var pp in partialP.PartialProblems)
                                {
                                    solutions.Add(new Solution(pp.TaskId, pp.TimeoutOccured, SolutionType.Partial, pp.ComputationsTime, pp.Data));
                                }

                                response = new Solutions(partialP.ProblemType, partialP.Id, partialP.CommonData, solutions);
                                return response;
                            }

                            Problem newP = problems.Find(t => t.Status == ProblemStatus.New);

                            if (newP != null)
                            {
                                newP.Status = ProblemStatus.WaitingForDivision;
                                node.TemporaryProblems.Add(new Tuple<ulong, List<ulong>>(newP.Id, null));
                                ulong computationalNodesCount = (ulong)computationalNodes.Sum(t => t.GetAvailableThreads());
                                response = new DivideProblem(newP.ProblemType, newP.Id, newP.Data, computationalNodesCount);
                                return response;
                            }
                        }
                        else    // ComputationalNode
                        {
                            Problem dividedP = problems.Find(t => t.Status == ProblemStatus.Divided);

                            if (dividedP != null)
                            {
                                List<CommunicationXML.PartialProblem> pp = 
                                    dividedP.GetPartialProblemListToSolve(node.GetAvailableThreads());

                                response =
                                    new SolvePartialProblems(dividedP.ProblemType, dividedP.Id,
                                        dividedP.CommonData, dividedP.SolvingTimeout, pp);

                                List<ulong> ppnums = new List<ulong>();

                                foreach(var x in pp)
                                {
                                    ppnums.Add(x.TaskId);
                                }

                                node.TemporaryProblems.Add(new Tuple<ulong, List<ulong>>(dividedP.Id, ppnums));

                                return response;
                            }
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

            lock (lockObj)
            {
                Problem p = new Problem(req.ProblemType, req.Data, req.SolvingTimeout);
                problems.Add(p);
                response = new SolveRequestResponse(p.Id);
            }

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
                    // TODO: Jeżeli wyślemy XML z zadaniem np do CN i on go zignoruje albo np się rozłączy to nigdy się o tym nie dowiemy! NAPRAWIĆ!
                    // TODO: Usunięcie Solution po wysłaniu do CC
                    // TODO: jeśli dany CN i TM coś obliczał to zmienić (cofnąć) status zadania.
                    // TODO: w przypadku CN trzeba wycofać odpowiedni podzbiór zadań!!!
                    // Jeśli TM nie odpowiada (dzielenie zadania) to zmienic status z WaitingForDivision! na New
                    // Jeśli CN nie odpowiada to zmienić status partial problems z Sended! na New i problemu z WaitingForPartialSolution! na Divided (jeśli trzeba)
                    // Jeśli TM nie odpowiada (łączenie rozwiązania) to zmiana statusu z WaitingForSolution! na PartiallySolved
                    computationalNodes.RemoveAll(x => x.LastTime.Ticks < DateTime.Now.Ticks - timeout.Ticks);
                    taskManagers.RemoveAll(x => x.LastTime.Ticks < DateTime.Now.Ticks - timeout.Ticks);
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

            lock (lockObj)
            {
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
            }

            return response;
        }
    }
}
