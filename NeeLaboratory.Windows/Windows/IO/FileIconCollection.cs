using NeeLaboratory.Windows.Media.Imaging;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Media.Imaging;


namespace NeeLaboratory.Windows.IO
{
    public class FileIconCollection
    {
        private bool _allowJumbo;
        private Dictionary<string, BitmapSourceCollection> _caches = new Dictionary<string, BitmapSourceCollection>();

        public FileIconCollection(bool allowJumbo)
        {
            _allowJumbo = allowJumbo;
        }


        public BitmapSource? GetDefaultFileIcon(double width)
        {
            return GetFileIcon(FileIcon.GetFileTypeName(".*"), width, true);
        }

        public BitmapSource? GetDefaultFolderIcon(double width)
        {
            return GetFileIcon(FileIcon.GetDirectoryTypeName(), width, true);
        }

        public BitmapSource? GetFileIcon(string filename, double width, bool useCache)
        {
            var collection = GetFileIconCollection(filename, useCache);
            return collection?.GetBitmapSource(width);
        }

        private BitmapSourceCollection GetFileIconCollection(string filename, bool useCache)
        {
            if (useCache && _caches.TryGetValue(filename, out BitmapSourceCollection? collection))
            {
                return collection;
            }

            ////var sw = Stopwatch.StartNew();
            try
            {
                collection = new BitmapSourceCollection(FileIcon.GetIconCollection(filename, _allowJumbo));
                _caches[filename] = collection;
                return collection;
            }
            catch (Exception ex)
            {
                Debug.WriteLine(ex);
                return new BitmapSourceCollection();
            }
            finally
            {
                ////sw.Stop();
                ////Debug.WriteLine($"FileIcon: {filename}: {sw.ElapsedMilliseconds}ms");
            }

        }
    }
}
