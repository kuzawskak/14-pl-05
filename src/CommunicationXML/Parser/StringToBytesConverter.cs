using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CommunicationXML
{
    /// <summary>
    /// Statyczna klasa pomagająca przy przekształcaniu stringa na tablicę bajtów i odwrotnie
    /// </summary>
    public static class StringToBytesConverter
    {
        /// <summary>
        /// Statyczna metoda do zmiany stringa w tablicę bajtów
        /// </summary>
        /// <param name="stringToConvert">string do konwersji</param>
        /// <returns>Otrzymana tablica bajtów</returns>
        public static byte[] GetBytes(string stringToConvert)
        {
            Console.WriteLine(stringToConvert);

            byte[] convertedBytes = new byte[stringToConvert.Length * sizeof(char)];
            System.Buffer.BlockCopy(stringToConvert.ToCharArray(), 0, convertedBytes, 0, convertedBytes.Length);
            return convertedBytes;
        }

        /// <summary>
        /// Statyczna metoda do zmiany tablicy bajtów w stringa
        /// </summary>
        /// <param name="bytesToConvert">Tablica bajtów do konwersji</param>
        /// <returns>Otrzymany string</returns>
        public static string GetString(byte[] bytesToConvert)
        {
            char[] chars = new char[bytesToConvert.Length / sizeof(char)];
            System.Buffer.BlockCopy(bytesToConvert, 0, chars, 0, bytesToConvert.Length);

            Console.WriteLine(chars);

            return new string(chars);
        }
    }
}
