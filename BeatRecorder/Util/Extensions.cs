using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;

namespace BeatRecorder
{
    class Extensions
    {
        /// <summary>
        /// Used for authentication with OBS Websocket if authentication is required ("Borrowed" from https://github.com/BarRaider/obs-websocket-dotnet/blob/268b7f6c52d8daf8e8d08cf517812009c6f9cc26/obs-websocket-dotnet/OBSWebsocket.cs#L797)
        /// </summary>
        /// <param name="input"></param>
        /// <returns></returns>
        internal protected static string HashEncode(string input)
        {
            using var sha256 = SHA256.Create();

            byte[] textBytes = Encoding.ASCII.GetBytes(input);
            byte[] hash = sha256.ComputeHash(textBytes);

            return System.Convert.ToBase64String(hash);
        }
    }
}
