using System;
using System.Text;

namespace LogicAnalyzerSim
{
    public class EyeDiagram
    {
        private int channels;
        private int samples;
        private int sampleRate;
        private string[] eyeDiagrams;

        public EyeDiagram(int channels, int samples, int sampleRate)
        {
            this.channels = channels;
            this.samples = samples;
            this.sampleRate = sampleRate;
            eyeDiagrams = new string[channels];
        }

        public void UpdateSampleRate(int newSampleRate)
        {
            this.sampleRate = newSampleRate;
        }

        public void Generate(bool[][] data)
        {
            for (int ch = 0; ch < channels; ch++)
            {
                // Simplified eye diagram as text (20x5 grid)
                char[,] diagram = new char[5, 20];
                for (int y = 0; y < 5; y++)
                    for (int x = 0; x < 20; x++)
                        diagram[y, x] = '.';

                int period = sampleRate / 1000; // Assume 1kHz signal for simplicity
                for (int i = 0; i < samples; i++)
                {
                    int x = (i % period) * 20 / period;
                    int y = data[ch][i] ? 1 : 3;
                    diagram[y, x] = '#';
                }

                StringBuilder sb = new StringBuilder();
                sb.AppendLine($"Channel {ch} Eye Diagram:");
                for (int y = 0; y < 5; y++)
                {
                    for (int x = 0; x < 20; x++)
                        sb.Append(diagram[y, x]);
                    sb.AppendLine();
                }
                eyeDiagrams[ch] = sb.ToString();
            }
        }

        public string GetEyeDiagram(int channel)
        {
            if (channel < 0 || channel >= channels) return "Invalid channel";
            return eyeDiagrams[channel] ?? "No data";
        }
    }
}