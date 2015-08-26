using ImageProcessor;
using ImageProcessor.Imaging;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Boyd.Steganography
{
    public enum BitsToEncode
    {
        One = 1,
        Two = 2,
        Four = 4,
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
                    throw new NotImplementedException("2 bytes per pixel is not yet supported.");    
                    //return 2;

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

        /// <summary>
        /// Gets whether a souce image has been loaded.
        /// </summary>
        public bool IsImageLoaded { get; private set; }

        private ImageFactory SourceImage;
        private int MessagePosition;
        private bool EndOfMessageReached;

        public Image()
        {
            this.AvailableMessageBytes = 0;
            this.AvailableMessageCharacters = 0;
            this.BytesPerPixel = 0;
            this.CharacterEncoding = Encoding.UTF8;
            this.EncodedBits = BitsToEncode.One;
            this.SourceImage = null;
            this.ImageDataLength = 0;
            this.ImagePixels = 0;
            this.PixelFormat = System.Drawing.Imaging.PixelFormat.DontCare;
            this.IsImageLoaded = false;

            this.MessagePosition = 0;
            this.EndOfMessageReached = false;
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
            SourceImage = new ImageFactory();
            SourceImage.Load(imageData);

            PixelFormat = SourceImage.Image.PixelFormat;
            BytesPerPixel = GetBytesPerPixel(PixelFormat);

            ImagePixels = SourceImage.Image.Width * SourceImage.Image.Height;
            ImageDataLength = ImagePixels * BytesPerPixel;


            CalculateAvailableMessageBytes();

            IsImageLoaded = true;
        }

        /// <summary>
        /// Encodes a message into the loaded image.
        /// </summary>
        /// <param name="message"></param>
        public void EncodeMessage(string message, Stream outStream)
        {
            if(!IsImageLoaded)
            {
                throw new InvalidOperationException("No source image has been loaded.");
            }

            byte[] messageBytes = CharacterEncoding.GetBytes(message);

            if(messageBytes.Length > AvailableMessageBytes)
            {
                throw new InvalidOperationException(
                    string.Format("Unable to fit message. Message is {0} bytes, and only {1} bytes are available.",
                        messageBytes.Length,
                        AvailableMessageBytes)
                    );
            }

            MessagePosition = 0;
            EndOfMessageReached = false;

            using (FastBitmap bitmap = new FastBitmap(SourceImage.Image))
            {
                for(int y = 0; y < bitmap.Height; y++)
                {
                    for(int x = 0; x < bitmap.Width; x++)
                    {
                        WriteBytesToPixel(bitmap, messageBytes, x, y);

                        if(EndOfMessageReached)
                        {
                            break;
                        }
                    }


                    if (EndOfMessageReached)
                    {
                        break;
                    }
                }
            }


            SourceImage.Save(outStream);
        }

        private void WriteBytesToPixel(FastBitmap bitmap, byte[] messageBytes, int x, int y)
        {
            Color oldPixel = bitmap.GetPixel(x, y);

            byte a, r, g, b;

            switch (EncodedBits)
            {
                case BitsToEncode.One:
                    byte valueMask;

                    if (!EndOfMessageReached && GetNextMessageBitValue(messageBytes, MessagePosition, out valueMask))
                    {
                        r = (byte)(oldPixel.R & valueMask);
                        MessagePosition += 1;
                    }
                    else
                    {
                        r = oldPixel.R;
                        EndOfMessageReached = true;
                    }

                    if (!EndOfMessageReached && GetNextMessageBitValue(messageBytes, MessagePosition, out valueMask))
                    {
                        g = (byte)(oldPixel.R & valueMask);
                        MessagePosition += 1;
                    }
                    else
                    {
                        g = oldPixel.G;
                        EndOfMessageReached = true;
                    }


                    if (!EndOfMessageReached && GetNextMessageBitValue(messageBytes, MessagePosition, out valueMask))
                    {
                        b = (byte)(oldPixel.R & valueMask);
                        MessagePosition += 1;
                    }
                    else
                    {
                        b = oldPixel.B;
                        EndOfMessageReached = true;
                    }

                    if (BytesPerPixel == 4 && !EndOfMessageReached && GetNextMessageBitValue(messageBytes, MessagePosition, out valueMask))
                    {
                        a = (byte)(oldPixel.R & valueMask);
                        MessagePosition += 1;
                    }
                    else
                    {
                        a = oldPixel.A;
                        EndOfMessageReached = true;
                    }

                    break;

                case BitsToEncode.Two:
                case BitsToEncode.Four:
                case BitsToEncode.Eight:
                    throw new NotImplementedException();
                default:
                    throw new InvalidOperationException(string.Format("Unrecognized value for BitsToEncode enum: {0}.", EncodedBits.ToString()));
            }

            Color newPixel;

            if(BytesPerPixel == 4)
            {
                newPixel = Color.FromArgb(a, r, g, b);
            }
            else
            {
                newPixel = Color.FromArgb(r, g, b);
            }

            bitmap.SetPixel(x, y, newPixel);

        }

        private bool GetNextMessageBitValue(byte[] messageBytes, int position, out byte valueMask)
        {
            int index = position / 8;

            if(index < messageBytes.Length)
            {
                int offset = position % 8;

                byte mask = (byte)(0x01 << (7 - offset));
                valueMask = (byte)(((messageBytes[index] & mask) >> offset) | 0xFE);

                return true;
            }
            else
            {
                valueMask = 0;
                return false;
            }
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
                if(SourceImage != null)
                {
                    SourceImage.Dispose();
                }
            }
        }

        #endregion
    }
}
