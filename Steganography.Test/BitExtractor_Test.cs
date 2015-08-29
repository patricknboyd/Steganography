using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Boyd.Steganography;

namespace Steganography.Test
{
    [TestClass]
    public class BitExtractor_Test
    {
        [TestMethod]
        [ExpectedException(typeof(ArgumentNullException))]
        public void BitExtractor_Constructor_NullData_Throws()
        {
            BitExtractor extractor = new BitExtractor(null, BitsToEncode.One);
        }

        [TestMethod]
        public void BitExtractor_Constructor_EmptyData_Success()
        {
            byte[] data = new byte[0];

            BitExtractor oneBit = new BitExtractor(data, BitsToEncode.One);
            BitExtractor twoBit = new BitExtractor(data, BitsToEncode.Two);
            BitExtractor fourBit = new BitExtractor(data, BitsToEncode.Four);
            BitExtractor eightBit = new BitExtractor(data, BitsToEncode.Eight);

            Assert.AreEqual(0, oneBit.EncodedByteLength);
            Assert.AreEqual(0, twoBit.EncodedByteLength);
            Assert.AreEqual(0, fourBit.EncodedByteLength);
            Assert.AreEqual(0, eightBit.EncodedByteLength);
        }

        [TestMethod]
        public void BitExtractor_Constructor_Data_Success()
        {

            byte[] data = new byte[] { 0x48, 0x45, 0x4C, 0x4C, 0x4F };

            BitExtractor oneBit = new BitExtractor(data, BitsToEncode.One);
            BitExtractor twoBit = new BitExtractor(data, BitsToEncode.Two);
            BitExtractor fourBit = new BitExtractor(data, BitsToEncode.Four);
            BitExtractor eightBit = new BitExtractor(data, BitsToEncode.Eight);

            Assert.AreEqual(40, oneBit.EncodedByteLength);
            Assert.AreEqual(20, twoBit.EncodedByteLength);
            Assert.AreEqual(10, fourBit.EncodedByteLength);
            Assert.AreEqual(5, eightBit.EncodedByteLength);
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentOutOfRangeException))]
        public void BitExtractor_GetBits_NegativeIndex_Throws()
        {
            byte[] data = new byte[] { 0x48, 0x45, 0x4C, 0x4C, 0x4F };

            BitExtractor oneBit = new BitExtractor(data, BitsToEncode.One);

            oneBit.GetBits(-1);
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentOutOfRangeException))]
        public void BitExtractor_GetBits_IndexTooLarge_Throws()
        {
            byte[] data = new byte[] { 0x48, 0x45, 0x4C, 0x4C, 0x4F };

            BitExtractor oneBit = new BitExtractor(data, BitsToEncode.One);

            oneBit.GetBits(40);
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentException))]
        public void BitExtractor_GetBits_InvalidBitsEnum_Throws()
        {
            byte[] data = new byte[] { 0x48, 0x45, 0x4C, 0x4C, 0x4F };

            BitExtractor oneBit = new BitExtractor(data, (BitsToEncode)3);

            oneBit.GetBits(5);
        }

        [TestMethod]
        public void BitExtractor_GetBits_OneBit_Success()
        {
            byte[] data = new byte[] { 0x48, 0x45, 0x4C, 0x4C, 0x4F };

            BitExtractor oneBit = new BitExtractor(data, BitsToEncode.One);

            Assert.AreEqual(0xFE, oneBit.GetBits(0));
            Assert.AreEqual(0xFF, oneBit.GetBits(1));
            Assert.AreEqual(0xFE, oneBit.GetBits(2));
            Assert.AreEqual(0xFE, oneBit.GetBits(3));
            Assert.AreEqual(0xFF, oneBit.GetBits(4));
            Assert.AreEqual(0xFE, oneBit.GetBits(5));
            Assert.AreEqual(0xFE, oneBit.GetBits(6));
            Assert.AreEqual(0xFE, oneBit.GetBits(7));
        }

        [TestMethod]
        public void BitExtractor_GetBits_TwoBit_Success()
        {
            byte[] data = new byte[] { 0x48, 0x45, 0x4C, 0x4C, 0x4F };

            BitExtractor twoBit = new BitExtractor(data, BitsToEncode.Two);

            Assert.AreEqual(0xFD, twoBit.GetBits(0));
            Assert.AreEqual(0xFC, twoBit.GetBits(1));
            Assert.AreEqual(0xFE, twoBit.GetBits(2));
            Assert.AreEqual(0xFC, twoBit.GetBits(3));
        }

        [TestMethod]
        public void BitExtractor_GetBits_FourBit_Success()
        {
            byte[] data = new byte[] { 0x48, 0x45, 0x4C, 0x4C, 0x4F };
            BitExtractor fourBit = new BitExtractor(data, BitsToEncode.Four);

            Assert.AreEqual(0xF4, fourBit.GetBits(0));
            Assert.AreEqual(0xF8, fourBit.GetBits(1));
        }

        [TestMethod]
        public void BitExtractor_GetBits_EightBit_Success()
        {
            byte[] data = new byte[] { 0x48, 0x45, 0x4C, 0x4C, 0x4F };
            BitExtractor eightBit = new BitExtractor(data, BitsToEncode.Eight);

            Assert.AreEqual(0x48, eightBit.GetBits(0));
            Assert.AreEqual(0x45, eightBit.GetBits(1));
        }
    }
}
