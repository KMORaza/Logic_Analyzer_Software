using System;

namespace LogicAnalyzerSim
{
    public class SignalGenerator
    {
        private int channels;
        private int samples;
        private int sampleRate;
        private Random random;

        public SignalGenerator(int channels, int samples)
        {
            this.channels = channels;
            this.samples = samples;
            this.sampleRate = 1000; // Default sample rate
            random = new Random();
        }

        public void UpdateSampleRate(int newSampleRate)
        {
            this.sampleRate = newSampleRate;
        }

        public void GenerateData(bool[][] data, int startIndex, int count)
        {
            for (int ch = 0; ch < channels; ch++)
            {
                for (int i = startIndex; i < startIndex + count && i < samples; i++)
                {
                    // Generate a simple square wave with random transitions
                    if (i == startIndex || random.NextDouble() < 0.05) // 5% chance to toggle
                        data[ch][i] = random.NextDouble() > 0.5;
                    else
                        data[ch][i] = data[ch][i - 1];
                }
            }
        }
    }
}