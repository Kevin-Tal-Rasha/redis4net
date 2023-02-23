using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace redis4net.Util.Encryption
{
    public static class EncryptionExtensions
    {
        public static string Encrypt(this string str)
        {
            return DESEncryptor.Encrypt(str);
        }

        public static string Decrypt(this string str, bool isForce = false)
        {
            string decryptStr = DESEncryptor.Decrypt(str);

            if (string.IsNullOrEmpty(decryptStr) && !string.IsNullOrEmpty(str))
            {
                if (isForce)
                    return null;
                else
                    return str;
            }
            else
            {
                return decryptStr;
            }
        }

    }
}
