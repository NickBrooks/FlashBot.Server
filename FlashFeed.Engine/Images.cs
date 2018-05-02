using PhotoSauce.MagicScaler;
using System;
using System.IO;

namespace FlashFeed.Engine
{
    public class Images
    {
        public static void CropSquare(int size, Stream input, Stream output)
        {
            MagicImageProcessor.ProcessImage(input, output, new ProcessImageSettings { Width = size, Height = size, SaveFormat = FileFormat.Jpeg, ResizeMode = CropScaleMode.Crop });
        }

        public static void Resize(int size, Stream input, Stream output)
        {
            MagicImageProcessor.ProcessImage(input, output, new ProcessImageSettings { Width = size, Height = size, SaveFormat = FileFormat.Jpeg, ResizeMode = CropScaleMode.Max });
        }

        public static void GenerateHero(int size, Stream input, Stream output)
        {
            MagicImageProcessor.ProcessImage(input, output, new ProcessImageSettings { Width = size, Height = Convert.ToInt32(Math.Round(size * 0.5625)), SaveFormat = FileFormat.Jpeg, ResizeMode = CropScaleMode.Crop });
        }
    }
}
