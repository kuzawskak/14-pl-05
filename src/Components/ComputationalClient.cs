using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Threading;
using System.Xml;
using CommunicationNetwork;
using CommunicationXML;

namespace Components
{
    //TODO:
   // add method to close node, but the id must be saved in some file
    //w konstruktorze sprawdzic czy jest cos w pliku z zapisanym stanem
    //plik z numerem portu dodatkwoo
    //jak zakonczy przetwarzanie po time-out lub rozwiazniu problemu to usuwamy plik




    public class ComputationalClient
    {
        private Listener listener;
        private ulong ProblemId { get;  set; }


        /**
         * Constructor will be changed, 
         * as we will get to know which parameters will be needed
         * the connection with server must be set
         */
        public ComputationalClient(int port_number)
        {
            listener.Start();
            listener = new Listener(port_number, ConnectionHandler);
        }



        //implementacja handlera dla klienta
        //wiadomosci jakie moze otrzymac klient:
        //potwierdzenie rejestracji, status zadania (zawarte w nim ewentualnie rozwiazanie koncowe)
        private void ConnectionHandler(byte[] data, ConnectionContext ctx)
        {
            XMLParser parser = new XMLParser(data);           
                switch (parser.MessageType)
                {
                    case MessageTypes.RegisterResponse:
                        ProblemId = BitConverter.ToUInt64(parser.Message.GetXmlData(), 0);
                        Console.WriteLine("Problem registered with nr: %l",ProblemId);
                        break;

                    case MessageTypes.Status:
                        string status ="";
                        Console.WriteLine("Status problemu: %s", status);
                        //TODO:
                        //sprawdz jaki status: error, rozwiazywanie,rozwiazany?
                        break;
                }
          
        }
        

        public void chooseFileAndRegister()
        {
            var dialog = new OpenFileDialog
            {
                Multiselect = false,
                Title = "Select XML Document",
                Filter = "XML Document|*.xml;"
            };

            string problem_data_filepath = null;

            using (dialog)
            {
                if (dialog.ShowDialog() == DialogResult.OK)
                {
                    problem_data_filepath = dialog.FileName;
                    Console.WriteLine("You have chosen {0} to solve", problem_data_filepath);
                }
            }


            if (problem_data_filepath != null)
            {
                XmlDocument problem_data_xml = new XmlDocument();
                problem_data_xml.Load(problem_data_filepath);
                byte[] problem_data_bytes = Encoding.Default.GetBytes(problem_data_xml.OuterXml);
                registerProblem(problem_data_bytes);
            }
        }


        public static void registerProblem(byte[] problem_data)
        {
            SolveRequest solve_request = new SolveRequest();
            ConnectionContext cc = new ConnectionContext(System.Threading.Thread.CurrentThread);
            cc.Send(solve_request.GetXmlData()); 
          
        }

        public void getProblemStatus()
        {            
            SolveRequest solve_request = new SolveRequest();
            ConnectionContext cc = new ConnectionContext(System.Threading.Thread.CurrentThread);
            cc.Send(solve_request.GetXmlData());  
        }




        //metoda obslugujaca zamkniecie CC na czas przetwarzania
        public void TemporaryCloseForComputation()
        {
            //zapis stanu (port na ktorym nasluchuje i ProblemId) do XML lub innego pliku


        }

    }
}
