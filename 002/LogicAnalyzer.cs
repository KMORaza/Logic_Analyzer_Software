using System;
using System.Collections.Generic;

namespace LogicAnalyzerSim
{
    public class LogicAnalyzer
    {
        private int channels;
        private int samples;
        private int sampleRate;
        private bool[][] data;
        private SignalGenerator generator;
        private SignalAnalyzer analyzer;
        private ProtocolDecoder decoder;
        private EyeDiagram eyeDiagram;
        private Trigger trigger;
        private bool isRunning;
        private int captureIndex;

        public LogicAnalyzer(int channels, int samples)
        {
            this.channels = channels;
            this.samples = samples;
            this.sampleRate = 1000; // Default sample rate
            data = new bool[channels][];
            for (int i = 0; i < channels; i++)
                data[i] = new bool[samples];
            generator = new SignalGenerator(channels, samples);
            analyzer = new SignalAnalyzer(channels, samples, sampleRate);
            decoder = new ProtocolDecoder(channels, samples, sampleRate);
            eyeDiagram = new EyeDiagram(channels, samples, sampleRate);
            isRunning = false;
            captureIndex = 0;
        }

        public int SampleRate
        {
            get => sampleRate;
            set
            {
                sampleRate = value;
                generator.UpdateSampleRate(sampleRate);
                analyzer.UpdateSampleRate(sampleRate);
                decoder.UpdateSampleRate(sampleRate);
                eyeDiagram.UpdateSampleRate(sampleRate);
            }
        }

        public void SetTrigger(Trigger trigger)
        {
            this.trigger = trigger;
        }

        public void Start()
        {
            isRunning = true;
            captureIndex = 0;
            generator.GenerateData(data, 0, samples);
            if (trigger != null)
            {
                int triggerPoint = trigger.FindTriggerPoint(data);
                if (triggerPoint >= 0)
                    captureIndex = triggerPoint;
            }
            AnalyzeData();
        }

        public void Stop()
        {
            isRunning = false;
        }

        private void AnalyzeData()
        {
            analyzer.Analyze(data);
            decoder.Decode(data);
            eyeDiagram.Generate(data);
        }

        public void SimulateStep()
        {
            if (!isRunning) return;

            int newSamples = samples / 10;
            if (captureIndex + newSamples > samples)
            {
                // Shift data left
                for (int ch = 0; ch < channels; ch++)
                {
                    Array.Copy(data[ch], newSamples, data[ch], 0, samples - newSamples);
                    for (int i = samples - newSamples; i < samples; i++)
                        data[ch][i] = false;
                }
                captureIndex = samples - newSamples;
            }

            generator.GenerateData(data, captureIndex, newSamples);
            captureIndex += newSamples;

            if (trigger != null)
            {
                int triggerPoint = trigger.FindTriggerPoint(data);
                if (triggerPoint >= 0)
                    captureIndex = triggerPoint;
            }

            AnalyzeData();
        }

        public bool[] GetChannelData(int channel)
        {
            if (channel < 0 || channel >= channels) return new bool[0];
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

        public string GetEyeDiagramData(int channel)
        {
            return eyeDiagram.GetEyeDiagram(channel);
        }
    }
}