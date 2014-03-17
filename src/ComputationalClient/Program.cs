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
        static void Main(string[] args)
        {
            string port;           
            Console.Write("Port number: ");
            port = Console.ReadLine();
            
            ComputationalClient new_client = new ComputationalClient(int.Parse(port));

            Console.Write(usage_info);

            ConsoleKeyInfo info = Console.ReadKey();
            if (info.Key == ConsoleKey.Enter)
            {
                new_client.chooseFileAndRegister();                                               
            }

            Console.Write(usage_info_after_register);
            info = Console.ReadKey();
            if (info.Key == ConsoleKey.A)
            {
                //send request to get problem status
                new_client.getProblemStatus();
            }
                 
            Console.Read();


        }
    }
}
