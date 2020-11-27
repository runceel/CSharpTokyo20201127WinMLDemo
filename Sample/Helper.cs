using System;
using System.IO;
using System.Runtime.InteropServices.WindowsRuntime;
using System.Threading.Tasks;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using Windows.Graphics.Imaging;
using Windows.Media;
using Windows.Storage.Streams;

namespace Windows10APIs.Samples
{
    public static class Helper
    {
        public static async Task<VideoFrame> GetHandWrittenImageAsync(InkCanvas inkCanvas)
        {
            // WPF の API で InkCanvas を PNG にする
            var renderTargetBitmap = new RenderTargetBitmap(
                (int)inkCanvas.ActualWidth,
                (int)inkCanvas.ActualHeight,
                96.0,
                96.0,
                PixelFormats.Default);
            renderTargetBitmap.Render(inkCanvas);

            using var ms = new MemoryStream();
            var encoder = new PngBitmapEncoder();
            encoder.Frames.Add(System.Windows.Media.Imaging.BitmapFrame.Create(renderTargetBitmap));
            encoder.Save(ms);
            await ms.FlushAsync();

            // WPF の API で作った PNG を Windows ML に渡せる形に変換
            using var stream = new InMemoryRandomAccessStream();
            await stream.WriteAsync(ms.ToArray().AsBuffer());
            stream.Seek(0);

            var decoder = await Windows.Graphics.Imaging.BitmapDecoder.CreateAsync(stream);
            var softwareBitmap = await decoder.GetSoftwareBitmapAsync();

            VideoFrame vf = VideoFrame.CreateWithSoftwareBitmap(softwareBitmap);

            // mnist が想定しているサイズにリサイズ
            return await CropAndDisplayInputImageAsync(vf);
        }

        const uint Width = 224;
        const uint Height = 224;

        private static async Task<VideoFrame> CropAndDisplayInputImageAsync(VideoFrame inputVideoFrame)
        {
            bool useDX = inputVideoFrame.SoftwareBitmap == null;

            BitmapBounds cropBounds = new BitmapBounds();
            uint h = Height;
            uint w = Width;
            var frameHeight = useDX ? inputVideoFrame.Direct3DSurface.Description.Height : inputVideoFrame.SoftwareBitmap.PixelHeight;
            var frameWidth = useDX ? inputVideoFrame.Direct3DSurface.Description.Width : inputVideoFrame.SoftwareBitmap.PixelWidth;

            var requiredAR = ((float)Width / Height);
            w = Math.Min((uint)(requiredAR * frameHeight), (uint)frameWidth);
            h = Math.Min((uint)(frameWidth / requiredAR), (uint)frameHeight);
            cropBounds.X = (uint)((frameWidth - w) / 2);
            cropBounds.Y = 0;
            cropBounds.Width = w;
            cropBounds.Height = h;

            var vf = new VideoFrame(BitmapPixelFormat.Bgra8, (int)Width, (int)Height, BitmapAlphaMode.Ignore);

            await inputVideoFrame.CopyToAsync(vf, cropBounds, null);
            return vf;
        }

    }
}
