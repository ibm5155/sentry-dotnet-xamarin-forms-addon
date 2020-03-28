using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Security.Cryptography;
using System.Text;

using Android.App;
using Android.Content;
using Android.OS;
using Android.Runtime;
using Android.Views;
using Android.Widget;
using Newtonsoft.Json;
using SentryNetXamarinAddon.Models;
using SentryNetXamarinAddon.Services.Interface;

namespace sentry_dotnet_xamarin_forms_addon.Droid.Helpers
{
    public class OfflineCacheHelper : IOfflineCacheHelper
    {
        private readonly string _logFileFolder = System.Environment.GetFolderPath(System.Environment.SpecialFolder.Personal) + Java.IO.File.Separator + "CacheLog";
        public bool ClearLogCache()
        {
            var folder = _logFileFolder;
            if (!Directory.Exists(folder))
            {
                Directory.CreateDirectory(folder);
            }
            else
            {
                try
                {
                    foreach (var file in Directory.GetFiles(folder))
                    {
                        if (File.Exists(file))
                        {
                            File.Delete(file);
                        }
                    }
                }
                catch
                {
                    return false;
                }
            }
            return true;
        }

        public CacheLog GetCache(string fileName, string key)
        {
            var file = LoadFile(fileName);
            if (file != null)
            {
                //decrypt the file
                var dencrypted = DecryptStringFromBytes(file, Encoding.UTF8.GetBytes(key));
                //save again in bytes but now its decrypted
                file = Encoding.Unicode.GetBytes(dencrypted);
                try
                {

                    var obj = JsonConvert.DeserializeObject<CacheLog>(dencrypted);
                    return obj;
                }
                catch
                {
                    //Invalid file format, delete the file
                    RemoveLogCache(fileName);
                }
            }
            return null;
        }

        public List<string> GetFileNames()
        {
            var folder = _logFileFolder;
            List<string> listFolders = new List<string>();
            if (!Directory.Exists(folder))
            {
                Directory.CreateDirectory(folder);
            }
            else
            {
                var filesList = Directory.GetFiles(folder);
                foreach (var file in filesList)
                {
                    listFolders.Add(Path.GetFileName(file));
                }
            }
            return listFolders;
        }

        public bool RemoveLogCache(string fileName)
        {
            var filePath = _logFileFolder + "/" + fileName;
            if (File.Exists(filePath))
            {
                File.Delete(filePath);
                return true;
            }
            return false;
        }

        public string SaveCache(CacheLog cache, string key)
        {
            try
            {
                var serializedEc = JsonConvert.SerializeObject(cache, Formatting.Indented);
                var encrypted = EncryptStringToBytes(serializedEc, Encoding.UTF8.GetBytes(key));
                var seed = new Random();
                var filename = DateTime.Now.ToFileTime().ToString() + seed.Next(0, 10) + ".evn";
                SaveFile(filename, encrypted);
                return filename;
            }
            catch
            {

            }
            return null;
        }

        static byte[] EncryptStringToBytes(string plainText, byte[] Key)
        {
            //https://stackoverflow.com/questions/273452/using-aes-encryption-in-c-sharp
            // Check arguments.
            if (plainText == null || plainText.Length <= 0)
                throw new ArgumentNullException("plainText");
            if (Key == null || Key.Length <= 0)
                throw new ArgumentNullException("Key");
            byte[] encrypted;
            // Create an RijndaelManaged object
            // with the specified key and IV.
            using (Aes aes = Aes.Create())
            {
                aes.Key = Key;
                aes.IV = Key;

                // Create a decrytor to perform the stream transform.
                ICryptoTransform encryptor = aes.CreateEncryptor(aes.Key, aes.IV);

                // Create the streams used for encryption.
                using (MemoryStream msEncrypt = new MemoryStream())
                {
                    using (CryptoStream csEncrypt = new CryptoStream(msEncrypt, encryptor, CryptoStreamMode.Write))
                    {
                        using (StreamWriter swEncrypt = new StreamWriter(csEncrypt))
                        {

                            //Write all data to the stream.
                            swEncrypt.Write(plainText);
                        }
                        encrypted = msEncrypt.ToArray();
                    }
                }
            }
            // Return the encrypted bytes from the memory stream.
            return encrypted;

        }

        static string DecryptStringFromBytes(byte[] cipherText, byte[] Key)
        {
            // Check arguments.
            if (cipherText == null || cipherText.Length <= 0)
                throw new ArgumentNullException("cipherText");
            if (Key == null || Key.Length <= 0)
                throw new ArgumentNullException("Key");

            // Declare the string used to hold
            // the decrypted text.
            string plaintext = null;

            // Create an RijndaelManaged object
            // with the specified key and IV.
            using (Aes aes = Aes.Create())
            {
                aes.Key = Key;
                aes.IV = Key;

                // Create a decryptor to perform the stream transform.
                ICryptoTransform decryptor = aes.CreateDecryptor(aes.Key, aes.IV);

                // Create the streams used for decryption.
                using (MemoryStream msDecrypt = new MemoryStream(cipherText))
                {
                    using (CryptoStream csDecrypt = new CryptoStream(msDecrypt, decryptor, CryptoStreamMode.Read))
                    {
                        using (StreamReader srDecrypt = new StreamReader(csDecrypt))
                        {
                            // Read the decrypted bytes from the decrypting stream
                            // and place them in a string.
                            plaintext = srDecrypt.ReadToEnd();
                        }
                    }
                }
            }
            return plaintext;
        }
        private void SaveFile(string fileName, byte[] data)
        {
            fileName = _logFileFolder + "/" + fileName;
            System.IO.File.WriteAllBytes(fileName, data);
        }

        private byte[] LoadFile(string fileName)
        {
            fileName = _logFileFolder + "/" + fileName;
            if (System.IO.File.Exists(fileName))
            {
                return System.IO.File.ReadAllBytes(fileName);
            }
            return null;
        }
    }
}