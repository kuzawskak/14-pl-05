using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Net.Sockets;

namespace CommunicationNetwork
{
    /// <summary>
    /// Klasa dla symulowania klienta sieci
    /// </summary>
    public class NetworkClient
    {
        TcpClient cli = null;
        int port;
        string address;
        const int pack_len = 512;

        public NetworkClient(string _address, int _port) {
            address = _address;
            port = _port;
        }

        /// <summary>
        /// metoda, która odpowiada za komunikację z Listenerem
        /// </summary>
        /// <param name="data"></param>
        /// <returns></returns>
        public byte[] Work(byte[] data) {
            try {
                if (address == null || port < 0)
                    throw new Exception("Invaild address or port");
                if (data == null)
                    throw new Exception("data is null");

                cli = new TcpClient(address, port);

                NetworkStream ns = cli.GetStream();
                ns.Write(data, 0, data.Length);
                List<byte> bytes = new List<byte>();
                byte[] buf = new byte[pack_len];
                int len = 0;
                do {
                    len = ns.Read(buf, 0, pack_len);

                    for (int i = 0; i < len; ++i)
                        bytes.Add(buf[i]);
                }
                while (ns.DataAvailable);

                ns.Close();
                cli.Close();

                return bytes.Count != 0 ? bytes.ToArray() : null;
            }
            catch (Exception e) { 
                // should be done in another way
                Console.WriteLine("NetworkClient: " + e.Message);
                if (cli != null)
                    cli.Close();
                return null;
            }
        }
    }
}
