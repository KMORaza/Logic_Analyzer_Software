using System;
using System.Drawing;

namespace LogicAnalyzerSim
{
    public class EyeDiagram
    {
        private int channels;
        private int samples;
        private int sampleRate;
        private bool[][] lastData;

        public EyeDiagram(int channels, int samples, int sampleRate)
        {
            this.channels = channels;
            this.samples = samples;
            this.sampleRate = sampleRate;
        }

        public void UpdateSampleRate(int newSampleRate)
        {
            this.sampleRate = newSampleRate;
        }

        public void Generate(bool[][] data)
        {
            lastData = data; // Store the data for rendering
        }

        public Bitmap GetEyeDiagram(int channel, int width, int height)
        {
            if (channel < 0 || channel >= channels || lastData == null)
                return new Bitmap(width, height);

            Bitmap bmp = new Bitmap(width, height);
            using (Graphics g = Graphics.FromImage(bmp))
            {
                g.Clear(Color.FromArgb(32, 32, 32));

                // Draw axes
                Pen axisPen = new Pen(Color.White);
                g.DrawLine(axisPen, 30, height - 30, width - 10, height - 30); // X-axis
                g.DrawLine(axisPen, 30, 10, 30, height - 30); // Y-axis

                // Use a proper Font and Brush for labels
                using (Font font = new Font("MS Sans Serif", 8.25F))
                {
                    g.DrawString("Time", font, Brushes.White, (float)(width / 2 - 20), (float)(height - 20));
                    g.DrawString("Signal", font, Brushes.White, 5.0F, (float)(height / 2 - 10));
                }

                // Estimate symbol period (e.g., 1kHz signal)
                int period = sampleRate / 1000; // Assume 1kHz signal for simplicity
                int displayPeriod = 2 * period; // Show two symbol periods (one eye)

                // Scale the X-axis to fit two symbol periods in the width
                float xScale = (width - 40) / (float)displayPeriod;
                float yScale = (height - 40) / 2.0f; // Signal levels (0 to 1)

                Pen signalPen = new Pen(Color.Cyan, 1);

                // Overlay multiple periods of the signal
                for (int start = 0; start < samples - displayPeriod; start += period)
                {
                    for (int i = 0; i < displayPeriod - 1; i++)
                    {
                        int sampleIndex = start + i;
                        if (sampleIndex + 1 >= samples) break;

                        float x1 = 30 + (i % displayPeriod) * xScale;
                        float x2 = 30 + ((i + 1) % displayPeriod) * xScale;
                        float y1 = (height - 30) - (lastData[channel][sampleIndex] ? 1 : 0) * yScale;
                        float y2 = (height - 30) - (lastData[channel][sampleIndex + 1] ? 1 : 0) * yScale;

                        g.DrawLine(signalPen, x1, y1, x2, y2);
                    }
                }

                axisPen.Dispose();
                signalPen.Dispose();
            }

            return bmp;
        }
    }
}