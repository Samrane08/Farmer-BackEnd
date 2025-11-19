using System.Security.Cryptography;
using System.Text;

namespace Helper;

public static class Utility
{
    // string EncryptKey = "@bm@cde@m@h@0nl!ne@11823";
    //string EncryptIV = "BMC@02@1";
    public static int CreateOtp()
    {
        int min = 100000;
        int max = 999999;
        int otp = 0;
        Random rdm = new Random();
        otp = rdm.Next(min, max);
        return otp;
    }
    public static string ReturnStrEncryptCode(string id)
    {
        string msg = string.Empty;
        msg = Encrypt(id, "sblw-3hn9-sqoy59");
        return msg;
    }
    public static string ReturnStrDecryptCode(string id)
    {
        string msg = string.Empty;
        string ids = id.Replace(" ", "+");
        msg = Decrypt(ids, "sblw-3hn9-sqoy59");
        return msg;
    }
    public static string Encrypt(string input, string key)
    {
        byte[] inputArray = UTF8Encoding.UTF8.GetBytes(input);
        TripleDESCryptoServiceProvider tripleDES = new TripleDESCryptoServiceProvider();
        tripleDES.Key = UTF8Encoding.UTF8.GetBytes(key);
        tripleDES.Mode = CipherMode.ECB;
        tripleDES.Padding = PaddingMode.PKCS7;
        ICryptoTransform cTransform = tripleDES.CreateEncryptor();
        byte[] resultArray = cTransform.TransformFinalBlock(inputArray, 0, inputArray.Length);
        tripleDES.Clear();
        return Convert.ToBase64String(resultArray, 0, resultArray.Length);
    }
    public static string Decrypt(string input, string key)
    {
        byte[] inputArray = Convert.FromBase64String(input);
        TripleDESCryptoServiceProvider tripleDES = new TripleDESCryptoServiceProvider();
        tripleDES.Key = UTF8Encoding.UTF8.GetBytes(key);
        tripleDES.Mode = CipherMode.ECB;
        tripleDES.Padding = PaddingMode.PKCS7;
        ICryptoTransform cTransform = tripleDES.CreateDecryptor();
        byte[] resultArray = cTransform.TransformFinalBlock(inputArray, 0, inputArray.Length);
        tripleDES.Clear();
        return UTF8Encoding.UTF8.GetString(resultArray);
    }
    public static string SimpleTripleDes(string Data, string strKey, string striv)
    {
        byte[] key = Encoding.UTF8.GetBytes(strKey);
        byte[] iv = Encoding.UTF8.GetBytes(striv);
        byte[] data = Encoding.UTF8.GetBytes(Data);
        byte[] enc = new byte[0];
        TripleDES tdes = TripleDES.Create();
        tdes.IV = iv;
        tdes.Key = key;
        tdes.Mode = CipherMode.CBC;
        tdes.Padding = PaddingMode.Zeros;
        ICryptoTransform ict = tdes.CreateEncryptor();
        enc = ict.TransformFinalBlock(data, 0, data.Length);
        return ByteArrayToString(enc);
    }
    public static string SimpleTripleDesDecrypt(string Data, string strKey, string striv)
    {
        byte[] key = Encoding.UTF8.GetBytes(strKey);
        byte[] iv = Encoding.UTF8.GetBytes(striv);
        byte[] data = StringToByteArray(Data);
        byte[] enc = new byte[0];
        TripleDES tdes = TripleDES.Create();
        tdes.IV = iv;
        tdes.Key = key;
        tdes.Mode = CipherMode.CBC;
        tdes.Padding = PaddingMode.Zeros;
        ICryptoTransform ict = tdes.CreateDecryptor();
        enc = ict.TransformFinalBlock(data, 0, data.Length); 
        return Encoding.UTF8.GetString(enc).TrimEnd('\0').TrimEnd('|');
    }
    public static byte[] StringToByteArray(string hex)
    {
        int NumberChars = hex.Length;
        byte[] bytes = new byte[NumberChars / 2];
        for (int i = 0; i < NumberChars; i += 2)
            bytes[i / 2] = Convert.ToByte(hex.Substring(i, 2), 16);
        return bytes;
    }
    public static string ByteArrayToString(byte[] ba)
    {
        string hex = BitConverter.ToString(ba);
        return hex.Replace("-", "");
    }
    public static DateTime datetoserver()
    {
        string zoneId = "India Standard Time";
        TimeZoneInfo tzi = TimeZoneInfo.FindSystemTimeZoneById(zoneId);
        DateTime result = TimeZoneInfo.ConvertTimeFromUtc(DateTime.UtcNow, tzi);
        return result;
    }

    public static string GetMd5Hash(string input)
    {
        // Create an MD5 instance
        using (MD5 md5 = MD5.Create())
        {
            // Convert the input string to a byte array and compute the hash
            byte[] data = md5.ComputeHash(Encoding.UTF8.GetBytes(input));

            // Create a StringBuilder to collect the bytes and create a string
            StringBuilder sb = new StringBuilder();
            for (int i = 0; i < data.Length; i++)
            {
                sb.Append(data[i].ToString("x2"));
            }

            // Return the hexadecimal string
            return sb.ToString();
        }
    }
    public static string HashPasswordWithSaltSHA256(string password, string salt)
    {
        byte[] saltBytes = Convert.FromBase64String(salt);
        byte[] passwordBytes = Encoding.UTF8.GetBytes(password);

        using (var deriveBytes = new Rfc2898DeriveBytes(passwordBytes, saltBytes, 1000))
        {
            byte[] hashBytes = deriveBytes.GetBytes(32); // 32 bytes = 256 bits
            return BitConverter.ToString(hashBytes).Replace("-", "").ToUpper();
        }
    }

    public static string CreateMD5(string input)
    {
        using (System.Security.Cryptography.MD5 md5 = System.Security.Cryptography.MD5.Create())
        {
            byte[] inputBytes = System.Text.Encoding.ASCII.GetBytes(input);
            byte[] hashBytes = md5.ComputeHash(inputBytes);
            return Convert.ToHexString(hashBytes);
        }
    }
}
