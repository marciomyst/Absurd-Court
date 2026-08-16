using System.Security.Cryptography;
using System.Text;

namespace AbsurdCourt.Domain.Rooms;

internal static class ReconnectTokenHasher
{
    public static string Hash(Guid token) =>
        Convert.ToHexString(SHA256.HashData(Encoding.UTF8.GetBytes(token.ToString("N"))));
}
