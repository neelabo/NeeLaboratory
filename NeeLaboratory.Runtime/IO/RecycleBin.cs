using System;
using System.Collections.Generic;
using System.Text;
using System.Runtime.InteropServices;

// TODO: SHFileOperation でなく IFileOeration を使用することが推奨されている

namespace NeeLaboratory.IO
{
    public static class RecycleBin
    {
        internal static class NativeMethods
        {
            /// <summary>
            /// Possible flags for the SHFileOperation method.
            /// </summary>
            [Flags]
            public enum FileOperationFlags : ushort
            {
                /// <summary>
                /// Do not show a dialog during the process
                /// </summary>
                FOF_SILENT = 0x0004,
                /// <summary>
                /// Do not ask the user to confirm selection
                /// </summary>
                FOF_NOCONFIRMATION = 0x0010,
                /// <summary>
                /// Delete the file to the recycle bin.  (Required flag to send a file to the bin
                /// </summary>
                FOF_ALLOWUNDO = 0x0040,
                /// <summary>
                /// Do not show the names of the files or folders that are being recycled.
                /// </summary>
                FOF_SIMPLEPROGRESS = 0x0100,
                /// <summary>
                /// Surpress errors, if any occur during the process.
                /// </summary>
                FOF_NOERRORUI = 0x0400,
                /// <summary>
                /// Warn if files are too big to fit in the recycle bin and will need
                /// to be deleted completely.
                /// </summary>
                FOF_WANTNUKEWARNING = 0x4000,
            }

            /// <summary>
            /// File Operation Function Type for SHFileOperation
            /// </summary>
            public enum FileOperationType : uint
            {
                FO_MOVE = 0x0001,
                FO_COPY = 0x0002,
                FO_DELETE = 0x0003,
                FO_RENAME = 0x0004,
            }


            /// <summary>
            /// SHFILEOPSTRUCT for SHFileOperation from COM
            /// </summary>
            [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Auto)]
            public struct SHFILEOPSTRUCT
            {
                public IntPtr hwnd;
                [MarshalAs(UnmanagedType.U4)]
                public FileOperationType wFunc;
                public string pFrom;
                public string pTo;
                public FileOperationFlags fFlags;
                [MarshalAs(UnmanagedType.Bool)]
                public bool fAnyOperationsAborted;
                public IntPtr hNameMappings;
                public string lpszProgressTitle;
            }

            [DllImport("shell32.dll", CharSet = CharSet.Auto)]
            public static extern int SHFileOperation(ref SHFILEOPSTRUCT FileOp);
        }


        /// <summary>
        /// Send file to recycle bin
        /// </summary>
        /// <param name="path">Location of directory or file to recycle</param>
        /// <param name="flags">FileOperationFlags to add in addition to FOF_ALLOWUNDO</param>
        private static bool Send(string path, NativeMethods.FileOperationFlags flags)
        {
            try
            {
                var fs = new NativeMethods.SHFILEOPSTRUCT
                {
                    wFunc = NativeMethods.FileOperationType.FO_DELETE,
                    pFrom = path + '\0' + '\0',
                    fFlags = NativeMethods.FileOperationFlags.FOF_ALLOWUNDO | flags
                };
                var result = NativeMethods.SHFileOperation(ref fs);
                return result == 0;
            }
            catch (Exception)
            {
                return false;
            }
        }

        /// <summary>
        /// Send file to recycle bin.  Display dialog, display warning if files are too big to fit (FOF_WANTNUKEWARNING)
        /// </summary>
        /// <param name="path">Location of directory or file to recycle</param>
        public static bool Send(string path)
        {
            return Send(path, NativeMethods.FileOperationFlags.FOF_NOCONFIRMATION | NativeMethods.FileOperationFlags.FOF_WANTNUKEWARNING);
        }

        /// <summary>
        /// Send file silently to recycle bin.  Surpress dialog, surpress errors, delete if too large.
        /// </summary>
        /// <param name="path">Location of directory or file to recycle</param>
        public static bool MoveToRecycleBin(string path)
        {
            return Send(path, NativeMethods.FileOperationFlags.FOF_NOCONFIRMATION | NativeMethods.FileOperationFlags.FOF_NOERRORUI | NativeMethods.FileOperationFlags.FOF_SILENT);

        }

        private static bool DeleteFile(string path, NativeMethods.FileOperationFlags flags)
        {
            try
            {
                var fs = new NativeMethods.SHFILEOPSTRUCT
                {
                    wFunc = NativeMethods.FileOperationType.FO_DELETE,
                    pFrom = path + '\0' + '\0',
                    fFlags = flags
                };
                var result = NativeMethods.SHFileOperation(ref fs);
                return result == 0;
            }
            catch (Exception)
            {
                return false;
            }
        }

        public static bool DeleteCompletelySilent(string path)
        {
            return DeleteFile(path, NativeMethods.FileOperationFlags.FOF_NOCONFIRMATION | NativeMethods.FileOperationFlags.FOF_NOERRORUI | NativeMethods.FileOperationFlags.FOF_SILENT);
        }
    }
}
