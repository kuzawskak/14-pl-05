using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Net.Sockets;
using System.Net;
using System.Threading;

namespace CommunicationNetwork
{
    public class NetworkListener
    {
        TcpListener tcp;
        bool is_running = false;
        int port;
        ConnectionHandler ch;
        const ushort pack_len = 512;
        List<Thread> threads = null;

        public delegate void ConnectionHandler(byte[] data, ConnectionContext ctx);

        /// <summary>
        /// Startowanie listenera
        /// </summary>
        public void Start()
        {
            try
            {
                if (is_running)
                    return;
                IPAddress ipAddress = Dns.GetHostEntry("localhost").AddressList[0];
                if (port < 0)
                    throw new Exception("Port value is invalid");
                tcp = new TcpListener(ipAddress, port);
                tcp.Start();
                is_running = true;

                while (true)
                {
                    // debug purpose
                    Console.WriteLine("Waiting for connection...");
                    TcpClient cli = tcp.AcceptTcpClient();
                    Console.WriteLine("Client has been accepted!");

                    // create a new thread for data analisys
                    Thread t = new Thread(new ParameterizedThreadStart(ThreadWork));
                    
                    // add try chatch block, if thread interrupts
                    threads.Add(t);
                    t.Start(cli);
                    /*
                    // should be carefully tested!!!
                    foreach(Thread _t in threads)
                        if (t.ThreadState != ThreadState.Running) {
                            _t.Join();
                            threads.Remove(_t);
                        }
                    */
                }
            }
            catch (SocketException se)
            {
                Stop();
                Console.WriteLine("Server had been stopped (SocketException): " + se.Message);

            }
            catch (Exception e)
            {
                Stop();
                Console.WriteLine("Server had been stopped (UnexpectedException): " + e.Message);
            }
        }
        /// <summary>
        /// Konstruktor
        /// </summary>
        /// <param name="port"></param>
        /// <param name="handler"></param>
        public NetworkListener(int port, ConnectionHandler handler)
        {
            this.port = port;
            ch = handler;
            threads = new List<Thread>();
        }
        /// <summary>
        /// Wątek zarządzający komunikacją
        /// </summary>
        /// <param name="cli"></param>
        void ThreadWork(object cli)
        {
            Console.WriteLine("ComputationalThread has been started");
            TcpClient _cli = null;
            try
            {
                _cli = (TcpClient)cli;
            }
            catch (Exception e)
            {
                Console.WriteLine(e.Message);
                Thread.CurrentThread.Interrupt();
            }
            NetworkStream ns = _cli.GetStream();

            // read data from stream
            byte[] bytes = new byte[pack_len];
            List<byte> lb = new List<byte>();
            //string packet = "";
            int read_bytes;
            do {
                read_bytes = ns.Read(bytes, 0, pack_len);
                Console.WriteLine("received: " + read_bytes);
                // add to list of partial read packet
                lb.AddRange(bytes.Take(read_bytes).ToArray());
                //lb.Add(((byte[])bytes.Clone()));
            }
            while (ns.DataAvailable);

            // put all data in one byte array
            //uint ps = 0;
            //foreach (byte[] b in lb)
                //ps += (uint)b.Length;

            // whole packet
            byte[] data = lb.ToArray();//new byte[ps];

            // copy data
            //int i = 0;
            //foreach (byte[] b in lb)
            //    foreach (byte _b in b)
            //        data[i++] = _b;

            // !!! CALL SERVER CONTEXT !!!
            ConnectionContext cc = new ConnectionContext(Thread.CurrentThread);
            if (ch != null)
            {
                ch(data, cc);
                // ***********************
                // waiting for event
                // ***********************
                //Thread.Sleep(Timeout.Infinite);
            }

            // interrupting by send method
            // message to send
            // test purposes
            data = cc.GetMessage();
            if (data != null)
                //data = System.Text.Encoding.ASCII.GetBytes("invalid input");
                ns.Write(data, 0, data.Length);
            _cli.Close();
        }

        public void Stop() {
            // kill all threads
            //List<Thread> _t = threads;
            foreach (Thread t in threads) {
                t.Abort();
                //_t.Remove(t);
            }
            foreach (Thread t in threads)
                t.Join();

            if (is_running)
                tcp.Stop();
            is_running = false;
        }

        public bool IsRunning() {
            return is_running;
        }

        /// <summary>
        /// Lokalny adres IP
        /// </summary>
        /// <returns></returns>
        //string LocalIPAddress()
        //{
        //    IPHostEntry host;
        //    string localIP = "";
        //    host = Dns.GetHostEntry(Dns.GetHostName());
        //    foreach (IPAddress ip in host.AddressList)
        //    {
        //        if (ip.AddressFamily == AddressFamily.InterNetwork)
        //        {
        //            localIP = ip.ToString();
        //            break;
        //        }
        //    }
        //    return localIP;
        //}
    }
}

