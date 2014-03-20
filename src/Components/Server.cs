using CommunicationXML;
using CommunicationNetwork;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Threading;

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

        private volatile bool working;
        private CancellationTokenSource backgroundToken;

        private MessagePrinter debug;

        public Server(int port, TimeSpan timeout)
        {
            this.port = port;
            this.timeout = timeout;
            listener = new Listener(port, ConnectionHandler);
            computationalNodes = new List<ComputationalNode>();
            taskManagers = new List<TaskManager>();
            problems = new List<Problem>();
            lockObj = new object();
            backgroundToken = new CancellationTokenSource();

            debug = new MessagePrinter(NodeName.CS);
        }

        /// <summary>
        /// Włączenie serwera.
        /// </summary>
        public void Start()
        {
            working = true;
            Task.Factory.StartNew(BackgroundWork, backgroundToken.Token);
            listener.Start();
        }

        /// <summary>
        /// Zatrzymanie serwera
        /// </summary>
        public void Stop()
        {
            // TODO: Zamienic to na ustawienie flagi
            working = false;
            backgroundToken.Cancel();
            Thread.Sleep(1000);
            Environment.Exit(0);
        }

        /// <summary>
        /// Handler wywoływany przez klasę nasłuchującą.
        /// </summary>
        /// <param name="data">Odebrane dane w postaci binarnej.</param>
        /// <param name="ctx">Kontekst połączenia. Można wysłać przez niego odpowiedź do klienta który wysłal dane</param>
        private void ConnectionHandler(byte[] data, ConnectionContext ctx)
        {
            XMLParser parser;

            try
            {
                parser = new XMLParser(data);
            }
            catch (Exception e)
            {
                debug.Print("Received invalid message!");
                ctx.Send(null);
                return;
            }

            MessageObject response = null;

            switch (parser.MessageType)
            {
                case MessageTypes.Register:
                    debug.Print("Received 'Register' message.");
                    response = RegisterNewNode(parser.Message);
                    break;

                case MessageTypes.SolveRequest:
                    debug.Print("Received 'SolveRequest' message.");
                    response = RegisterNewProblem(parser.Message);
                    break;

                case MessageTypes.Status:
                    debug.Print("Received 'Status' message.");
                    response = UpdateAndGiveData(parser.Message);
                    break;

                case MessageTypes.SolutionRequest:
                    debug.Print("Received 'SolutionRequest' message.");
                    response = SendSolution(parser.Message);
                    break;

                case MessageTypes.SolvePartialProblems:
                    debug.Print("Received 'SolvePartialProblems' message.");
                    GetDividedProblem(parser.Message);
                    break;

                case MessageTypes.Solutions:
                    debug.Print("Received 'Solutions' message.");
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
                                    s.PartialProblemStatus == PartialProblemStatus.Solved ?
                                    SolutionType.Partial : SolutionType.Ongoing, s.ComputationsTime, s.Data));
                        }
                    }

                    response = new Solutions(p.ProblemType, p.Id, p.CommonData, solutions);

                    // Usuwanie wysłanego problemu z listy.
                    problems.Remove(p);
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
        private void UpdateTemporaryProblems(List<TempProblem> tempProblems)
        {
            foreach (var t in tempProblems)
            {
                Problem p = problems.Find(x => x.Id == t.ProblemId);

                if (p != null)
                {
                    switch (p.Status)
                    {
                        case ProblemStatus.WaitingForDivision:
                            if(t.Status == ProblemStatus.WaitingForDivision)
                                p.Status = ProblemStatus.New;
                            break;

                        case ProblemStatus.Divided:
                            goto case ProblemStatus.WaitingForPartialSolutions;

                        case ProblemStatus.WaitingForPartialSolutions:
                            if (t.PartialProblems != null && (t.Status == ProblemStatus.WaitingForPartialSolutions || t.Status == ProblemStatus.Divided))
                            {
                                foreach (var ps in t.PartialProblems)
                                {
                                    PartialProblem pp = p.PartialProblems.Find(x => x.TaskId == ps.PartialId);

                                    if (pp != null && pp.PartialProblemStatus == PartialProblemStatus.Sended && 
                                        ps.PartialStatus == PartialProblemStatus.Sended)
                                        pp.PartialProblemStatus = PartialProblemStatus.New;
                                }

                                p.Status = p.PartialProblems.Where(
                                    x => x.PartialProblemStatus == PartialProblemStatus.New).Count() == 0 ?
                                    ProblemStatus.WaitingForPartialSolutions : ProblemStatus.Divided;
                            }
                            break;

                        case ProblemStatus.WaitingForSolutions:
                            if(t.Status == ProblemStatus.WaitingForSolutions)
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
                            Problem partialP = problems.Find(t => t.Status == ProblemStatus.PartiallySolved 
                                && node.SolvableProblems.Contains(t.ProblemType));

                            if (partialP != null)
                            {
                                partialP.Status = ProblemStatus.WaitingForSolutions;

                                List<Solution> solutions = new List<Solution>();
                                node.TemporaryProblems.Add(new TempProblem(partialP.Id, ProblemStatus.WaitingForSolutions, null));

                                foreach (var pp in partialP.PartialProblems)
                                {
                                    solutions.Add(new Solution(pp.TaskId, pp.TimeoutOccured, SolutionType.Partial, pp.ComputationsTime, pp.Data));
                                }

                                response = new Solutions(partialP.ProblemType, partialP.Id, partialP.CommonData, solutions);
                                return response;
                            }

                            Problem newP = problems.Find(t => t.Status == ProblemStatus.New 
                                && node.SolvableProblems.Contains(t.ProblemType));

                            if (newP != null)
                            {
                                newP.Status = ProblemStatus.WaitingForDivision;
                                node.TemporaryProblems.Add(new TempProblem(newP.Id, ProblemStatus.WaitingForDivision, null));
                                ulong computationalNodesCount = (ulong)computationalNodes.Sum(t => t.GetAvailableThreads());
                                response = new DivideProblem(newP.ProblemType, newP.Id, newP.Data, computationalNodesCount);
                                return response;
                            }
                        }
                        else    // ComputationalNode
                        {
                            Problem dividedP = problems.Find(t => t.Status == ProblemStatus.Divided 
                                && node.SolvableProblems.Contains(t.ProblemType));

                            if (dividedP != null)
                            {
                                List<CommunicationXML.PartialProblem> pp = 
                                    dividedP.GetPartialProblemListToSolve(node.GetAvailableThreads());

                                response =
                                    new SolvePartialProblems(dividedP.ProblemType, dividedP.Id,
                                        dividedP.CommonData, dividedP.SolvingTimeout, pp);

                                List<TempPartial> ppnums = new List<TempPartial>();

                                foreach(var x in pp)
                                {
                                    ppnums.Add(new TempPartial(x.TaskId, PartialProblemStatus.Sended));
                                }

                                node.TemporaryProblems.Add(new TempProblem(dividedP.Id, dividedP.Status, ppnums));

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
        /// Anuluje Status zadania jeśli CN/TM nie odpowiada.
        /// </summary>
        /// <param name="t">Wątek CN/TM rozwiązujący zadanie</param>
        private void UpdateProblems(ComputationalThread t)
        {
            if (t.ProblemInstanceId != null)
            {
                Problem p = problems.Find(x => x.Id == t.ProblemInstanceId);

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
                            if (p.PartialProblems != null)
                            {
                                PartialProblem pp = p.PartialProblems.Find(x => x.TaskId == t.TaskId);

                                if (pp != null && pp.PartialProblemStatus == PartialProblemStatus.Sended)
                                    pp.PartialProblemStatus = PartialProblemStatus.New;

                                p.Status = p.PartialProblems.Where(
                                    x => x.PartialProblemStatus == PartialProblemStatus.New).Count() == 0 ?
                                    ProblemStatus.WaitingForPartialSolutions : ProblemStatus.Divided;
                            }
                            break;

                        case ProblemStatus.WaitingForSolutions:
                            p.Status = ProblemStatus.PartiallySolved;
                            break;
                    }
                }
            }
        }

        /// <summary>
        /// Usuwanie nieaktywnych CN i TM.
        /// </summary>
        private void BackgroundWork()
        {
            while (working)
            {
                if (backgroundToken.Token.WaitHandle.WaitOne(timeout))
                {
                    debug.Print("Background task stopped.");
                    continue;
                }

                lock (lockObj)
                {
                    var cnl = computationalNodes.Where(x => x.LastTime.Ticks < DateTime.Now.Ticks - timeout.Ticks).ToList();
                    var tml = taskManagers.Where(x => x.LastTime.Ticks < DateTime.Now.Ticks - timeout.Ticks).ToList();

                    foreach (var cn in cnl)
                    {
                        debug.Print("CN_" + cn.Id + " removed due to timeout.");
                        // Wycofanie zmian w przypadku informacji w Temp
                        if (cn.TemporaryProblems.Count != 0)
                            UpdateTemporaryProblems(cn.TemporaryProblems);

                        // Wycofanie zmian w przypadku informacji w Node.Threads
                        foreach (var t in cn.Threads)
                            UpdateProblems(t);
                    }

                    foreach (var tm in tml)
                    {
                        debug.Print("TM_" + tm.Id + " removed due to timeout.");
                        // Wycofanie zmian w przypadku informacji w Temp
                        if (tm.TemporaryProblems.Count != 0)
                            UpdateTemporaryProblems(tm.TemporaryProblems);

                        // Wycofanie zmian w przypadku informacji w Node.Threads
                        foreach (var t in tm.Threads)
                            UpdateProblems(t);
                    }
                    
                    computationalNodes.RemoveAll(x => x.LastTime.Ticks < DateTime.Now.Ticks - timeout.Ticks);
                    taskManagers.RemoveAll(x => x.LastTime.Ticks < DateTime.Now.Ticks - timeout.Ticks);
                }

                debug.Print("Background cleanup. Nodes count: " + (computationalNodes.Count + taskManagers.Count));
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
