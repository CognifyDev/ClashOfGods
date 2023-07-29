using System.Security.Cryptography;
using System.Text;

namespace COG.Utils;

public static class StringUtils
{
    public static string GetSHA1Hash(this string input)
    {
        using SHA1 sha1 = SHA1.Create();
        var inputBytes = Encoding.UTF8.GetBytes(input);
        var hashBytes = sha1.ComputeHash(inputBytes);

        var sb = new StringBuilder();
        foreach (var b in hashBytes)
        {
            sb.Append(b.ToString("x2"));
        }

        return sb.ToString();
    }
}