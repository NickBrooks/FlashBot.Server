using System.Security.Cryptography;
using System.Text;

namespace Abstrack.Data.Engine
{
    public class Tools
    {
        // https://goo.gl/sShUhC
        public static string CreateSHA256(string input)
        {
            byte[] SHA256Result;

            StringBuilder stringBuilder = new StringBuilder();
            using (HMACSHA256 shaM = new HMACSHA256())
            {
                SHA256Result = shaM.ComputeHash(Encoding.UTF8.GetBytes(input));
            }

            foreach (byte b in SHA256Result)
                stringBuilder.AppendFormat("{0:x2}", b);

            return stringBuilder.ToString();
        }
    }
}
