using System;
using System.Collections.Generic;

namespace LogicAnalyzerSim
{
    public class ProtocolDecoder
    {
        private int sampleRate;

        public ProtocolDecoder(int sampleRate)
        {
            this.sampleRate = sampleRate;
        }

        public List<string> DecodeUART(bool[] signal)
        {
            List<string> decoded = new List<string>();
            int bitLength = sampleRate / 50; // Assume 50 baud for simulation
            int i = 0;

            while (i < signal.Length - bitLength * 9)
            {
                if (!signal[i]) // Start bit (low)
                {
                    byte data = 0;
                    for (int bit = 0; bit < 8; bit++)
                    {
                        int sampleIdx = i + (bit + 1) * bitLength + bitLength / 2;
                        if (sampleIdx < signal.Length && signal[sampleIdx])
                            data |= (byte)(1 << bit);
                    }
                    decoded.Add($"Byte: 0x{data:X2} ('{(char)data}')");
                    i += bitLength * 9; // Skip start + 8 data bits
                }
                else
                {
                    i++;
                }
            }
            return decoded;
        }
    }
}