using System;
using System.Linq;
using System.Collections.Generic;
using System.IO;
using System.Windows;
using System.Runtime.InteropServices;
using System.Runtime.InteropServices.ComTypes;

namespace IMP.Shared
{
    /// <summary>
    /// Helper class with Windows Clipboard functions
    /// </summary>
    internal static class ClipboardUtils
    {
        #region NativeMethods class
        private static class NativeMethods
        {
            public const string CFSTR_FILEDESCRIPTORW = "FileGroupDescriptorW";

            [Flags]
            public enum FD : uint
            {
                FD_ACCESSTIME = 0x10,
                FD_ATTRIBUTES = 4,
                FD_CLSID = 1,
                FD_CREATETIME = 8,
                FD_FILESIZE = 0x40,
                FD_LINKUI = 0x8000,
                FD_SIZEPOINT = 2,
                FD_WRITESTIME = 0x20
            }

            [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Unicode)]
            public struct FILEDESCRIPTOR    //Unicode FILEDESCRIPTORW
            {
                public FD dwFlags;
                public Guid clsid;
                public System.Drawing.Size sizel;
                public System.Drawing.Point pointl;
                public UInt32 dwFileAttributes;
                public System.Runtime.InteropServices.ComTypes.FILETIME ftCreationTime;
                public System.Runtime.InteropServices.ComTypes.FILETIME ftLastAccessTime;
                public System.Runtime.InteropServices.ComTypes.FILETIME ftLastWriteTime;
                public UInt32 nFileSizeHigh;
                public UInt32 nFileSizeLow;
                [MarshalAs(UnmanagedType.ByValTStr, SizeConst = 260)]
                public String cFileName;
            }
        }
        #endregion

        #region action methods
        public static bool ContainsClipboardFiles()
        {
            if (Clipboard.ContainsFileDropList())
            {
                return true;
            }

            var dataObject = (DataObject)Clipboard.GetDataObject();
            return dataObject.GetDataPresent(NativeMethods.CFSTR_FILEDESCRIPTORW);    //Wide File Descriptor
        }

        public static List<ClipboardFileInfo> GetClipboardFiles()
        {
            var list = new List<ClipboardFileInfo>();

            if (Clipboard.ContainsFileDropList())
            {
                var fileDropList = Clipboard.GetFileDropList();
                if (fileDropList != null && fileDropList.Count != 0)
                {
                    foreach (string item in fileDropList)
                    {
                        var fi = new System.IO.FileInfo(item);
                        if (fi.Exists && (fi.Attributes & FileAttributes.Directory) == 0)    //Only files
                        {
                            list.Add(new ClipboardFileInfo(fi));
                        }
                    }
                }
            }
            else
            {
                var dataObject = (DataObject)Clipboard.GetDataObject();
                List<NativeMethods.FILEDESCRIPTOR> descriptors = GetFileDescriptors(dataObject);
                int fileIndex = -1;
                foreach (var descriptor in descriptors)
                {
                    fileIndex++;

                    if ((descriptor.dwFlags & NativeMethods.FD.FD_ATTRIBUTES) != 0 &&
                        ((FileAttributes)descriptor.dwFileAttributes & FileAttributes.Directory) == 0)    //Only files
                    {
                        if (descriptors.Any(o => descriptor.cFileName.StartsWith(o.cFileName + "\\")))    //File in folder
                        {
                            continue;
                        }

                        long length = ((descriptor.nFileSizeHigh << 0x20) | (descriptor.nFileSizeLow & ((long) 0xffffffffL)));
                        list.Add(new ClipboardFileInfo(dataObject, fileIndex, descriptor.cFileName, (FileAttributes)descriptor.dwFileAttributes, length));
                    }
                }
            }

            return list;
        }
        #endregion

        #region private member functions
        private static List<NativeMethods.FILEDESCRIPTOR> GetFileDescriptors(DataObject dataObject)
        {
            var list = new List<NativeMethods.FILEDESCRIPTOR>();
            if (dataObject.GetDataPresent(NativeMethods.CFSTR_FILEDESCRIPTORW))    //Wide File Descriptor
            {
                var obj2 = dataObject as System.Runtime.InteropServices.ComTypes.IDataObject;
                if (obj2 != null)
                {
                    var input = dataObject.GetData(NativeMethods.CFSTR_FILEDESCRIPTORW) as Stream;
                    if (input != null)
                    {
                        int count = new BinaryReader(input).ReadInt32();
                        for (int i = 0; i < count; i++)
                        {
                            var descriptor = (NativeMethods.FILEDESCRIPTOR)ReadStructureFromStream(input, typeof(NativeMethods.FILEDESCRIPTOR));
                            list.Add(descriptor);
                        }
                    }
                }
            }

            return list;
        }

        private static object ReadStructureFromStream(Stream source, Type structureType)
        {
            byte[] buffer = new byte[Marshal.SizeOf(structureType)];
            int readed = source.Read(buffer, 0, buffer.Length);
            if (readed == buffer.Length)
            {
                GCHandle handle = GCHandle.Alloc(buffer, GCHandleType.Pinned);
                try
                {
                    IntPtr ptr = handle.AddrOfPinnedObject();
                    return Marshal.PtrToStructure(ptr, structureType);
                }
                finally
                {
                    handle.Free();
                }
            }
            if (readed != 0)
            {
                throw new ArgumentException("source is too small to hold entire structure");
            }

            return null;
        }
        #endregion
    }

    internal class ClipboardFileInfo
    {
        #region NativeMethods class
        private static class NativeMethods
        {
            public const string CFSTR_FILECONTENTS = "FileContents";

            [DllImport("ole32.dll")]
            public static extern void ReleaseStgMedium([In] ref STGMEDIUM pmedium);
        }
        #endregion

        #region member varible and default property initialization
        private DataObject DataObject;
        private int FileIndex;

        public string FullName { get; private set; }
        public FileAttributes Attributes { get; private set; }
        public long Length { get; private set; }
        #endregion

        #region constructors and destructors
        public ClipboardFileInfo(System.IO.FileInfo fileInfo)
        {
            this.FullName = fileInfo.FullName;
            this.Attributes = fileInfo.Attributes;
            this.Length = fileInfo.Length;
        }

        public ClipboardFileInfo(DataObject dataObject, int fileIndex, string fullName, FileAttributes attributes, long length)
        {
            this.DataObject = dataObject;
            this.FileIndex = fileIndex;
            this.FullName = fullName;
            this.Attributes = attributes;
            this.Length = length;
        }
        #endregion

        #region action methods
        public Stream OpenRead()
        {
            if (this.DataObject == null) //Data from System.IO.FileInfo
            {
                return new FileStream(this.FullName, FileMode.Open, FileAccess.Read, FileShare.Read, 0x1000, false);
            }

            //Read file from clipboard FileContents
            return ReadFromDataObject(this.DataObject, this.FileIndex);
        }
        #endregion

        #region property getters/setters
        public string Name
        {
            get { return System.IO.Path.GetFileName(this.FullName); }
        }

        public string Extension
        {
            get
            {
                int length = this.FullName.Length;
                int startIndex = length;

                while (--startIndex >= 0)
                {
                    char ch = this.FullName[startIndex];
                    if (ch == '.')
                    {
                        return this.FullName.Substring(startIndex, length - startIndex);
                    }
                    if (ch == Path.DirectorySeparatorChar || ch == Path.AltDirectorySeparatorChar || ch == Path.VolumeSeparatorChar)
                    {
                        break;
                    }
                }

                return string.Empty;
            }
        }
        #endregion

        #region private member functions
        private static Stream ReadFromDataObject(DataObject dataObject, int fileIndex)
        {
            var formatetc = new FORMATETC
            {
                cfFormat = (short)DataFormats.GetDataFormat(NativeMethods.CFSTR_FILECONTENTS).Id,
                dwAspect = DVASPECT.DVASPECT_CONTENT,
                lindex = fileIndex,
                ptd = new IntPtr(0),
                tymed = TYMED.TYMED_ISTREAM
            };

            STGMEDIUM medium;
            System.Runtime.InteropServices.ComTypes.IDataObject obj2 = dataObject;
            obj2.GetData(ref formatetc, out medium);

            try
            {
                if (medium.tymed == TYMED.TYMED_ISTREAM)
                {
                    var mediumStream = (IStream)Marshal.GetTypedObjectForIUnknown(medium.unionmember, typeof(IStream));
                    Marshal.Release(medium.unionmember);

                    var streaWrapper = new ComStreamWrapper(mediumStream, FileAccess.Read, ComRelease.None);

                    streaWrapper.Closed += delegate(object sender, EventArgs e)
                    {
                        NativeMethods.ReleaseStgMedium(ref medium);
                        Marshal.FinalReleaseComObject(mediumStream);
                    };

                    return streaWrapper;
                }

                throw new NotSupportedException(string.Format("Unsupported STGMEDIUM.tymed ({0})", medium.tymed));
            }
            catch
            {
                NativeMethods.ReleaseStgMedium(ref medium);
                throw;
            }
        }
        #endregion
    }

    #region ComStreamWrapper class
    internal enum ComRelease
    {
        None,
        Single,
        Final
    }

    internal class ComStreamWrapper : Stream
    {
        private FileAccess FAccess;
        private IStream FBaseStream;
        private bool? FCanSeek;
        private long? FSize;
        private ComRelease FStreamRelease;

        public event EventHandler Closed;

        public ComStreamWrapper(IStream baseStream, FileAccess access, ComRelease release)
        {
            if (baseStream == null)
            {
                throw new ArgumentNullException("baseStream");
            }
            if (!Enum.IsDefined(typeof(FileAccess), access))
            {
                throw new ArgumentException("access");
            }
            this.FBaseStream = baseStream;
            this.FAccess = access;
            this.FStreamRelease = release;
        }

        public override void Close()
        {
            if (this.FBaseStream != null)
            {
                switch (this.FStreamRelease)
                {
                    case ComRelease.Single:
                        Marshal.ReleaseComObject(this.FBaseStream);
                        break;

                    case ComRelease.Final:
                        Marshal.FinalReleaseComObject(this.FBaseStream);
                        break;
                }
                this.FBaseStream = null;
                if (this.Closed != null)
                {
                    this.Closed(this, EventArgs.Empty);
                }
            }
        }

        public override void Flush()
        {
        }

        public override int Read(byte[] buffer, int offset, int count)
        {
            int num2;
            if (this.FBaseStream == null)
            {
                throw new ObjectDisposedException("ComStreamWrapper");
            }
            if (buffer == null)
            {
                throw new ArgumentNullException("buffer");
            }
            if (offset < 0)
            {
                throw new ArgumentOutOfRangeException("offset");
            }
            if (count < 0)
            {
                throw new ArgumentOutOfRangeException("count");
            }
            IntPtr pcbRead = Marshal.AllocHGlobal(4);
            try
            {
                if (offset == 0)
                {
                    this.FBaseStream.Read(buffer, count, pcbRead);
                    return Marshal.ReadInt32(pcbRead);
                }
                byte[] pv = new byte[count];
                this.FBaseStream.Read(pv, count, pcbRead);
                int length = Marshal.ReadInt32(pcbRead);
                Array.Copy(pv, 0, buffer, offset, length);
                num2 = length;
            }
            finally
            {
                Marshal.FreeHGlobal(pcbRead);
            }
            return num2;
        }

        public override long Seek(long offset, SeekOrigin origin)
        {
            long num;
            if (this.FBaseStream == null)
            {
                throw new ObjectDisposedException("ComStreamWrapper");
            }
            if (!Enum.IsDefined(typeof(SeekOrigin), origin))
            {
                throw new ArgumentOutOfRangeException("origin");
            }
            IntPtr plibNewPosition = Marshal.AllocHGlobal(8);
            try
            {
                this.FBaseStream.Seek(offset, (int)origin, plibNewPosition);
                num = Marshal.ReadInt64(plibNewPosition);
            }
            finally
            {
                Marshal.FreeHGlobal(plibNewPosition);
            }
            return num;
        }

        public override void SetLength(long value)
        {
            if (this.FBaseStream == null)
            {
                throw new ObjectDisposedException("ComStreamWrapper");
            }
            if (value < 0L)
            {
                throw new ArgumentOutOfRangeException();
            }
            this.FBaseStream.SetSize(value);
        }

        public override void Write(byte[] buffer, int offset, int count)
        {
            if (this.FBaseStream == null)
            {
                throw new ObjectDisposedException("ComStreamWrapper");
            }
            if (buffer == null)
            {
                throw new ArgumentNullException("buffer");
            }
            if (offset < 0)
            {
                throw new ArgumentOutOfRangeException("offset");
            }
            if (count < 0)
            {
                throw new ArgumentOutOfRangeException("count");
            }
            if (offset == 0)
            {
                this.FBaseStream.Write(buffer, count, IntPtr.Zero);
            }
            else
            {
                byte[] pv = new byte[count];
                this.FBaseStream.Write(pv, count, IntPtr.Zero);
                Array.Copy(pv, 0, buffer, offset, count);
            }
        }

        public IStream BaseStream
        {
            get
            {
                return this.FBaseStream;
            }
        }

        public override bool CanRead
        {
            get
            {
                return (this.FAccess != FileAccess.Write);
            }
        }

        public override bool CanSeek
        {
            get
            {
                if (this.FBaseStream == null)
                {
                    throw new ObjectDisposedException("ComStreamWrapper");
                }
                if (!this.FCanSeek.HasValue)
                {
                    try
                    {
                        this.Seek(0L, SeekOrigin.Current);
                        this.FCanSeek = true;
                    }
                    catch (NotImplementedException)
                    {
                        this.FCanSeek = false;
                    }
                }
                return this.FCanSeek.Value;
            }
        }

        public override bool CanWrite
        {
            get
            {
                return (this.FAccess != FileAccess.Read);
            }
        }

        public override long Length
        {
            get
            {
                if (this.FBaseStream == null)
                {
                    throw new ObjectDisposedException("ComStreamWrapper");
                }
                if (!this.CanSeek)
                {
                    throw new NotImplementedException();
                }
                if (!this.FSize.HasValue)
                {
                    try
                    {
                        System.Runtime.InteropServices.ComTypes.STATSTG statstg;
                        this.FBaseStream.Stat(out statstg, 1);
                        this.FSize = new long?(statstg.cbSize);
                    }
                    catch (SystemException exception)
                    {
                        if ((!(exception is NotImplementedException) && !(exception is NotSupportedException)) && (Marshal.GetHRForException(exception) != -2147287039))
                        {
                            throw;
                        }
                        long position = this.Position;
                        this.FSize = new long?(this.Seek(0L, SeekOrigin.End));
                        this.Position = position;
                    }
                }
                return this.FSize.Value;
            }
        }

        public override long Position
        {
            get
            {
                return this.Seek(0L, SeekOrigin.Current);
            }
            set
            {
                if (this.FBaseStream == null)
                {
                    throw new ObjectDisposedException("ComStreamWrapper");
                }
                this.FBaseStream.Seek(value, 0, IntPtr.Zero);
            }
        }
    }
    #endregion
}