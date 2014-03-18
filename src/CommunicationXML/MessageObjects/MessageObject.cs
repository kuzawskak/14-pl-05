using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Serialization;

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
        private const string UCC = "ucc";
        protected const string ADRES = "http://www.mini.pw.edu.pl/ucc/";

        /// <summary>
        /// Zwraca zawartość obiektu jako XML w formacie binarnym - gotowy do wysłania
        /// </summary>
        public abstract byte[] GetXmlData();

        /// <summary>
        /// Atrybut potrzebny do poprawnego serializowania do XML
        /// </summary>
        [XmlNamespaceDeclarations]
        public XmlSerializerNamespaces xmlns;

        public MessageObject()
        {
            xmlns = new XmlSerializerNamespaces();
            xmlns.Add(UCC, ADRES);
        }
    }
}
