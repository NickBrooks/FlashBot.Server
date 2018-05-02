using SixLabors.ImageSharp;
using SixLabors.ImageSharp.Formats;
using SixLabors.ImageSharp.Processing;
using SixLabors.ImageSharp.Processing.Transforms;
using System.IO;

namespace FlashFeed.Engine
{
    public class Images
    {
        public static void CropSquare(int size, Stream input, Stream output)
        {
            using (var image = Image.Load(input))
            {
                image.Mutate(x => x
                     .Resize(150, 150));

                image.SaveAsJpeg(output);
            }
        }

        //public static void ResizeToMax(int size, MemoryStream input, MemoryStream output)
        //{
        //    using (var factory = new ImageFactory())
        //    using (var memory = new MemoryStream())
        //    {
        //        factory.Load(input)
        //          .Resize(new ResizeLayer(new Size(size, size), ResizeMode.Max))
        //          .Quality(70)
        //          .Save(memory);

        //        memory.CopyTo(output);
        //    }
        //}

        //public static void GenerateHero(int width, MemoryStream input, MemoryStream output)
        //{
        //    using (var factory = new ImageFactory())
        //    using (var memory = new MemoryStream())
        //    {
        //        factory.Load(input)
        //          .Resize(new ResizeLayer(new Size(width, Convert.ToInt32(Math.Round(width * 0.5625))), ResizeMode.Crop))
        //          .Quality(70)
        //          .Save(memory);

        //        memory.CopyTo(output);
        //    }
        //}

        //public static byte[] ImageToByteArray(Image image)
        //{
        //    using (var ms = new MemoryStream())
        //    {
        //        image.Save(ms, image.RawFormat);
        //        return ms.ToArray();
        //    }
        //}

        //public static string GetContentType(byte[] data)
        //{
        //    if (ImageFormat.Jpeg.Equals(data))
        //    {
        //        return "image/jpeg";
        //    }
        //    else if (ImageFormat.Png.Equals(data))
        //    {
        //        return "image/png";
        //    }

        //    return null;
        //}
    }
}
