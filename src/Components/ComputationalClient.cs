using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Threading;

namespace Components
{

    public class ComputationalClient
    {
        private static string ProblemDataFilepath { get;  set; }
        public ulong ProblemId { get; private set; }


        /**
         * Constructor will be changed, 
         * as we will get to know which parameters will be needed
         */
        public ComputationalClient()
        {
            
        }

        public void chooseFile()
        {
            Thread newThread = new Thread(new ThreadStart(openAndSendXMLFileDialog));
            newThread.SetApartmentState(ApartmentState.STA);
            newThread.Start();  
        }


        //use ConnectionContext (?) in these methods
        public static int registerProblem()
        {
            int problemInstanceId = 0;
    
            return problemInstanceId;
        }

        public string getProblemStatus()
        {
            
            return null;
        }



        /**
         *  Open dialog to choose XML file
         *  if the file is chosen send request to register problem to CS
         */
        static void openAndSendXMLFileDialog()
        {
            
            var dialog = new OpenFileDialog
            {
                Multiselect = false,
                Title = "Select XML Document",
                Filter = "XML Document|*.xml;"
            };
            using (dialog)
            {
                if (dialog.ShowDialog() == DialogResult.OK)     
                {
                    ProblemDataFilepath = dialog.FileName;
                    Console.WriteLine("You have chosen {0} to solve", ProblemDataFilepath);
                }
            }


            if (ProblemDataFilepath != null)
            {
             /*
             * TODO:
             * Send problem to Communication Server
             * */
                registerProblem();
            }
            
        }









    }
}
