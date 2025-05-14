using System;
using System.Collections.Generic;
using System.Drawing;

namespace LogicAnalyzerSim
{
    public class LogicAnalyzer
    {
        private int channels;
        private int samples;
        private int sampleRate;
        private bool[][] data;
        private SignalGenerator generator;
        private ProtocolDecoder decoder;
        private SignalAnalyzer analyzer;
        private EyeDiagram eyeDiagram;
        private Trigger trigger;

        public LogicAnalyzer(int channels, int samples)
        {
            this.channels = channels;
            this.samples = samples;
            this.sampleRate = 1000; // Default sample rate
            data = new bool[channels][];
            for (int i = 0; i < channels; i++)
                data[i] = new bool[samples];
            generator = new SignalGenerator(channels, samples);
            decoder = new ProtocolDecoder(channels, samples, sampleRate);
            analyzer = new SignalAnalyzer(channels, samples, sampleRate);
            eyeDiagram = new EyeDiagram(channels, samples, sampleRate);
        }

        public int SampleRate
        {
            get => sampleRate;
            set
            {
                sampleRate = value;
                generator.UpdateSampleRate(sampleRate);
                decoder.UpdateSampleRate(sampleRate);
                analyzer.UpdateSampleRate(sampleRate);
                eyeDiagram.UpdateSampleRate(sampleRate);
            }
        }

        public void SetTrigger(Trigger trigger)
        {
            this.trigger = trigger;
        }

        public void Start()
        {
            // Generate initial data
            generator.GenerateData(data, 0, samples);

            // Check trigger condition
            int triggerPoint = trigger != null ? trigger.FindTrigger(data) : 0;
            if (triggerPoint > 0)
            {
                // Shift data to align trigger point at the start
                bool[][] newData = new bool[channels][];
                for (int ch = 0; ch < channels; ch++)
                {
                    newData[ch] = new bool[samples];
                    for (int i = 0; i < samples; i++)
                    {
                        int srcIndex = (i + triggerPoint) % samples;
                        newData[ch][i] = data[ch][srcIndex];
                    }
                }
                data = newData;
            }

            // Process data
            decoder.Decode(data);
            analyzer.Analyze(data);
            eyeDiagram.Generate(data);
        }

        public void Stop()
        {
            // isRunning = false;
        }

        public bool[] GetChannelData(int channel)
        {
            if (channel < 0 || channel >= channels) return new bool[samples];
            return data[channel];
        }

        public List<string> GetDecodedData()
        {
            return decoder.GetDecodedData();
        }

        public List<string> GetMetricsData()
        {
            return analyzer.GetMetrics();
        }

        public List<string> GetGlitchData()
        {
            return analyzer.GetGlitches();
        }

        public List<string> GetFFTResults(int channel)
        {
            return analyzer.GetFFTResults(channel);
        }

        public Bitmap GetEyeDiagramData(int channel, int width, int height)
        {
            return eyeDiagram.GetEyeDiagram(channel, width, height);
        }
    }
}