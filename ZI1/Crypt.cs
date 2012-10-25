using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Runtime.InteropServices;
using ZI1.Natives;

namespace ZI1
{
    class Crypt
    {
        public static byte[] cryptWithSizing(byte[] key, byte[] val)
        {
            byte[] data = cryptDecrypt(key, val, false);
            byte[] outdata = new byte[data.Length + 4];
            Array.Copy(data, 0, outdata, 4, data.Length);
            outdata[0] = (byte)((val.Length >> 24) & 0xFF);
            outdata[1] = (byte)((val.Length >> 16) & 0xFF);
            outdata[2] = (byte)((val.Length >> 8) & 0xFF);
            outdata[3] = (byte)(val.Length & 0xFF);
            return outdata;
        }

        public static byte[] decryptWithSizing(byte[] key, byte[] val)
        {
            int sz = (val[0] << 24) + (val[1] << 16) + (val[2] << 8) + val[3];
            byte[] valwoheader = new byte[val.Length - 4];
            Array.Copy(val, 4, valwoheader, 0, valwoheader.Length);
            byte[] data = cryptDecrypt(key, valwoheader, true);
            byte[] outdata = new byte[sz];
            Array.Copy(data, outdata, outdata.Length);

            return outdata;
        }


        public static byte[] cryptDecrypt(byte[] key, byte[] val, Boolean decrypt)
        {
            IntPtr hProv = new IntPtr();
            IntPtr hHash = new IntPtr();
            IntPtr hKey = new IntPtr();

            Natives.Crypto.CryptAcquireContext(ref hProv, null, "Microsoft Enhanced Cryptographic Provider v1.0",
                Natives.Crypto.PROV_RSA_FULL, (uint)Natives.Crypto.CRYPT_VERIFYCONTEXT);

            General.CheckError();
            Natives.Crypto.CryptCreateHash(
                        hProv,
                        (uint)Natives.Crypto.CryptAlg.CALG_MD5,
                        IntPtr.Zero,
                        0,
                        ref hHash);
            General.CheckError();

            Natives.Crypto.CryptHashData(
                        hHash,
                        key,
                        (uint)key.Length,
                        0);

            General.CheckError();

            Natives.Crypto.CryptDeriveKey(
                        hProv,
                        (int)Natives.Crypto.CryptAlg.CALG_AES_192,
                        hHash,
                        0,
                        ref hKey);

            General.CheckError();

            byte[] buf;
            if (!decrypt)
            {
                uint count = (uint)val.Length;

                // Hardcoded, blocksize for AES is 8 bytes. Can we get this info from cryptoapi?
                if (val.Length % 8 == 0)
                {
                    buf = val;
                }
                else
                {
                    int sz = (int)(Math.Ceiling(val.Length / 8.0) * 8);
                    buf = new byte[sz];
                    Array.Copy(val, buf, val.Length);
                }
                Natives.Crypto.CryptEncrypt(hKey, IntPtr.Zero, 1, 0, buf,
                    ref count, (uint)buf.Length);
                General.CheckError();
            }
            else
            {
                uint count = (uint)val.Length;
                Natives.Crypto.CryptDecrypt(hKey, IntPtr.Zero, 1, 0, val,
                   ref count);
                General.CheckError();
                buf = val;
            }

            Natives.Crypto.CryptDestroyHash(hHash);
            General.CheckError();

            Natives.Crypto.CryptReleaseContext(hProv, 0);
            General.CheckError();
            return buf;

        }
    }
}
