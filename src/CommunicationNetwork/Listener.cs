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
    public class Listener
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
                    throw new Exception("Listener is already listening on port: " + port);
                tcp = new TcpListener(IPAddress.Parse(LocalIPAddress()), port);
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
                }
            }
            catch (SocketException se)
            {
                // start musi byc ustawiony na false
                if (is_running)
                    tcp.Stop();
                is_running = false;
                Console.WriteLine("Listener start failure on port: " + port);
                Console.WriteLine(se.Message);

                // wait for all threads
                foreach (Thread t in threads)
                    t.Join();
            }
            catch (Exception e)
            {
                // musi byc LogWriter, ale no, coz, mamy debug :)
                if (is_running)
                    tcp.Stop();
                is_running = false;
                Console.WriteLine(e.Message);

                // wait for all threads
                foreach (Thread t in threads)
                    t.Join();
            }
        }
        /// <summary>
        /// Konstruktor
        /// </summary>
        /// <param name="port"></param>
        /// <param name="handler"></param>
        public Listener(int port, ConnectionHandler handler)
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
            List<byte[]> lb = new List<byte[]>();
            //string packet = "";
            int read_bytes;
            while ((read_bytes = ns.Read(bytes, 0, pack_len)) != 0)
            {
                Console.WriteLine("received: " + read_bytes);

                // add to list of partial read packet
                lb.Add(((byte[])bytes.Clone()));
            }

            // put all data in one byte array
            uint ps = 0;
            foreach (byte[] b in lb)
                ps += (uint)b.Length;

            // whole packet
            byte[] data = new byte[ps];

            // copy data
            int i = 0;
            foreach (byte[] b in lb)
                foreach (byte _b in b)
                    data[i] = _b;

            // !!! CALL SERVER CONTEXT !!!
            ConnectionContext cc = new ConnectionContext(Thread.CurrentThread);
            if (ch != null)
            {
                ch(data, cc);
                // ***********************
                // waiting for event
                // ***********************
                Thread.Sleep(Timeout.Infinite);
            }

            // interrupting by send method
            // message to send
            // test purposes
            data = cc.GetMessage();
            if (data == null)
                data = System.Text.Encoding.ASCII.GetBytes("invalid input");
            ns.Write(data, 0, data.Length);
            _cli.Close();
        }

        /// <summary>
        /// Lokalny adres IP
        /// </summary>
        /// <returns></returns>
        string LocalIPAddress()
        {
            IPHostEntry host;
            string localIP = "";
            host = Dns.GetHostEntry(Dns.GetHostName());
            foreach (IPAddress ip in host.AddressList)
            {
                if (ip.AddressFamily == AddressFamily.InterNetwork)
                {
                    localIP = ip.ToString();
                    break;
                }
            }
            return localIP;
        }
    }
}

