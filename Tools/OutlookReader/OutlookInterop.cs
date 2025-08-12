using System;
using System.Diagnostics;
using System.Runtime.InteropServices;
using System.Threading.Tasks;
using Outlook = Microsoft.Office.Interop.Outlook;

namespace OutlookReader
{
    public static class OutlookInterop
    {
        private static Outlook.Application GetOutlookApplication()
        {
            if (Process.GetProcessesByName("OUTLOOK").Length == 0)
                throw new InvalidOperationException("Outlook is not running. Please open Outlook first.");

            return new Outlook.Application();
        }

        private static Outlook.MAPIFolder GetFolder(Outlook.NameSpace outlookNamespace, string rootFolderName, string? subFolderName = null)
        {
            if (string.IsNullOrWhiteSpace(rootFolderName))
                throw new ArgumentException("Root folder name cannot be null or empty.", nameof(rootFolderName));

            Outlook.MAPIFolder rootFolder = null;

            if (rootFolderName.Equals("Inbox", StringComparison.OrdinalIgnoreCase))
            {
                rootFolder = outlookNamespace.GetDefaultFolder(Outlook.OlDefaultFolders.olFolderInbox);
            }
            else
            {
                var mainFolder = outlookNamespace.Folders.GetFirst(); // Assuming the first folder is the default one
                foreach (Outlook.MAPIFolder folder in mainFolder.Folders)
                {
                    if (folder.Name.Equals(rootFolderName, StringComparison.OrdinalIgnoreCase))
                    {
                        rootFolder = folder;
                        break;
                    }
                    Marshal.ReleaseComObject(folder);
                }
            }

            if (rootFolder == null)
                throw new InvalidOperationException($"Root folder '{rootFolderName}' not found.");

            if (!string.IsNullOrWhiteSpace(subFolderName))
            {
                foreach (Outlook.MAPIFolder folder in rootFolder.Folders)
                {
                    if (folder.Name.Equals(subFolderName, StringComparison.OrdinalIgnoreCase))
                    {
                        Marshal.ReleaseComObject(rootFolder);
                        return folder;
                    }
                    Marshal.ReleaseComObject(folder);
                }

                throw new InvalidOperationException($"Subfolder '{subFolderName}' not found in '{rootFolderName}'.");
            }

            return rootFolder;
        }

        public static async Task<Outlook.MailItem?> WaitForNewMailAsync(string subjectKeyword, string rootFolderName, string? subFolderName = null)
        {
            Outlook.Application outlookApp = GetOutlookApplication();
            Outlook.NameSpace ns = outlookApp.Session;

            Outlook.MAPIFolder folder = GetFolder(ns, rootFolderName, subFolderName);

            try
            {
                Console.WriteLine($"Monitoring folder: {folder.Name}");

                DateTime startTime = DateTime.Now;

                while (true)
                {
                    Outlook.Items items = folder.Items;
                    items.Sort("[ReceivedTime]", true);

                    foreach (object item in items)
                    {
                        if (item is Outlook.MailItem mail)
                        {
                            if (mail.ReceivedTime < startTime)
                                break;

                            if (mail.UnRead && mail.Subject.Contains(subjectKeyword, StringComparison.OrdinalIgnoreCase))
                            {
                                Console.WriteLine($"New mail: {mail.Subject} - {mail.ReceivedTime}");
                                return mail;
                            }

                            Marshal.ReleaseComObject(mail);
                        }
                    }

                    Marshal.ReleaseComObject(items);

                    await Task.Delay(5000);
                }
            }
            finally
            {
                Marshal.ReleaseComObject(folder);
                Marshal.ReleaseComObject(ns);
            }
        }
    }
}
