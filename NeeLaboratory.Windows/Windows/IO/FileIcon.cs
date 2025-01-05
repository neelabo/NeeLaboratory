using System;
using System.Windows.Media.Imaging;
using System.Runtime.InteropServices;
using System.Collections.Generic;
using System.Windows;
using System.Linq;
using System.Diagnostics;
using System.Threading;
using System.IO;
using System.Globalization;
using NeeLaboratory.IO;

namespace NeeLaboratory.Windows.IO
{
    // from https://www.ipentec.com/document/csharp-shell-namespace-get-big-icon-from-file-path
    public static class FileIcon
    {
        internal static class NativeMethods
        {
            [DllImport("shell32.dll", CharSet = CharSet.Unicode)]
            public static extern IntPtr SHGetFileInfo(string pszPath, FILE_ATTRIBUTE dwFileAttribs, out SHFILEINFO psfi, uint cbFileInfo, SHGFI uFlags);

            [DllImport("shell32.dll", EntryPoint = "#727")]
            public static extern int SHGetImageList(int iImageList, ref Guid riid, ref IImageList ppv);

            //[DllImport("shell32.dll", EntryPoint = "#727")]
            //public static extern int SHGetImageList(SHIL iImageList, [MarshalAs(UnmanagedType.LPStruct)] Guid riid, ref IImageList ppv);

            [DllImport("shell32.dll", EntryPoint = "#727")]
            public static extern int SHGetImageList(SHIL iImageList, ref Guid riid, out IImageList ppv);

            //
            [DllImport("shell32.dll", EntryPoint = "#727")]
            public static extern int SHGetImageList(SHIL iImageList, ref Guid riid, out IntPtr ppv);

            [DllImport("comctl32.dll", SetLastError = true)]
            public static extern IntPtr ImageList_GetIcon(IntPtr himl, int i, int flags);

            // DestroyIcon関数
            [DllImport("user32.dll")]
            [return: MarshalAs(UnmanagedType.Bool)]
            public static extern bool DestroyIcon(IntPtr hIcon);

            public const int S_OK = 0;

            //SHFILEINFO
            [Flags]
            public enum SHGFI
            {
                ICON = 0x000000100,
                DISPLAYNAME = 0x000000200,
                TYPENAME = 0x000000400,
                ATTRIBUTES = 0x000000800,
                ICONLOCATION = 0x000001000,
                EXETYPE = 0x000002000,
                SYSICONINDEX = 0x000004000,
                LINKOVERLAY = 0x000008000,
                SELECTED = 0x000010000,
                ATTR_SPECIFIED = 0x000020000,
                LARGEICON = 0x000000000,
                SMALLICON = 0x000000001,
                OPENICON = 0x000000002,
                SHELLICONSIZE = 0x000000004,
                PIDL = 0x000000008,
                USEFILEATTRIBUTES = 0x000000010,
                ADDOVERLAYS = 0x000000020,
                OVERLAYINDEX = 0x000000040
            };

            [Flags]
            public enum SHIL
            {
                LARGE = 0x0000, // 32x32
                SMALL = 0x0001, // 16x16
                EXTRALARGE = 0x0002, // 48x48 maybe.
                ////SYSSMALL = 0x0003, // ?
                JUMBO = 0x0004, //256x256 maybe.
            }

            [Flags]
            public enum FILE_ATTRIBUTE
            {
                READONLY = 0x0001,
                HIDDEN = 0x0002,
                SYSTEM = 0x0004,
                DIRECTORY = 0x0010,
                ARCHIVE = 0x0020,
                ENCRYPTED = 0x0040,
                NORMAL = 0x0080,
                TEMPORARY = 0x0100,
                SPARSE_FILE = 0x0200,
                REPARSE_POINT = 0x0400,
                COMPRESSED = 0x0800,
                OFFLINE = 0x1000,
            }

            [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Unicode, Pack = 4)]
            public struct SHFILEINFO
            {
                public IntPtr hIcon;
                public int iIcon;
                public uint dwAttributes;
                [MarshalAs(UnmanagedType.ByValTStr, SizeConst = 260)]
                public string szDisplayName;
                [MarshalAs(UnmanagedType.ByValTStr, SizeConst = 80)]
                public string szTypeName;
            }

            //IMAGE LIST
            public static Guid IID_IImageList = new Guid("46EB5926-582E-4017-9FDF-E8998DAA0950");
            public static Guid IID_IImageList2 = new Guid("192B9D83-50FC-457B-90A0-2B82A8B5DAE1");


            [StructLayout(LayoutKind.Sequential)]
            public struct POINT
            {
                int x;
                int y;
            }

            [StructLayout(LayoutKind.Sequential)]
            public struct RECT
            {
                public int left, top, right, bottom;
            }

            [StructLayout(LayoutKind.Sequential)]
            public struct IMAGEINFO
            {
                public IntPtr hbmImage;
                public IntPtr hbmMask;
                public int Unused1;
                public int Unused2;
                public RECT rcImage;
            }

            [StructLayout(LayoutKind.Sequential)]
            public struct IMAGELISTDRAWPARAMS
            {
                public int cbSize;
                public IntPtr himl;
                public int i;
                public IntPtr hdcDst;
                public int x;
                public int y;
                public int cx;
                public int cy;
                public int xBitmap;    // x offest from the upperleft of bitmap
                public int yBitmap;    // y offset from the upperleft of bitmap
                public int rgbBk;
                public int rgbFg;
                public int fStyle;
                public int dwRop;
                public int fState;
                public int Frame;
                public int crEffect;
            }

            [Flags]
            public enum ImageListDrawItemConstants : int
            {
                /// <summary>
                /// Draw item normally.
                /// </summary>
                ILD_NORMAL = 0x0,
                /// <summary>
                /// Draw item transparently.
                /// </summary>
                ILD_TRANSPARENT = 0x1,
                /// <summary>
                /// Draw item blended with 25% of the specified foreground colour
                /// or the Highlight colour if no foreground colour specified.
                /// </summary>
                ILD_BLEND25 = 0x2,
                /// <summary>
                /// Draw item blended with 50% of the specified foreground colour
                /// or the Highlight colour if no foreground colour specified.
                /// </summary>
                ILD_SELECTED = 0x4,
                /// <summary>
                /// Draw the icon's mask
                /// </summary>
                ILD_MASK = 0x10,
                /// <summary>
                /// Draw the icon image without using the mask
                /// </summary>
                ILD_IMAGE = 0x20,
                /// <summary>
                /// Draw the icon using the ROP specified.
                /// </summary>
                ILD_ROP = 0x40,
                /// <summary>
                /// ?
                /// </summary>
                ILD_OVERLAYMASK = 0xF00,
                /// <summary>
                /// Preserves the alpha channel in dest. XP only.
                /// </summary>
                ILD_PRESERVEALPHA = 0x1000, // 
                /// <summary>
                /// Scale the image to cx, cy instead of clipping it.  XP only.
                /// </summary>
                ILD_SCALE = 0x2000,
                /// <summary>
                /// Scale the image to the current DPI of the display. XP only.
                /// </summary>
                ILD_DPISCALE = 0x4000
            }


            // interface COM IImageList
            [ComImportAttribute()]
            [GuidAttribute("46EB5926-582E-4017-9FDF-E8998DAA0950")]
            [InterfaceTypeAttribute(ComInterfaceType.InterfaceIsIUnknown)]
            public interface IImageList
            {
                [PreserveSig]
                int Add(IntPtr hbmImage, IntPtr hbmMask, ref int pi);

                [PreserveSig]
                int ReplaceIcon(int i, IntPtr hicon, ref int pi);

                [PreserveSig]
                int SetOverlayImage(int iImage, int iOverlay);

                [PreserveSig]
                int Replace(int i, IntPtr hbmImage, IntPtr hbmMask);

                [PreserveSig]
                int AddMasked(IntPtr hbmImage, int crMask, ref int pi);

                [PreserveSig]
                int Draw(ref IMAGELISTDRAWPARAMS pimldp);

                [PreserveSig]
                int Remove(int i);

                [PreserveSig]
                int GetIcon(int i, int flags, ref IntPtr picon);
            };
        }


        private readonly static Lock _lock = new();

        private static string _directoryTypeName = "*\\";


        public static string GetFileTypeName(string filename)
        {
            return "*" + LoosePath.GetExtension(filename);
        }

        public static string GetDirectoryTypeName()
        {
            return _directoryTypeName;
        }


        public static List<BitmapSource> GetIconCollection(string filename, bool allowJumbo)
        {
            if (filename == null) throw new ArgumentNullException(nameof(filename));

            NativeMethods.FILE_ATTRIBUTE attribute = 0;
            NativeMethods.SHGFI flags = 0;

            if (filename.Length > 0 && filename[0] == '*')
            {
                if (filename == _directoryTypeName)
                {
                    attribute = NativeMethods.FILE_ATTRIBUTE.DIRECTORY;
                    flags = NativeMethods.SHGFI.USEFILEATTRIBUTES;
                }
                else
                {
                    attribute = NativeMethods.FILE_ATTRIBUTE.NORMAL;
                    flags = NativeMethods.SHGFI.USEFILEATTRIBUTES;
                }
            }
            else
            {
                if (allowJumbo && (filename.EndsWith(":", StringComparison.Ordinal) || filename.EndsWith(":\\", StringComparison.Ordinal)))
                {
                    flags = NativeMethods.SHGFI.ICONLOCATION;
                }
            }

            return CreateFileIconCollection(filename, attribute, flags, allowJumbo);
        }


        private static List<BitmapSource> CreateFileIconCollection(string filename, NativeMethods.FILE_ATTRIBUTE attribute, NativeMethods.SHGFI flags, bool allowJumbo)
        {
            if (allowJumbo)
            {
                return GetFileIconCollectionExtra(filename, attribute, flags);
            }
            else
            {
                return GetFileIconCollectionNormal(filename, attribute, flags);
            }
        }

        private static List<BitmapSource> GetFileIconCollectionNormal(string filename, NativeMethods.FILE_ATTRIBUTE attribute, NativeMethods.SHGFI flags)
        {
            var bitmaps = new List<BitmapSource>();

            var smallIcon = GetFileIcon(filename, attribute, flags | NativeMethods.SHGFI.SMALLICON);
            if (smallIcon != null) bitmaps.Add(smallIcon);

            var largeIcon = GetFileIcon(filename, attribute, flags | NativeMethods.SHGFI.LARGEICON);
            if (largeIcon != null) bitmaps.Add(largeIcon);

            return bitmaps;
        }

        private static List<BitmapSource> GetFileIconCollectionFromIconFile(string filename)
        {
            using (var imageFileStrm = new FileStream(filename, FileMode.Open, FileAccess.Read, FileShare.Read))
            {
                var decoder = BitmapDecoder.Create(imageFileStrm, BitmapCreateOptions.None, BitmapCacheOption.OnLoad);
                var bitmaps = decoder.Frames.Cast<BitmapSource>().ToList();
                bitmaps.ForEach(e => e.Freeze());
                return bitmaps;
            }
        }

        private static List<BitmapSource> GetFileIconCollectionExtra(string filename, NativeMethods.FILE_ATTRIBUTE attribute, NativeMethods.SHGFI flags)
        {
            Debug.Assert(Thread.CurrentThread.GetApartmentState() == ApartmentState.STA);

            ////var sw = Stopwatch.StartNew();
            lock (_lock)
            {
                NativeMethods.SHFILEINFO shinfo = new NativeMethods.SHFILEINFO();
                shinfo.szDisplayName = string.Empty;
                shinfo.szTypeName = string.Empty;
                IntPtr hImg = NativeMethods.SHGetFileInfo(filename, attribute, out shinfo, (uint)Marshal.SizeOf(typeof(NativeMethods.SHFILEINFO)), NativeMethods.SHGFI.SYSICONINDEX | flags);

                try
                {
                    if ((flags & NativeMethods.SHGFI.ICONLOCATION) != 0 && LoosePath.GetExtension(shinfo.szDisplayName) == ".ico")
                    {
                        return GetFileIconCollectionFromIconFile(shinfo.szDisplayName);
                    }
                }
                catch (Exception ex)
                {
                    Debug.WriteLine(ex.Message);
                }

                var bitmaps = new List<BitmapSource>();

                var shils = Enum.GetValues(typeof(NativeMethods.SHIL)).Cast<NativeMethods.SHIL>();
                foreach (var shil in shils)
                {
                    try
                    {
                        int hResult = NativeMethods.SHGetImageList(shil, ref NativeMethods.IID_IImageList, out NativeMethods.IImageList imglist);
                        Marshal.ThrowExceptionForHR(hResult);

                        IntPtr hicon = IntPtr.Zero;
                        
                        hResult = imglist.GetIcon(shinfo.iIcon, (int)NativeMethods.ImageListDrawItemConstants.ILD_TRANSPARENT, ref hicon);
                        Marshal.ThrowExceptionForHR(hResult);

                        BitmapSource bitmapSource = System.Windows.Interop.Imaging.CreateBitmapSourceFromHIcon(hicon, System.Windows.Int32Rect.Empty, BitmapSizeOptions.FromEmptyOptions());
                        bitmapSource?.Freeze();
                        NativeMethods.DestroyIcon(hicon);
                        ////Debug.WriteLine($"Icon: {filename} - {shil}: {sw.ElapsedMilliseconds}ms");
                        if (bitmapSource != null)
                        {
                            bitmaps.Add(bitmapSource);
                        }
                    }
                    catch (Exception ex)
                    {
                        Debug.WriteLine($"Icon: {filename} - {shil}: {ex.Message}");
                        throw;
                    }
                }
                return bitmaps;
            }
        }

        private static BitmapSource? GetFileIcon(string filename, NativeMethods.FILE_ATTRIBUTE attribute, NativeMethods.SHGFI flags)
        {
            Debug.Assert(Thread.CurrentThread.GetApartmentState() == ApartmentState.STA);

            ////var sw = Stopwatch.StartNew();
            lock (_lock)
            {
                NativeMethods.SHFILEINFO shinfo = new NativeMethods.SHFILEINFO();
                IntPtr hSuccess = NativeMethods.SHGetFileInfo(filename, attribute, out shinfo, (uint)Marshal.SizeOf(shinfo), flags | NativeMethods.SHGFI.ICON);
                if (hSuccess != IntPtr.Zero && shinfo.hIcon != IntPtr.Zero)
                {
                    BitmapSource bitmapSource = System.Windows.Interop.Imaging.CreateBitmapSourceFromHIcon(shinfo.hIcon, System.Windows.Int32Rect.Empty, BitmapSizeOptions.FromEmptyOptions());
                    bitmapSource?.Freeze();
                    NativeMethods.DestroyIcon(shinfo.hIcon);
                    ////Debug.WriteLine($"Icon: {filename} - {iconSize}: {sw.ElapsedMilliseconds}ms");
                    return bitmapSource;
                }
                else
                {
                    Debug.WriteLine($"Icon: {filename}: Cannot created!!");
                    throw new ApplicationException("Cannot create file icon.");
                }
            }
        }
    }
}