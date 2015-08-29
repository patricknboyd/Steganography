using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Boyd.Steganography
{
    public class BitExtractor
    {
        public BitsToEncode BitsEncodedPerByte { get; private set; }

        /// <summary>
        /// Gets the number of bytes needed to encode the whole message, based on the number of bits per byte.
        /// </summary>
        /// <remarks>
        /// For example, if there are 5 bytes in the source message, and we can encode 1 bit per byte, we need 40 bytes.
        /// If we can encode 2 bits per byte, you only need 20.
        /// </remarks>
        public int EncodedByteLength { get; private set; }

        private byte[] data;

        public BitExtractor(byte[] sourceData, BitsToEncode bitsToEncode)
        {
            if (sourceData == null)
            {
                throw new ArgumentNullException("sourceData");
            }

            this.BitsEncodedPerByte = bitsToEncode;
            this.data = sourceData;

            CalculateStorageDimensions();
        }

        public byte GetBits(int position)
        {
            if(position < 0)
            {
                throw new ArgumentOutOfRangeException("position");
            }
            else if (position >= EncodedByteLength)
            {
                throw new ArgumentOutOfRangeException("position");
            }

            switch(BitsEncodedPerByte)
            {
                case BitsToEncode.One:
                    return GetOneBit(position);

                case BitsToEncode.Two:
                    return GetTwoBits(position);

                case BitsToEncode.Four:
                    return GetFourBits(position);

                case BitsToEncode.Eight:
                    return GetEightBits(position);

                default:
                    throw new ArgumentException(string.Format("Unrecoginzed value for BitsToEncode enum: {0}.", BitsEncodedPerByte.ToString()));
            }
        }

        private void CalculateStorageDimensions()
        {
            // Since we are restricting the number of encoded bits per byte to 1, 2, 4, or 8, 
            // we don't need to worry about decimals in this calculation.
            EncodedByteLength = (data.Length * 8) / (int)BitsEncodedPerByte;
        }

        private byte GetOneBit(int position)
        {
            int sourceIndex = position / 8;
            int offset = 7 - (position % 8);

            byte result = (byte)((data[sourceIndex] >> offset) & 0x01);

            return (byte)(result | 0xFE);
            
        }

        private byte GetTwoBits(int position)
        {
            int sourceIndex = position / 4;
            int offset = (3 - (position % 4)) * 2;

            byte result = (byte)((data[sourceIndex] >> offset) & 0x03);

            return (byte)(result | 0xFC);
        }

        private byte GetFourBits(int position)
        {
            int sourceIndex = position / 2;
            int offset = (1 - (position % 2)) * 4;

            byte result = (byte)((data[sourceIndex] >> offset) & 0x0F);

            return (byte)(result | 0xF0);
        }

        private byte GetEightBits(int position)
        {
            return data[position];
        }

        private string ToBinary(int value)
        {
            return Convert.ToString(value, 2);
        }
    }
}
