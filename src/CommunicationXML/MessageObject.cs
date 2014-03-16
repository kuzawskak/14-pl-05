using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CommunicationXML
{
    public enum MessageTypes
    {
        Register,
        RegisterResponse,
        Status,
        SolveRequest,
        SolveRequestResponse,
        DivideProblem,
        SolutionRequest,
        SolvePartialProblems,
        Solutions
    }
    public class MessageObject
    {
        // Tworzy XML z obiektu klasy do wysłania jako dane binarne.
        // Dopisałem żeby się kompilowało - Mateusz
        public byte[] XMLData;
    }
}
