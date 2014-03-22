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
        static String usage_info = "Computational Client usage: \n"+
                                   "press ENTER to load file with problem data\n"+
                                   "press ESC to close Client\n";

        //CC usage info to show after problem register
        static String usage_info_after_register = "Problem registered successfully\n"+
                                                  "press A to check problem status\n";

        
        /**
         *  in Main function we only handle user's key press
         * */
        [STAThread]
        static void Main(string[] args)
        {
            string ip_address;
            string port; 
            string timeout;
            string problem_type;
            bool is_registered = false;
          
            Console.Write("IP Address ");
            ip_address = Console.ReadLine();
            Console.Write("Port number: ");
            port = Console.ReadLine();
            Console.Write("problem_type: ");
            problem_type = Console.ReadLine();
            Console.Write("(optionally) timeout: ");
            timeout = Console.ReadLine();

            ComputationalClient new_client ;
            if(timeout!=null)
                new_client = new ComputationalClient(ip_address,int.Parse(port),ulong.Parse(timeout),problem_type);
            else 
                new_client = new  ComputationalClient(ip_address,int.Parse(port),null,problem_type);

            Console.Write(usage_info);

            ConsoleKeyInfo info = Console.ReadKey();
            if (info.Key == ConsoleKey.Enter)
            {
                string chosen_file = null;
             
                if ((chosen_file = chooseFile()) != null)
                {
                    is_registered = new_client.registerProblem(chosen_file);
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
                while (true);

            }
            else
                Console.WriteLine("Client is not registered");

            Console.Read();
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
