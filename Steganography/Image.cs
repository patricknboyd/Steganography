using ImageProcessor;
using System;
using System.Collections.Generic;
using System.Drawing.Imaging;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Boyd.Steganography
{
    public enum BitsToEncode
    {
        Zero = 0,
        One = 1,
        Two = 2,
        Three = 3,
        Four = 4,
        Five = 5,
        Six = 6,
        Seven = 7,
        Eight = 8
    }

    public class Image : IDisposable
    {
        /// <summary>
        /// Gets the number of bytes per pixel for a given pixel format.
        /// </summary>
        /// <param name="format"></param>
        /// <returns></returns>
        public static int GetBytesPerPixel(PixelFormat format)
        {
            switch(format)
            {
                case PixelFormat.Format16bppArgb1555:
                case PixelFormat.Format16bppGrayScale:
                case PixelFormat.Format16bppRgb555:
                case PixelFormat.Format16bppRgb565:
                    return 2;

                case PixelFormat.Format24bppRgb:
                    return 3;

                case PixelFormat.Format32bppArgb:
                case PixelFormat.Format32bppPArgb:
                case PixelFormat.Format32bppRgb:
                    return 4;

                default:
                    throw new InvalidOperationException(string.Format("Unrecognized Pixel Format {0}.", format.ToString()));
            }
        }

        private BitsToEncode _encodedBits;
        /// <summary>
        /// Gets or sets the number of bits to encode for each color channel.
        /// </summary>
        public BitsToEncode EncodedBits
        {
            get { return _encodedBits; }
            set
            {
                _encodedBits = value;
                CalculateAvailableMessageBytes();
            }
        }

        private Encoding _characterEncoding;
        /// <summary>
        /// Gets or sets the Encoding to use to encode the message.
        /// </summary>
        public Encoding CharacterEncoding
        {
            get { return _characterEncoding; }
            set
            {
                _characterEncoding = value;
                CalculateAvailableMessageBytes();
            }
        }

        /// <summary>
        /// Gets the number of characters that can be encoded using the current image and settings.
        /// </summary>
        public int AvailableMessageCharacters { get; private set; }
        /// <summary>
        /// Gets the number of bytes available to encode using the current image and settings.
        /// </summary>
        public int AvailableMessageBytes { get; private set; }

        /// <summary>
        /// Gets the number of bytes in the source image.
        /// </summary>
        public long ImageDataLength { get; private set; }
        /// <summary>
        /// Gets the number of pixels in the source image.
        /// </summary>
        public long ImagePixels { get; private set; }

        /// <summary>
        /// Gets the format the pixels are stored in for this image.
        /// </summary>
        public PixelFormat PixelFormat { get; private set; }
        /// <summary>
        /// Gets the number of bytes per pixel for this image.
        /// </summary>
        public int BytesPerPixel { get; private set; }

        private ImageFactory ImageData;

        public Image()
        {
            this.AvailableMessageBytes = 0;
            this.AvailableMessageCharacters = 0;
            this.BytesPerPixel = 0;
            this.CharacterEncoding = Encoding.UTF8;
            this.EncodedBits = BitsToEncode.One;
            this.ImageData = null;
            this.ImageDataLength = 0;
            this.ImagePixels = 0;
            this.PixelFormat = System.Drawing.Imaging.PixelFormat.DontCare;
        }

        /// <summary>
        /// Loads an image from the given file path.
        /// </summary>
        /// <param name="filePath">The file path to the image.</param>
        public void LoadImage(string filePath)
        {
            byte[] data = System.IO.File.ReadAllBytes(filePath);

            LoadImage(data);
        }

        /// <summary>
        /// Loads an image from byte data.
        /// </summary>
        /// <param name="imageData">The image byte data.</param>
        public void LoadImage(byte[] imageData)
        {
            ImageData = new ImageFactory();
            ImageData.Load(imageData);

            PixelFormat = ImageData.Image.PixelFormat;
            BytesPerPixel = GetBytesPerPixel(PixelFormat);

            ImagePixels = ImageData.Image.Width * ImageData.Image.Height;
            ImageDataLength = ImagePixels * BytesPerPixel;


            CalculateAvailableMessageBytes();
            
        }


        private void CalculateAvailableMessageBytes()
        {
            AvailableMessageBytes = (int)Math.Floor((double)ImageDataLength * ((double)EncodedBits / 8.0));
            AvailableMessageCharacters = CharacterEncoding.GetMaxCharCount(AvailableMessageBytes);
        }

        #region IDisposable Members

        public void Dispose()
        {
            Dispose(true);

            GC.SuppressFinalize(this);
        }

        protected void Dispose(bool disposing)
        {
            if(disposing)
            {
                if(ImageData != null)
                {
                    ImageData.Dispose();
                }
            }
        }

        #endregion
    }
}
