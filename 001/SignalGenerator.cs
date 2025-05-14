using System;

namespace LogicAnalyzerSim
{
    public class SignalGenerator
    {
        private Random random = new Random();
        private int channels;
        private int samples;

        public SignalGenerator(int channels, int samples)
        {
            this.channels = channels;
            this.samples = samples;
        }

        public bool[][] GenerateSignals()
        {
            bool[][] signals = new bool[channels][];
            for (int ch = 0; ch < channels; ch++)
            {
                signals[ch] = new bool[samples];
                if (ch == 0) // Channel 0 simulates UART-like signal
                {
                    int bitLength = 20; // Simulate 1 bit duration
                    int byteCount = samples / (bitLength * 9); // 8 data bits + 1 start bit
                    for (int b = 0; b < byteCount; b++)
                    {
                        int start = b * bitLength * 9;
                        signals[ch][start] = false; // Start bit
                        byte data = (byte)random.Next(0, 256);
                        for (int bit = 0; bit < 8; bit++)
                        {
                            bool bitValue = (data & (1 << bit)) != 0;
                            for (int i = 0; i < bitLength; i++)
                            {
                                int idx = start + (bit + 1) * bitLength + i;
                                if (idx < samples) signals[ch][idx] = bitValue;
                            }
                        }
                    }
                }
                else // Other channels generate random square waves
                {
                    int period = random.Next(50, 200);
                    for (int i = 0; i < samples; i++)
                    {
                        signals[ch][i] = (i / period) % 2 == 0;
                        if (random.NextDouble() < 0.05)
                            signals[ch][i] = !signals[ch][i];
                    }
                }
            }
            return signals;
        }
    }
}