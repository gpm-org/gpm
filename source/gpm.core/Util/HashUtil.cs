using System.Security.Cryptography;

namespace gpm.Core.Util;

public static class HashUtil
{
    /// <summary>
    /// Convert a byte array to hex string representation
    /// </summary>
    /// <param name="bytes"></param>
    /// <returns></returns>
    public static string BytesToString(byte[] bytes)
    {
        var sb = new System.Text.StringBuilder(bytes.Length * 2);
        foreach (var b in bytes)
        {
            sb.Append(b.ToString("X2"));
        }
        return sb.ToString();
    }

    /// <summary>
    /// Compute sha512 from a byte array
    /// </summary>
    /// <param name="bytes"></param>
    /// <returns></returns>
    public static byte[] Sha512Bytes(byte[] bytes)
    {
        using var sha512 = SHA512.Create();
        return sha512.ComputeHash(bytes);
    }

    /// <summary>
    /// Compute sha512 from a stream
    /// </summary>
    /// <param name="stream"></param>
    /// <returns></returns>
    public static byte[] Sha512Bytes(Stream stream)
    {
        stream.Seek(0, SeekOrigin.Begin);
        using var sha512 = SHA512.Create();
        return sha512.ComputeHash(stream);
    }

}
