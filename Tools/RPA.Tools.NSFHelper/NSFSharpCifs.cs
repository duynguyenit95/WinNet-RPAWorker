using System;
using System.ComponentModel;
using System.Net;
using System.Runtime.InteropServices;
using SharpCifs.Smb;
using System.IO;
namespace RPA.Tools.NSFHelper
{
    public class SmbActionResult
    {
        public bool IsSuccess { get; set; } = true;
        public string Message { get; set; } = "OK";
        public string TargetPath { get; set; }
        public static SmbActionResult Success(string targetPath)
        {
            return new SmbActionResult() { 
                TargetPath = targetPath
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
        public static SmbActionResult NSFCopyFile(NtlmPasswordAuthentication auth
                        , string sourcePath, string targetPath)
        {

            // Validate source
            var source = new SmbFile(sourcePath, auth);
            if (!source.Exists()) return new SmbActionResult()
            {
                IsSuccess = true,
                Message = "Source file doesn't exist"
            };

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
        public static SmbActionResult LocalCopyFile(NtlmPasswordAuthentication auth
                        , string sourcePath, string targetPath)
        {
            // Validate source
            var source = new SmbFile(sourcePath, auth);
            if (!source.Exists()) return new SmbActionResult()
            {
                IsSuccess = true,
                Message = "Source file doesn't exist"
            };

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
    }
}