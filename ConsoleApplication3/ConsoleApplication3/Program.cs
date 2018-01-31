using System;
using System.Collections.Generic;
using System.Text;
using System.Net;
using Microsoft.Win32;
using System.Security.Authentication;
using System.Security.Cryptography;

namespace ConsoleApplication3
{
    class Program
    {
        static void Main(string[] args)
        {
            string first = "GradingRecordExporter";
            string second = "GradingRecordImporter";
            Console.WriteLine(first);
            CredentialsManager.GetNetworkCredentials(first);
            Console.WriteLine(second);
            CredentialsManager.GetNetworkCredentials(second);

        }
    }

    public static class CredentialsManager
    {
        const string ValueName = "data";
        const string KeyFormat = "SOFTWARE\\Microsoft\\MSTCS\\GP\\Role\\{0}";
        const char Seperator = '\u0000';
        static byte[] AdditionalEntropy = { 9, 8, 7, 6, 5 };
        /// <summary>
        /// gets the credential for a role.
        /// </summary>
        /// <param name="roleName"></param>
        /// <returns></returns>
        static public NetworkCredential GetNetworkCredentials(string roleName)
        {
            var mykey = GetRegKeyString(roleName);
            if (string.IsNullOrEmpty(roleName))
                throw new ArgumentNullException("roleName");
            using (RegistryKey rolekey = Registry.LocalMachine.OpenSubKey(GetRegKeyString(mykey)))
            {
                if (null == rolekey)
                {
                    throw new ArgumentOutOfRangeException("Credential for the role is not set in the system");
                }
                byte[] encryptedData = (byte[])rolekey.GetValue(ValueName);
                return ConvertByteArrayToCredential(GetCredentialData(encryptedData));
            }
        }
        /// <summary>
        /// Sets the credential for a role.
        /// </summary>
        /// <param name="roleName"></param>
        /// <param name="networkCredential"></param>
        public static void SetCredential(string roleName, NetworkCredential networkCredential)
        {
            if (string.IsNullOrEmpty(roleName))
            {
                throw new ArgumentNullException("roleName");
            }
            if (null == networkCredential)
            {
                throw new ArgumentNullException("networkCredential");
            }

            string regkey = GetRegKeyString(roleName);
            RegistryKey rolekey = null;
            try
            {
                rolekey = Registry.LocalMachine.OpenSubKey(regkey, true);
                {
                    if (null == rolekey)
                    {
                        //create the key
                        rolekey = Registry.LocalMachine.CreateSubKey(regkey, RegistryKeyPermissionCheck.ReadWriteSubTree);
                    }
                    rolekey.SetValue(ValueName, GetProtectedData(ConvertCredentialToByteArray(networkCredential)), RegistryValueKind.Binary);
                }
            }
            finally
            {
                if (null != rolekey)
                {
                    rolekey.Close();
                }
            }
        }
        private static string GetRegKeyString(string roleName)
        {
            return string.Format(KeyFormat, roleName);
        }
        private static byte[] ConvertCredentialToByteArray(NetworkCredential networkCredential)
        {
            string value = string.Format("{1}{0}{2}", Seperator, networkCredential.UserName, networkCredential.Password);
            return System.Text.Encoding.Unicode.GetBytes(value);
        }
        private static NetworkCredential ConvertByteArrayToCredential(byte[] credential)
        {
            string value = System.Text.Encoding.Unicode.GetString(credential);
            string[] parts = value.Split(new char[] { Seperator });
            if (2 != parts.Length)
            {
                throw new IndexOutOfRangeException("Invalid data");
            }
            NetworkCredential credentinal = new NetworkCredential();
            Console.WriteLine("user: {0}\npw: {1}", parts[0], parts[1]);
            credentinal.UserName = parts[0];
            credentinal.Password = parts[1];
            return credentinal;
        }
        public static byte[] GetProtectedData(byte[] credential)
        {
            return ProtectedData.Protect(credential, AdditionalEntropy, DataProtectionScope.LocalMachine);
        }
        private static byte[] GetCredentialData(byte[] encryptedData)
        {
            return ProtectedData.Unprotect(encryptedData, AdditionalEntropy, DataProtectionScope.LocalMachine);
        }
    }
}
