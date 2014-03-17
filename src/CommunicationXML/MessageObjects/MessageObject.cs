using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CommunicationXML
{
    /// <summary>
    /// Wylicznie nazw klas, które reprezentują różne wiadomości
    /// </summary>
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

    /// <summary>
    /// Abstrakcyjna klasa bazowa dla klas wiadomości
    /// </summary>
    public abstract class MessageObject
    {
        /// <summary>
        /// Zwraca zawartość obiektu jako XML w formacie binarnym - gotowy do wysłania
        /// </summary>
        public abstract byte[] GetXmlData();


    }
}
