using System;
using System.Collections.Generic;

namespace LogicAnalyzerSim
{
    public class ProtocolDecoder
    {
        private int channels;
        private int samples;
        private int sampleRate;
        private List<string> decodedData;

        public ProtocolDecoder(int channels, int samples, int sampleRate)
        {
            this.channels = channels;
            this.samples = samples;
            this.sampleRate = sampleRate;
            decodedData = new List<string>();
        }

        public void UpdateSampleRate(int newSampleRate)
        {
            this.sampleRate = newSampleRate;
        }

        public void Decode(bool[][] data)
        {
            decodedData.Clear();
            for (int ch = 0; ch < channels; ch++)
            {
                // Simple UART decoding simulation (9600 baud, 8N1)
                int baudRate = 9600;
                int samplesPerBit = sampleRate / baudRate;
                List<byte> bytes = new List<byte>();
                int bitIndex = 0;
                byte currentByte = 0;
                bool inFrame = false;

                for (int i = 0; i < samples; i++)
                {
                    if (!inFrame && !data[ch][i]) // Start bit (low)
                    {
                        inFrame = true;
                        bitIndex = 0;
                        currentByte = 0;
                        i += samplesPerBit / 2; // Move to middle of start bit
                    }
                    else if (inFrame)
                    {
                        if (bitIndex < 8)
                        {
                            if (data[ch][i])
                                currentByte |= (byte)(1 << bitIndex);
                            bitIndex++;
                        }
                        else if (bitIndex == 8) // Stop bit (high)
                        {
                            if (data[ch][i]) // Valid stop bit
                                bytes.Add(currentByte);
                            inFrame = false;
                        }
                        i += samplesPerBit - 1; // Move to next bit
                    }
                }

                string result = $"Channel {ch}: ";
                if (bytes.Count > 0)
                    result += string.Join(" ", bytes.ConvertAll(b => b.ToString("X2")));
                else
                    result += "No data";
                decodedData.Add(result);
            }
        }

        public List<string> GetDecodedData()
        {
            return decodedData;
        }
    }
}