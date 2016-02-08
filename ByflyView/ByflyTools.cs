using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using xNet.Net;
using xNet.Text;
using xNet.Collections;
using System.Diagnostics;
using System.Threading;
using NWTweak;
using System.Security.Cryptography;
using System.IO;

namespace CoffeeJelly.Byfly.ByflyView
{
    internal static class ByflyTools
    {
        internal delegate bool FirstDelegate();

        /// <summary>
        /// Считает время ожидания ответа на запрос к http серверу
        /// </summary>
        /// <param name="address">Url адрес</param>
        /// <param name="maxRequestTime">Максимальное время ожидания ответа. -1 для бесконечного времени ожидания.</param>
        /// <returns>Время запроса в мс</returns>

        internal static int GetRequestTime(string address, RequestParams requestParams, int maxRequestTime, out Exception outEx)
        {
            int result = -1;
            Exception innerEx = null;
            using (var request = new HttpRequest())
            {
                request.UserAgent = HttpHelper.ChromeUserAgent();
                EventWaitHandle wh = new AutoResetEvent(false);
                var requestThread = new Thread(() =>
                {
                    var watch = new Stopwatch();
                    watch.Start();
                    try
                    {
                        string resultPage = request.Get(address, requestParams).ToString();
                    }
                    catch (Exception ex) { innerEx = ex; }
                    result = Convert.ToInt32(watch.ElapsedMilliseconds);
                    watch.Reset();
                    wh.Set();
                });
                requestThread.Start();
                var stoptimer = new System.Threading.Timer((Object state) =>
                {
                    requestThread.Abort();
                    wh.Set();
                }, null, maxRequestTime, Timeout.Infinite);
                wh.WaitOne();

                stoptimer.Dispose();
            }
            outEx = innerEx;
            return result;
        }
        private static readonly Object LockGetRequestObj = new object();
        /// <summary>
        /// Общий метод для разных потоков на Get запрос с функцией дозированных запросов раз в  <paramref name="delay"/> мс и временем ожидания <paramref name="waitingTime"/>, в случае неудачного запроса
        /// </summary>
        /// <param name="request">Объект HttpRequest вызывающего потока</param>
        /// <param name="url">URL строка для  Get  запроса</param>
        /// <param name="delay">Время задержки между запросами в мс</param>
        /// <param name="waitingTime">Время ожидания не ошибочного ответа от сервера</param>
        /// <param name="setter">делегат передающий  сигналу задержки состояние true, означающий недавнее выполнение http get запроса</param>
        /// <param name="getter">делегат возвращающий текущее состояние сигнала разрешения задержки</param>
        /// <returns></returns>
        internal static string GetDosingRequests(HttpRequest request, string url, RequestParams rParams, int delay, int waitingTime, Action<bool> setter, FirstDelegate getter)
        {
            string resultPage = "";

            Stopwatch waitingWatch = new Stopwatch();
            waitingWatch.Start();
            while (String.IsNullOrEmpty(resultPage))
            {

                if (getter())
                {
                    //этой конструкцией выстраиваю потоки в очередь
                    lock (LockGetRequestObj)
                    {
                        Thread.Sleep(delay);
                    }
                }
                try
                {
                    setter(true);
                    resultPage = request.Get(url, rParams).ToString();
                    Console.WriteLine("request");
                }
                catch (Exception ex)
                {
                    if (waitingWatch.ElapsedMilliseconds > waitingTime && waitingTime != -1)
                        throw new Exception("Время ожидания вышло. Причина: \r\n" + ex.Message);
                    Thread.Sleep(5000);
                }
            }
            waitingWatch.Reset();
            return resultPage;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="keyValue"></param>
        /// <param name="valueName"></param>
        /// <param name="keyLocation"></param>
        /// <param name="defaulttValue"></param>
        internal static void CheckRegistrySettings(ref string keyValue, string valueName, string keyLocation, string defaulttValue)
        {
            if (RegistryWorker.GetKeyValue<string>(Microsoft.Win32.RegistryHive.LocalMachine, keyLocation, valueName) != null)
                keyValue = RegistryWorker.GetKeyValue<string>(Microsoft.Win32.RegistryHive.LocalMachine, keyLocation, valueName);
            else if (RegistryWorker.WriteKeyValue(Microsoft.Win32.RegistryHive.LocalMachine, keyLocation, Microsoft.Win32.RegistryValueKind.String, valueName, defaulttValue))
                keyValue = defaulttValue;
            else
                throw new Exception("UNABLE TO USE REGKEY HKEY_LOCAL_MACHINE\\" + keyLocation + "\\" + valueName + " PLEASE TRY TO RUN PROGRAM WITH ADMIN RIGHTS");
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="lastValue"></param>
        /// <param name="valueName"></param>
        /// <param name="keyLocation"></param>
        /// <param name="saveValue"></param>
        internal static void SaveRegistrySettings(string valueName, string keyLocation, string saveValue)
        {
            if (RegistryWorker.WriteKeyValue(Microsoft.Win32.RegistryHive.LocalMachine, keyLocation, Microsoft.Win32.RegistryValueKind.String, valueName, saveValue))
                return;
            else
                throw new Exception("UNABLE TO USE REGKEY HKEY_LOCAL_MACHINE\\" + keyLocation + "\\" + valueName + " PLEASE TRY TO RUN PROGRAM WITH ADMIN RIGHTS");
        }

        /// <summary>
        /// Шифрует строку <paramref name="s"/> по алгоритму MD5
        /// </summary>
        /// <param name="s">Строка, которую необходимо зашифровать</param>
        /// <returns>Хеш-код</returns>
        public static string GetHashString(string s)
        {
            //переводим строку в байт-массим
            byte[] bytes = Encoding.Unicode.GetBytes(s);

            //создаем объект для получения средст шифрования
            MD5CryptoServiceProvider CSP =
                new MD5CryptoServiceProvider();

            //вычисляем хеш-представление в байтах
            byte[] byteHash = CSP.ComputeHash(bytes);

            string hash = string.Empty;

            //формируем одну цельную строку из массива
            foreach (byte b in byteHash)
                hash += string.Format("{0:x2}", b);

            return hash;
        }

        /// <summary>
        /// Шифрует строку <paramref name="s"/> по алгоритму MD5
        /// </summary>
        /// <param name="s">Строка, которую необходимо зашифровать</param>
        /// <returns>Хеш-код</returns>
        public static Guid GetHashGuid(string s)
        {
            //переводим строку в байт-массим
            byte[] bytes = Encoding.Unicode.GetBytes(s);

            //создаем объект для получения средст шифрования
            MD5CryptoServiceProvider CSP =
                new MD5CryptoServiceProvider();

            //вычисляем хеш-представление в байтах
            byte[] byteHash = CSP.ComputeHash(bytes);

            string hash = string.Empty;

            //формируем одну цельную строку из массива
            foreach (byte b in byteHash)
                hash += string.Format("{0:x2}", b);

            return new Guid(hash);
        }

        //http://stackoverflow.com/questions/1678555/password-encryption-decryption-code-in-net
        internal static string EncryptText(string openText, string c_key, string c_iv)
        {
            RC2CryptoServiceProvider rc2CSP = new RC2CryptoServiceProvider();
            ICryptoTransform encryptor = rc2CSP.CreateEncryptor(Convert.FromBase64String(c_key), Convert.FromBase64String(c_iv));
            using (MemoryStream msEncrypt = new MemoryStream())
            {
                using (CryptoStream csEncrypt = new CryptoStream(msEncrypt, encryptor, CryptoStreamMode.Write))
                {
                    byte[] toEncrypt = Encoding.Unicode.GetBytes(openText);

                    csEncrypt.Write(toEncrypt, 0, toEncrypt.Length);
                    csEncrypt.FlushFinalBlock();

                    byte[] encrypted = msEncrypt.ToArray();

                    return Convert.ToBase64String(encrypted);
                }
            }
        }

        internal static string DecryptText(string encryptedText, string c_key, string c_iv)
        {
            RC2CryptoServiceProvider rc2CSP = new RC2CryptoServiceProvider();
            ICryptoTransform decryptor = rc2CSP.CreateDecryptor(Convert.FromBase64String(c_key), Convert.FromBase64String(c_iv));
            using (MemoryStream msDecrypt = new MemoryStream(Convert.FromBase64String(encryptedText)))
            {
                using (CryptoStream csDecrypt = new CryptoStream(msDecrypt, decryptor, CryptoStreamMode.Read))
                {
                    List<Byte> bytes = new List<byte>();
                    int b;
                    do
                    {
                        b = csDecrypt.ReadByte();
                        if (b != -1)
                            bytes.Add(Convert.ToByte(b));

                    }
                    while (b != -1);

                    return Encoding.Unicode.GetString(bytes.ToArray());
                }
            }
        }

        /// <summary>
        /// Генерирует случайный ключ и вектор инициализации и возвращает их в паре имя-ключ
        /// </summary>
        /// <returns></returns>
        internal static KeyValuePair<byte[], byte[]> GenerateKeyIV()
        {
            KeyValuePair<byte[], byte[]> keyiv;

            using (RijndaelManaged rm = new RijndaelManaged())
            {
                rm.GenerateKey();
                rm.GenerateIV();
                keyiv = new KeyValuePair<byte[], byte[]>(rm.Key, rm.IV);
            }
            return keyiv;
        }

    }
    internal static partial class ByFlyConstants
    {
        internal const string _BELTELECOM_URL = @"https://issa.beltelecom.by/main.html";
        internal const string _SETTINGS_LOCATION = @"SOFTWARE\ByflyView";
        internal const string _PROFILES_LOCATION = @"Profiles";
        internal const string _LOGIN = @"lok'tar";
        internal const string _PASSWORD = @"psw";
        internal const string _PROFILESLIST = "List";
    }
}
