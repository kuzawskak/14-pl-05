using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CommunicationXML
{
    /// <summary>
    /// Klasa odpowiadająca wiadomości ReisterResponseMessage
    /// </summary>
    public class RegisterResponse : MessageObject
    {
        
        /// <summary>
        /// Id komponentu nadane przez Server
        /// </summary>
        public ulong Id
        {
            get { return id; }
            set { id = value; }
        }
        private ulong id;

        /// <summary>
        /// Timeout serwera
        /// </summary>
        public TimeSpan Timeout
        {
            get { return timeout; }
            set { timeout = value; }
        }
        private TimeSpan timeout;

        /// <summary>
        /// Konstruktor
        /// </summary>
        /// <param name="id">Id komponentu, który nadał Server</param>
        /// <param name="timeout">Timeout serwera</param>
        public RegisterResponse(ulong _id, TimeSpan _timeout)
        {
            //Sprawdzenie poprawności parametrów
            if (_timeout == null)
                throw new System.ArgumentNullException();

            id = _id;
            timeout = _timeout;
        }
    }
}
