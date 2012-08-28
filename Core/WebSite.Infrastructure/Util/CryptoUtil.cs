using System.Text;

namespace Website.Infrastructure.Util
{
    public static class CryptoUtil
    {
        //use when implementing stsprinicpal UniqueIdentifier
        public static string CalculateHash(string input)
        {
            var sha1 = System.Security.Cryptography.SHA1.Create();
            var inputBytes = System.Text.Encoding.ASCII.GetBytes(input);
            var hash = sha1.ComputeHash(inputBytes);

            var sb = new StringBuilder();
            for (var i = 0; i < hash.Length; i++)
                sb.Append(hash[i].ToString("x2"));

            return sb.ToString();
        }
    }
}