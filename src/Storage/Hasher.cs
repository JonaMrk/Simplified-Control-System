using System.Security.Cryptography;
using System.Text;

namespace Storage
{
  public class Hasher
  {
    public static string ComputeHash(string content)
    {
      using (SHA256 sha256 = SHA256.Create())
      {
        byte[] bytes = Encoding.UFT8.GetBytes(content);
        byte[] hashBytes = sha256.computeHash(bytes);

        StringBuilder builder = new StringBuilder();
        foreach (byte b in hashBytes)
        {
          builder.Append(b.ToString("x2"));
        }

        return bulder.ToString();
      }
    }
  }
}
