using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO.Compression;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Collections;
using System.Threading.Channels;
using System.Runtime.InteropServices;
using ImageMagick;
namespace test
{
    internal static class LSB
    {
        private static LSBMessage _message;
        private static LSBMessage Message { get { return _message; } set { _message = value; } }

        public static void EncryptPNGImage(string input_path, string output_path, string message)
        {
            if (!File.Exists(input_path)) throw new FileNotFoundException();

            Message = new LSBMessage(message);
            using (var bmp = new MagickImage(input_path))
            {
                var pixels = bmp.GetPixelsUnsafe();
                var mapping = bmp.HasAlpha ? PixelMapping.RGBA : PixelMapping.RGB;

                var bytes = pixels.ToByteArray(mapping);

                //Teraz format jest taki:
                //bytes[0] = R1
                //bytes[1] = G1
                //bytes[2] = B1
                //bytes[3] = A1
                //bytes[4] = R2
                //itd...

                //Length Segment
                for(int i =0; i < 64; i++)
                {
                    bytes[i] = SetLastBit(bytes[i], Message.Bits[i]);
                    Console.Write(Message.Bits[i] ? "1" : "0");
                }
                long jump = CalculateJump(bmp.Width * bmp.Height, Message.Bits.Count - 64);
                
                for (int i = 0; i< Message.Bits.Count - 64; i++)
                {
                    long byteIndex = (i * jump + 64);
                    if (byteIndex < bytes.Length)
                    {
                        bytes[(int)byteIndex] = SetLastBit(bytes[byteIndex], Message.Bits[i + 64]);
                        Console.Write(Message.Bits[i + 64] ? "1" : "0");
                    }
                    else
                    {
                        break;
                    }
                }

                pixels.SetPixels(bytes);
                bmp.Write(output_path);
            }
                

        }
        
        public static BitArray DecryptPNGImage(string input_path)
        {
            if (!File.Exists(input_path)) throw new FileNotFoundException();
            List<bool> bits_list = new List<bool>();

            using (var bmp = new MagickImage(input_path))
            {
                var pixels = bmp.GetPixelsUnsafe();
                var mapping = bmp.HasAlpha ? PixelMapping.RGBA : PixelMapping.RGB;
                var bytes = pixels.ToByteArray(mapping);

                //Length segment
                bool[] lengthArray = new bool[64];
                for (int i = 0; i < lengthArray.Length; i++)
                {
                    lengthArray[i] = readLastBit(bytes[i]);
                }
                long rawmessagelength = readBitsToLong(lengthArray) * 8;
                long jump = CalculateJump((bmp.Width * bmp.Height), rawmessagelength);

                for (int i = 0; i < rawmessagelength; i++)
                {
                    int byteIndex = (int)(i * jump + 64);
                    if(byteIndex < bytes.Length)
                    {
                        bits_list.Add(readLastBit(bytes[byteIndex]));
                    }
                }
            }
            return new BitArray(bits_list.ToArray());

        }      

        private static byte SetLastBit(byte value, bool bit)
        {
            return (byte)((value & 0b_11111110) | (bit ? 1 : 0));
        }
        private static long CalculateJump(long pixelCount,long rawmessagelength)
        {
            return (pixelCount - 64)/(rawmessagelength +1);
        }

        private static bool readLastBit(byte b)
        {
            return b % 2 != 0;
        }
        private static long readBitsToLong(bool[] bits)
        {
            Array.Reverse(bits);
            long result = 0;
            for(int i = 0 ;  i < bits.Length; i ++)
            {
                if (bits[i])
                {
                    result += (long)Math.Pow(2,i);
                }
            }
            return result;
        }
        public static byte[] ToByteArray(BitArray bits)
        {
            byte[] reversed_bytes = new byte[(bits.Length - 1) / 8 + 1];
            bits.CopyTo(reversed_bytes, 0);
            byte[] result = new byte[reversed_bytes.Length];
            if (BitConverter.IsLittleEndian)
            {
                for (int i = 0; i < reversed_bytes.Length;i++)
                {
                    result[i] = ReverseByte(reversed_bytes[i]);   
                }
            }
            return result;
        }
        public static byte ReverseByte(byte b)
        {
            byte result = 0;
            for (int i = 0; i < 8; i++)
            {
                result <<= 1;
                result |= (byte)(b & 1);
                b >>= 1;
            }
            return result;
        }
    }
}
