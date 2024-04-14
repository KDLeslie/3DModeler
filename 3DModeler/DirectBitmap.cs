using System;
using System.Collections.Generic;
using System.Drawing.Imaging;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;

namespace _3DModeler
{

    // A bitmap class used for efficient getting and setting of pixels
    // by A.Konzel. Taken from the link below
    // https://stackoverflow.com/questions/24701703/c-sharp-faster-alternatives-to-setpixel-and-getpixel-for-bitmaps-for-windows-f
    public class DirectBitmap : IDisposable
    {
        public DirectBitmap()
        {

        }
        // Initializes a bitmap from an image file
        public DirectBitmap(string filePath)
        {
            // Create a temporary bitmap
            Bitmap bitmap = new Bitmap(filePath);
            Width = bitmap.Width;
            Height = bitmap.Height;
            Pixels = new Int32[Width * Height];
            // Lock the bitmap in memory
            BitmapData data = bitmap.LockBits(new Rectangle(0, 0, Width, Height), ImageLockMode.ReadWrite, PixelFormat.Format32bppArgb);
            // Get the address of the bitmap data in memory
            IntPtr ptr = data.Scan0;
            // Copy the ARGB values from the temporary bitmap to a new pixel data array
            Marshal.Copy(ptr, Pixels, 0, Width * Height);
            // Get a handle to the pixel data
            BitsHandle = GCHandle.Alloc(Pixels, GCHandleType.Pinned);
            // Create a new bitmap that uses the array for its pixel information
            Bitmap = new Bitmap(Width, Height, Width * 4, PixelFormat.Format32bppArgb, BitsHandle.AddrOfPinnedObject());
            // Dispose of the temporary bitmap
            bitmap.UnlockBits(data);
            bitmap.Dispose();
        }

        public DirectBitmap(int width, int height)
        {
            Width = width;
            Height = height;
            Pixels = new Int32[Width * Height];
            BitsHandle = GCHandle.Alloc(Pixels, GCHandleType.Pinned);
            Bitmap = new Bitmap(Width, Height, Width * 4, PixelFormat.Format32bppArgb, BitsHandle.AddrOfPinnedObject());
        }
        // Initializes a bitmap consisting of one color
        public DirectBitmap(int width, int height, Color color)
        {
            Width = width;
            Height = height;
            Pixels = new Int32[Width * Height];
            int argb = color.ToArgb();
            for (int i = 0; i < Width * Height; i++)
            {
                Pixels[i] = argb;
            }
            BitsHandle = GCHandle.Alloc(Pixels, GCHandleType.Pinned);
            Bitmap = new Bitmap(Width, Height, Width * 4, PixelFormat.Format32bppArgb, BitsHandle.AddrOfPinnedObject());
        }

        public Bitmap Bitmap { get; private set; }
        public Int32[] Pixels { get; private set; } // Color data for each pixel
        public bool Disposed { get; private set; }
        public int Height { get; private set; }
        public int Width { get; private set; }
        protected GCHandle BitsHandle { get; private set; } // Used for retaining the pixel color data in memory

        public void Clear()
        {
            Array.Clear(Pixels);
        }

        public void SetPixel(int x, int y, Color color)
        {
            Pixels[y * Width + x] = color.ToArgb();
        }

        public void RemoveAlpha()
        {
            for (int p = 0; p < Pixels.Length; p++)
            {
                Pixels[p] = (int)(Pixels[p] | 0xFF000000);
            }
        }

        public Color GetPixel(int x, int y)
        {
            return Color.FromArgb(Pixels[y * Width + x]);
        }

        public void Dispose()
        {
            if (Disposed)
                return;
            Disposed = true;
            Bitmap.Dispose();
            BitsHandle.Free();
        }
    }
}
