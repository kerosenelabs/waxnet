﻿using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Security.Cryptography;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;
using System.Web;

namespace WAX.Utils
{
    static class Extensions
    {
        public static long GetChatId(this string id)
        {
            return Convert.ToInt64(id.Replace("@s.whatsapp.net", ""));
        }

        public static string GetChatId(this long id)
        {
            return id.ToString() + "@s.whatsapp.net";
        }

        public static string GetTag(this Api api)
        {
            return $"{DateTime.Now.GetTimeStampInt()}.--{Interlocked.Increment(ref api._msgCount) - 1}";
        }

        public static string ConverFromUnicode(this string str)
        {
            return Regex.Replace(str.Replace(@"\u200e", ""), @"\\u([\da-f]{4})", m => ((char)Convert.ToInt32(m.Groups[1].Value, 16)).ToString());
        }

        public static string RegexGetString(this string str, string pattern, int retuenIndex = 1)
        {
            Regex r = new Regex(pattern, RegexOptions.None);
            return r.Match(str).Groups[retuenIndex].Value;
        }

        public static string UrlEncode(this string str)
        {
            return HttpUtility.UrlEncode(str);
        }

        public static long GetTimeStampLong(this DateTime dateTime)
        {
            return (dateTime.ToUniversalTime().Ticks - 621355968000000000) / 10000;
        }

        public static long GetTimeStampInt(this DateTime dateTime)
        {
            return GetTimeStampLong(dateTime) / 1000;
        }

        public static DateTime GetDateTime(this string timeStamp)
        {
            if (string.IsNullOrWhiteSpace(timeStamp))
            {
                return DateTime.MinValue;
            }
            var num = long.Parse(timeStamp);
            DateTime dtStart = TimeZoneInfo.ConvertTime(new DateTime(1970, 1, 1), TimeZoneInfo.Local);
            if (num > 9466560000)
            {
                TimeSpan toNow = new TimeSpan(num * 10000);
                return dtStart.Add(toNow);
            }
            else
            {
                TimeSpan toNow = new TimeSpan(num * 1000 * 10000);
                return dtStart.Add(toNow);
            }
        }

        public static byte[] HMACSHA256_Encrypt(this byte[] bs, byte[] key)
        {
            using (HMACSHA256 hmac = new HMACSHA256(key))
            {
                byte[] computedHash = hmac.ComputeHash(bs);
                return computedHash;
            }
        }

        public static byte[] SHA256_Encrypt(this byte[] bs)
        {
            HashAlgorithm iSha = new SHA256CryptoServiceProvider();
            return iSha.ComputeHash(bs);
        }

        public static bool ValueEquals(this byte[] bs, byte[] bs2)
        {
            if (bs.Length != bs.Length)
            {
                return false;
            }
            for (int i = 0; i < bs.Length; i++)
            {
                if (bs[i] != bs2[i])
                {
                    return false;
                }
            }
            return true;
        }

        public static string ToHexString(this byte[] bytes)
        {
            string hexString = string.Empty;
            if (bytes != null)
            {
                StringBuilder strB = new StringBuilder();
                for (int i = 0; i < bytes.Length; i++)
                {
                    strB.Append(bytes[i].ToString("X2"));
                }
                hexString = strB.ToString();
            }
            return hexString;
        }
        public static byte[] AesCbcDecrypt(this byte[] data, byte[] key, byte[] iv)
        {
            var rijndaelCipher = new RijndaelManaged
            {
                Mode = CipherMode.CBC,
                Padding = PaddingMode.PKCS7,
                KeySize = key.Length * 8,
                BlockSize = iv.Length * 8
            };
            rijndaelCipher.Key = key;
            rijndaelCipher.IV = iv;
            var transform = rijndaelCipher.CreateDecryptor();
            var plainText = transform.TransformFinalBlock(data, 0, data.Length);
            return plainText;
        }
        public static byte[] AesCbcEncrypt(this byte[] data, byte[] key, byte[] iv)
        {
            var rijndaelCipher = new RijndaelManaged
            {
                Mode = CipherMode.CBC,
                Padding = PaddingMode.PKCS7,
                KeySize = key.Length * 8,
                BlockSize = iv.Length * 8
            };
            rijndaelCipher.Key = key;
            rijndaelCipher.IV = iv;
            var transform = rijndaelCipher.CreateEncryptor();
            var plainText = transform.TransformFinalBlock(data, 0, data.Length);
            return plainText;
        }
        public static byte[] AesCbcDecrypt(this byte[] data, byte[] key)
        {
            return AesCbcDecrypt(data.Skip(16).ToArray(), key, data.Take(16).ToArray());
        }

        public static bool IsNullOrWhiteSpace(this string str)
        {
            return string.IsNullOrWhiteSpace(str);
        }

        public static async Task<MemoryStream> GetStream(this string url)
        {
            MemoryStream memory = new MemoryStream();
            HttpClientHandler Handler = new HttpClientHandler();
            using (var client = new HttpClient(Handler))
            {
                var message = new HttpRequestMessage(HttpMethod.Get, url)
                {
                    Version = HttpVersion.Version20,
                };
                message.Headers.Add("user-agent", "Mozilla/5.0 (MSIE 10.0; Windows NT 6.1; Trident/5.0)");
                var response = await client.SendAsync(message);
                if (response.IsSuccessStatusCode)
                {
                    await response.Content.CopyToAsync(memory);
                }
            }
            return memory;
        }
        public static async Task<MemoryStream> Post(this string url, byte[] data, Dictionary<string, string> head = null)
        {
            MemoryStream memory = new MemoryStream();
            HttpClientHandler Handler = new HttpClientHandler();
            using (var client = new HttpClient(Handler))
            {
                var message = new HttpRequestMessage(HttpMethod.Post, url)
                {
                    Version = HttpVersion.Version20,
                };
                message.Content = new ByteArrayContent(data);
                message.Headers.Add("user-agent", "Mozilla/5.0 (MSIE 10.0; Windows NT 6.1; Trident/5.0)");
                if (head != null)
                {
                    foreach (var item in head)
                    {
                        message.Headers.Add(item.Key, item.Value);
                    }
                }
                var response = await client.SendAsync(message);
                if (response.IsSuccessStatusCode)
                {
                    await response.Content.CopyToAsync(memory);
                }
            }
            return memory;
        }
        public static async Task<string> PostHtml(this string url, byte[] data, Dictionary<string, string> head = null, Encoding encoding = null)
        {
            var memory = await Post(url, data, head);
            if (encoding == null)
            {
                return Encoding.UTF8.GetString(memory.ToArray());
            }
            else
            {
                return encoding.GetString(memory.ToArray());
            }
        }
    }
}
