using System;
using System.Numerics; // Ensure this is present for Complex type
using System.Collections.Generic;

namespace LogicAnalyzerSim
{
    public class SignalAnalyzer
    {
        private int channels;
        private int samples;
        private int sampleRate;
        private double[][] fftResults;
        private List<string> metrics;
        private List<string> glitches;

        public SignalAnalyzer(int channels, int samples, int sampleRate)
        {
            this.channels = channels;
            this.samples = samples;
            this.sampleRate = sampleRate;
            fftResults = new double[channels][];
            for (int i = 0; i < channels; i++)
                fftResults[i] = new double[samples / 2];
            metrics = new List<string>();
            glitches = new List<string>();
        }

        public void UpdateSampleRate(int newSampleRate)
        {
            this.sampleRate = newSampleRate;
        }

        public void Analyze(bool[][] data)
        {
            ComputeFFT(data);
            ComputeMetrics(data);
            DetectGlitches(data);
        }

        private void ComputeFFT(bool[][] data)
        {
            for (int ch = 0; ch < channels; ch++)
            {
                Complex[] fftInput = new Complex[samples];
                for (int i = 0; i < samples; i++)
                    fftInput[i] = new Complex(data[ch][i] ? 1.0 : 0.0, 0.0);

                FFT(fftInput);

                for (int i = 0; i < samples / 2; i++)
                    fftResults[ch][i] = fftInput[i].Magnitude;
            }
        }

        private void FFT(Complex[] data)
        {
            int n = data.Length;
            if (n <= 1) return;

            Complex[] even = new Complex[n / 2];
            Complex[] odd = new Complex[n / 2];
            for (int i = 0; i < n / 2; i++)
            {
                even[i] = data[i * 2];
                odd[i] = data[i * 2 + 1];
            }

            FFT(even);
            FFT(odd);

            for (int k = 0; k < n / 2; k++)
            {
                Complex t = Complex.FromPolarCoordinates(1.0, -2 * Math.PI * k / n) * odd[k];
                data[k] = even[k] + t;
                data[k + n / 2] = even[k] - t;
            }
        }

        private void ComputeMetrics(bool[][] data)
        {
            metrics.Clear();
            for (int ch = 0; ch < channels; ch++)
            {
                // Frequency: Count transitions to estimate frequency
                int transitions = 0;
                for (int i = 1; i < samples; i++)
                    if (data[ch][i] != data[ch][i - 1])
                        transitions++;
                double period = (double)samples / (transitions / 2.0);
                double frequency = transitions > 0 ? sampleRate / period : 0;

                // Duty Cycle: Percentage of time the signal is high
                int highCount = 0;
                for (int i = 0; i < samples; i++)
                    if (data[ch][i])
                        highCount++;
                double dutyCycle = (highCount / (double)samples) * 100;

                // Rise/Fall Time: Approximate by finding transition times
                double riseTime = 0;
                double fallTime = 0;
                int riseCount = 0;
                int fallCount = 0;
                for (int i = 1; i < samples; i++)
                {
                    if (!data[ch][i - 1] && data[ch][i]) // Rising edge
                    {
                        riseTime += 1.0 / sampleRate;
                        riseCount++;
                    }
                    else if (data[ch][i - 1] && !data[ch][i]) // Falling edge
                    {
                        fallTime += 1.0 / sampleRate;
                        fallCount++;
                    }
                }
                riseTime = riseCount > 0 ? (riseTime / riseCount) * 1e6 : 0; // Convert to microseconds
                fallTime = fallCount > 0 ? (fallTime / fallCount) * 1e6 : 0; // Convert to microseconds

                metrics.Add($"Channel {ch}:");
                metrics.Add($"  Frequency: {frequency:F2} Hz");
                metrics.Add($"  Duty Cycle: {dutyCycle:F2}%");
                metrics.Add($"  Rise Time: {riseTime:F2} µs");
                metrics.Add($"  Fall Time: {fallTime:F2} µs");
            }
        }

        private void DetectGlitches(bool[][] data)
        {
            glitches.Clear();
            for (int ch = 0; ch < channels; ch++)
            {
                for (int i = 1; i < samples - 1; i++)
                {
                    if (data[ch][i - 1] == data[ch][i + 1] && data[ch][i] != data[ch][i - 1])
                    {
                        glitches.Add($"Glitch detected on Channel {ch} at sample {i}");
                    }
                }
            }
        }

        public List<string> GetMetrics()
        {
            return metrics;
        }

        public List<string> GetGlitches()
        {
            return glitches;
        }

        public List<string> GetFFTResults(int channel)
        {
            List<string> results = new List<string>();
            if (channel < 0 || channel >= channels) return results;

            results.Add($"Channel {channel} Frequency Components:");
            double freqStep = sampleRate / (double)samples;
            for (int i = 0; i < fftResults[channel].Length; i++)
            {
                double freq = i * freqStep;
                if (freq > 0 && fftResults[channel][i] > 0.1) // Threshold to filter noise
                    results.Add($"  Freq: {freq:F2} Hz, Magnitude: {fftResults[channel][i]:F2}");
            }
            return results;
        }
    }
}