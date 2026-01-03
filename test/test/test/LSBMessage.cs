using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;

namespace test
{
    internal class LSBMessage
    {
        private BitArray _bits;
        public BitArray Bits { get { return _bits; } set { _bits = value; } }
        public LSBMessage(string message) {
            byte[] message_bytes = Encoding.UTF8.GetBytes(message);
            long message_length = message_bytes.Length;
            byte[] length_segment_bytes = BitConverter.GetBytes(message_length);
            byte[] fullmessage = new byte[length_segment_bytes.Length + message_bytes.Length];
            if (BitConverter.IsLittleEndian)
            {
                Array.Reverse(length_segment_bytes);
            }
            for (int i = 0; i < length_segment_bytes.Length; i++)
            {
                fullmessage[i] = ReverseByte(length_segment_bytes[i]);
            }
            for (int i = 0; i < message_bytes.Length; i++)
            {
                fullmessage[i + length_segment_bytes.Length] = ReverseByte(message_bytes[i]);
            }

            BitArray fullmessage_bits = new BitArray(fullmessage); 
            Bits = fullmessage_bits;
        }

        public string LengthSegmentToString()
        {
            string result = "";
            for(int i = 0; i < 64; i++)
            {
                result += Bits[i] ? "1" : 0;
            }
            return result;
        }
        public override string ToString()
        {
            string result = "";
            foreach (bool bit in Bits)
            {
                result += bit ? "1" : 0;
            }
            return result;
        }
        byte ReverseByte(byte b)
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
