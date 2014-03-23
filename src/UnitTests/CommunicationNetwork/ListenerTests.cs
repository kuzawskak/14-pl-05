using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using CommunicationNetwork;
using System.Net.Sockets;
using System.Threading;
using Components;
using System.Text;
using System.Collections.Generic;
using System.Linq;
using CommunicationXML;
using System.Threading.Tasks;

namespace UnitTests.CommunicationNetwork
{
    [TestClass]
    public class ListenerTest
    {
        delegate bool memcmp(byte[] d1, byte[] d2);
        [TestMethod]
        public void TestStartStop() {
            NetworkListener.ConnectionHandler ch = (d, cc) => { cc.Send(d); }; 
            byte[] data = new Register(NodeType.ComputationalNode, 1, new List<string>() { "test1", "test2" }).GetXmlData();
            byte[] bytes = null;
            NetworkClient nc = null;
            NetworkListener nl = null;
            NetworkClient[] _nc = null;
            Thread[] _t = null;
            Thread t = null;
            bool?[] _state = null;
            const int port = 22222;
            memcmp mcmp = (d1, d2) => { if (d1.Length != d2.Length) return false;
                                        for (int i = 0; i < d1.Length; ++i)
                                            if (d1[i] != d2[i])
                                                return false;
                                        return true; 
                                        };

            // *** Valid port: Listener is started (one client) ***
            nl = new NetworkListener(port, ch);
            Assert.IsNotNull(nl);
            t = new Thread(nl.Start);
            Assert.IsNotNull(t);
            t.Start();

            nc = new NetworkClient("localhost", port);

            bytes = nc.Work(data);
            Assert.IsNotNull(bytes);
            Assert.IsTrue(mcmp(bytes, data));
            
            // cleaning
            nl.Stop();
            Assert.IsFalse(nl.IsRunning());
            t.Abort();
            t.Join();

            // *** Invalid port: Listener is trying to start ***
            nl = new NetworkListener(-1, ch);
            Assert.IsNotNull(nl);

            nl.Start();
            Assert.IsFalse(nl.IsRunning());

            // *** Valid port: Listener is started (few clients) ***
            nl = new NetworkListener(port, ch);
            Assert.IsNotNull(nl);

            t = new Thread(nl.Start);
            Assert.IsNotNull(t);
            t.Start();

            // few clients connection simulation
            const int clients = 5;
            _nc = new NetworkClient[clients];
            _t = new Thread[clients];
            _state = new bool?[clients];

            Assert.IsNotNull(_nc);
            Assert.IsNotNull(_t);
            Assert.IsNotNull(_state);
            /*
            for (int j = 0; j < clients; ++j) {
                _nc[j] = new NetworkClient("localhost", port);
                _t[j] = new Thread(() => {
                                    byte[] _b = _nc[j].Work(data);
                                    if (_b == null) {
                                        _state[j] = false;
                                        return;
                                    }
                                    //Assert.IsTrue(mcmp(_b, data));
                                    _state[j] = mcmp(_b, data);
                                    });

                _t[j].Start();
            }
            */

            _nc[0] = new NetworkClient("localhost", port);
            _t[0] = new Thread(() =>
            {
                byte[] _b = _nc[0].Work(data);
                if (_b == null)
                {
                    _state[0] = false;
                    return;
                }
                //Assert.IsTrue(mcmp(_b, data));
                _state[0] = mcmp(_b, data);
            });

            _t[0].Start();

            _nc[1] = new NetworkClient("localhost", port);
            _t[1] = new Thread(() =>
            {
                byte[] _b = _nc[1].Work(data);
                if (_b == null)
                {
                    _state[1] = false;
                    return;
                }
                //Assert.IsTrue(mcmp(_b, data));
                _state[1] = mcmp(_b, data);
            });

            _t[1].Start();

            _nc[2] = new NetworkClient("localhost", port);
            _t[2] = new Thread(() =>
            {
                byte[] _b = _nc[2].Work(data);
                if (_b == null)
                {
                    _state[2] = false;
                    return;
                }
                //Assert.IsTrue(mcmp(_b, data));
                _state[2] = mcmp(_b, data);
            });

            _t[2].Start();

            _nc[3] = new NetworkClient("localhost", port);
            _t[3] = new Thread(() =>
            {
                byte[] _b = _nc[3].Work(data);
                if (_b == null)
                {
                    _state[3] = false;
                    return;
                }
                //Assert.IsTrue(mcmp(_b, data));
                _state[3] = mcmp(_b, data);
            });

            _t[3].Start();

            _nc[4] = new NetworkClient("localhost", port);
            _t[4] = new Thread(() =>
            {
                byte[] _b = _nc[4].Work(data);
                if (_b == null)
                {
                    _state[4] = false;
                    return;
                }
                //Assert.IsTrue(mcmp(_b, data));
                _state[4] = mcmp(_b, data);
            });

            _t[4].Start();


            // czekanie na wątki klientów

            for (int k = 0; k < clients; ++k) {
                _t[k].Join();
                Assert.IsTrue((bool)_state[k]);
            }
            
            // cleaning
            nl.Stop();
            t.Abort();
            t.Join();
        }
    }
}
