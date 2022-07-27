using MyDrive.Base.Win32;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Interop;
using System.Windows.Media.Imaging;
using static MyDrive.Base.Win32.Shell32;

namespace MyDrive.Base
{
    public class FolderUtils
    {
        private Dictionary<int, BitmapSource> _existIcons;

        public FolderUtils()
        {
            _existIcons = new Dictionary<int, BitmapSource>();
        }

        // 256*256
        public async Task<BitmapSource> GetFileOrFolderIcon(string path)
        {
            return await Task.Factory.StartNew(() =>
            {
                SHFILEINFO sfi = new SHFILEINFO();
                IntPtr hIcon = IntPtr.Zero;
                IImageList spiml = null;
                Guid guil = new Guid(IID_IImageList2);  //or IID_IImageList
                SHGetFileInfo(path, 0, ref sfi, (uint)Marshal.SizeOf(sfi), (uint)(SHGFI.SysIconIndex | SHGFI.SmallIcon));

                if (!_existIcons.ContainsKey(sfi.iIcon))
                {
                    SHGetImageList(SHIL_JUMBO, ref guil, ref spiml);
                    spiml.GetIcon(sfi.iIcon, ILD_IMAGE, ref hIcon);
                    var ic2 = Imaging.CreateBitmapSourceFromHIcon(hIcon, Int32Rect.Empty, BitmapSizeOptions.FromEmptyOptions());
                    ic2.Freeze();
                    _existIcons.Add(sfi.iIcon, ic2);
                    return ic2;

                }
                else
                    return _existIcons[sfi.iIcon];
            });
        }
    }
}
