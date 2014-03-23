using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Threading;

namespace Components
{
    class Program
    {
        
        //CC usage info to show on start
        static String usage_info = "______________________________\n"+
                                   "Computational Client usage: \n"+
                                   "press ENTER to load file with problem data\n"+
                                   "press ESC to close Client\n";

        //CC usage info to show after problem register
        static String usage_info_after_register = "________________________________\n"+
                                                  "Problem registered successfully\n"+
                                                  "press A to check problem status\n";

        
        /**
         *  in Main function we only handle user's key press
         * */
        [STAThread]
        static void Main(string[] args)
        {
            string ip_address;
            string port;
            ulong timeout;
            ulong? n_timeout;
            string problem_type;
            bool is_registered = false;
            ulong? problem_id = null;

            Console.WriteLine("Do you want to check status of existing problem? (y/n)\n");
            string dec = Console.ReadLine();
            if (dec == "y")
            {
                Console.Write("problem id:  ");
                problem_id = ulong.Parse(Console.ReadLine());
            }

          
            Console.Write("IP Address:  ");
            ip_address = Console.ReadLine();
            Console.Write("Port number: ");
            port = Console.ReadLine();
            Console.Write("problem_type: ");
            problem_type = Console.ReadLine();
            Console.Write("(optionally) timeout: ");
            UInt64.TryParse(Console.ReadLine(),out timeout);
            if (timeout == 0) n_timeout = null;
            else n_timeout = timeout;
            ComputationalClient new_client;
 
            new_client = new ComputationalClient(problem_id,ip_address, int.Parse(port), n_timeout, problem_type);

            ConsoleKeyInfo info;
            if (dec == "y") is_registered = true;
            else
            {
                Console.Write(usage_info);

                 info = Console.ReadKey();
                if (info.Key == ConsoleKey.Enter)
                {
                    string chosen_file = null;

                    if ((chosen_file = chooseFile()) != null)
                    {
                        is_registered = new_client.registerProblem(chosen_file);
                    }

                }
            }

            if (is_registered)
            {
                new_client.Work();
                Console.Write(usage_info_after_register);
                do
                {

                    info = Console.ReadKey();
                    if (info.Key == ConsoleKey.A)
                    {
                        //pobranie statusu problemu na żądanie
                        new_client.getProblemStatus();
                    }
    
                }
                while (info.Key!=ConsoleKey.Escape);

            }
            else
                Console.WriteLine("Client is not registered");

            return;
        }


        public static string chooseFile()
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
            return problem_data_filepath;
        }
    }
}
