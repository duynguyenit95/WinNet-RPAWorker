using System;
using System.ComponentModel;
using System.Net;
using System.Runtime.InteropServices;
using SharpCifs.Smb;
using System.IO;
namespace RPA.Tools
{
    public class SmbActionResult
    {
        public bool IsSuccess { get; set; } = true;
        public string Message { get; set; } = "OK";
        public string TargetPath { get; set; }
        public static SmbActionResult Success(string targetPath)
        {
            return new SmbActionResult()
            {
                TargetPath = targetPath
            };
        }
        public static SmbActionResult SourceFileDoesNotExist()
        {
            return new SmbActionResult()
            {
                IsSuccess = false,
                Message = "Source file doesn't exist"
            };
        }
    }

    public static class SmbExtensions
    {
        /// <summary>
        /// Return File Modified Date in Local Date Time
        /// </summary>
        /// <param name="smbFile"></param>
        /// <returns></returns>
        public static DateTime GetLocalModifiedDate(this SmbFile smbFile)
        {
            return new DateTime(1970, 1, 1, 0, 0, 0, DateTimeKind.Utc)
                                .AddMilliseconds(smbFile.LastModified())
                                .ToLocalTime();
        }

        /// <summary>
        /// Copy files between Network Shared Folder.
        /// Automatically create target folder if not exists.
        /// Overwrite target if exists.
        /// </summary>
        /// <param name="auth"></param>
        /// <param name="sourcePath"></param>
        /// <param name="targetPath"></param>
        /// <returns></returns>
        public static SmbActionResult CopyNSFFileToNSF(NtlmPasswordAuthentication auth
                        , string sourcePath, string targetPath)
        {

            // Validate source
            var source = new SmbFile(sourcePath, auth);
            if (!source.Exists()) return SmbActionResult.SourceFileDoesNotExist();

            var target = new SmbFile(targetPath, auth);

            // Validate Parent Folder. Create New If Not Exists
            var targetParentDir = new SmbFile(target.GetParent(), auth);
            if (!targetParentDir.Exists())
            {
                targetParentDir.Mkdir();
            }

            // Copy File using Stream
            source.CopyTo(target);

            return SmbActionResult.Success(targetPath);
        }
        /// <summary>
        /// Copy file to local path.
        /// Automatically create target folder if not exist.
        /// Overwrite target if exists.
        /// </summary>
        /// <param name="auth"></param>
        /// <param name="sourcePath"></param>
        /// <param name="targetPath"></param>
        /// <returns></returns>
        public static SmbActionResult CopyNSFFileToLocal(NtlmPasswordAuthentication auth
                        , string sourcePath, string targetPath)
        {
            // Validate source
            var source = new SmbFile(sourcePath, auth);
            if (!source.Exists()) return SmbActionResult.SourceFileDoesNotExist();

            var targetParentDir = Path.GetDirectoryName(targetPath);
            // Validate Parent Folder. Create New If Not Exists
            if (!Directory.Exists(targetParentDir))
            {
                Directory.CreateDirectory(targetParentDir);
            }

            using (var fileStream = File.Create(targetPath))
            {
                var sourceStream = source.GetInputStream();
                ((Stream)sourceStream).CopyTo(fileStream);
                sourceStream.Dispose();
            }

            return SmbActionResult.Success(targetPath);
        }
        public static SmbActionResult CopyLocalFileToNSF(NtlmPasswordAuthentication auth
                        , string sourcePath, string targetPath)
        {
            // Validate source
            if (!File.Exists(sourcePath)) return SmbActionResult.SourceFileDoesNotExist();

            var target = new SmbFile(targetPath, auth);
            if (!target.Exists())
            {
                target.CreateNewFile();
            }

            var saveFileStream = target.GetOutputStream();
            saveFileStream.Write(File.ReadAllBytes(sourcePath));
            saveFileStream.Dispose();

            return SmbActionResult.Success(targetPath);
        }
        public static SmbActionResult CopyLocalFolderToNSF(NtlmPasswordAuthentication auth
                        , string sourceFolderPath, string targetFolderPath)
        {
            // Validate source
            if (!Directory.Exists(sourceFolderPath)) return SmbActionResult.SourceFileDoesNotExist();

            var targetFolder = new SmbFile(targetFolderPath, auth);
            if (!targetFolder.Exists())
            {
                targetFolder.Mkdirs();
            }

            var dirInfor = new DirectoryInfo(sourceFolderPath);
            foreach (var childDir in dirInfor.GetDirectories())
            {
                CopyLocalFolderToNSF(auth, childDir.FullName, targetFolderPath + childDir.Name + "/");
            }

            foreach (var file in dirInfor.GetFiles())
            {
                CopyLocalFileToNSF(auth, file.FullName, targetFolderPath + file.Name);
            }

            return SmbActionResult.Success(targetFolderPath);
        }
        public static SmbActionResult CopyNSFFolderToLocal(NtlmPasswordAuthentication auth
                , string sourceFolderPath, string targetFolderPath)
        {
            // Validate source

            var sourceFolder = new SmbFile(sourceFolderPath, auth);
            if (!sourceFolder.Exists()) return SmbActionResult.SourceFileDoesNotExist();

            if (!Directory.Exists(targetFolderPath))
            {
                Directory.CreateDirectory(targetFolderPath);
            }

            foreach (var file in sourceFolder.ListFiles())
            {
                if (file.IsDirectory())
                {
                    CopyNSFFolderToLocal(auth, file.GetPath(), Path.Combine(targetFolderPath, file.GetName()));
                }
                else
                {
                    CopyNSFFileToLocal(auth, file.GetPath(), Path.Combine(targetFolderPath, file.GetName()));
                }
            }

            return SmbActionResult.Success(targetFolderPath);
        }

        public static SmbActionResult CopyNSFFolderToNSF(NtlmPasswordAuthentication auth
                , string sourceFolderPath, string targetFolderPath)
        {
            // Validate source

            var sourceFolder = new SmbFile(sourceFolderPath, auth);
            if (!sourceFolder.Exists()) return SmbActionResult.SourceFileDoesNotExist();

            var targetFolder = new SmbFile(targetFolderPath, auth);
            if (!targetFolder.Exists())
            {
                targetFolder.Mkdirs();
            }

            foreach (var file in sourceFolder.ListFiles())
            {
                if (file.IsDirectory())
                {
                    CopyNSFFolderToNSF(auth, file.GetPath(), targetFolderPath +  file.GetName() + "/");
                }
                else
                {
                    CopyNSFFileToNSF(auth, file.GetPath(), targetFolderPath +  file.GetName());
                }
            }

            return SmbActionResult.Success(targetFolderPath);
        }

        public static SmbActionResult DeleteAllFilesInNSF(NtlmPasswordAuthentication auth
            ,string sourceFolderPath)
        {
            var sourceFolder = new SmbFile(sourceFolderPath, auth);
            if (!sourceFolder.Exists()) return SmbActionResult.SourceFileDoesNotExist();
            foreach(var file in sourceFolder.ListFiles())
            {
                file.Delete();
            }
            return SmbActionResult.Success(sourceFolderPath);
        }

        public static SmbActionResult DeleteAllFilesLocal(string path)
        {
            DirectoryInfo dir = new DirectoryInfo(path);

            foreach (FileInfo fi in dir.GetFiles())
            {
                fi.Delete();
            }

            foreach (DirectoryInfo di in dir.GetDirectories())
            {
                DeleteAllFilesLocal(di.FullName);
                di.Delete();
            }
            return SmbActionResult.Success(path);
        }

        public static void CopyLocalFolder(string sourcePath, string targetPath)
        {
            //Now Create all of the directories
            foreach (string dirPath in Directory.GetDirectories(sourcePath, "*", SearchOption.AllDirectories))
            {
                Directory.CreateDirectory(dirPath.Replace(sourcePath, targetPath));
            }

            //Copy all the files & Replaces any files with the same name
            foreach (string newPath in Directory.GetFiles(sourcePath, "*.*", SearchOption.AllDirectories))
            {
                File.Copy(newPath, newPath.Replace(sourcePath, targetPath), true);
            }
        }

        public static void CopyLocalFilesRecursively(string sourcePath, string targetPath)
        {
            //Now Create all of the directories
            foreach (string dirPath in Directory.GetDirectories(sourcePath, "*", SearchOption.AllDirectories))
            {
                Directory.CreateDirectory(dirPath.Replace(sourcePath, targetPath));
            }

            //Copy all the files & Replaces any files with the same name
            foreach (string newPath in Directory.GetFiles(sourcePath, "*.*", SearchOption.AllDirectories))
            {
                File.Copy(newPath, newPath.Replace(sourcePath, targetPath), true);
            }
        }

        public static string RemoveInvalidPathChars(this string source)
        {
            foreach (var c in Path.GetInvalidFileNameChars())
            {
                source = source.Replace(c, '-');
            }
            return source;
        }
    }
}