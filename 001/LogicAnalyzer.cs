using System;
using System.Collections.Generic;

namespace LogicAnalyzerSim
{
    public class LogicAnalyzer
    {
        private int channels;
        private int samples;
        private bool[][] data;
        private SignalGenerator generator;
        private ProtocolDecoder decoder;
        private bool isCapturing;
        private int sampleRate;
        private Trigger trigger;

        public int Channels => channels;
        public int Samples => samples;
        public int SampleRate
        {
            get => sampleRate;
            set => sampleRate = Math.Max(1, value);
        }

        public LogicAnalyzer(int channels, int samples)
        {
            this.channels = channels;
            this.samples = samples;
            this.sampleRate = 1000;
            data = new bool[channels][];
            for (int i = 0; i < channels; i++)
                data[i] = new bool[samples];
            generator = new SignalGenerator(channels, samples);
            decoder = new ProtocolDecoder(sampleRate);
            isCapturing = false;
        }

        public void SetTrigger(Trigger trigger)
        {
            this.trigger = trigger;
        }

        public void Start()
        {
            if (!isCapturing)
            {
                isCapturing = true;
                data = generator.GenerateSignals();
                if (trigger != null)
                {
                    int triggerPoint = trigger.FindTriggerPoint(data);
                    if (triggerPoint > 0)
                    {
                        bool[][] newData = new bool[channels][];
                        for (int ch = 0; ch < channels; ch++)
                        {
                            newData[ch] = new bool[samples];
                            for (int i = 0; i < samples; i++)
                            {
                                int srcIdx = (i + triggerPoint) % samples;
                                newData[ch][i] = data[ch][srcIdx];
                            }
                        }
                        data = newData;
                    }
                }
            }
        }

        public void Stop()
        {
            isCapturing = false;
        }

        public bool[] GetChannelData(int channel)
        {
            if (channel < 0 || channel >= channels)
                throw new ArgumentException("Invalid channel index");
            return data[channel];
        }

        public List<string> GetDecodedData()
        {
            return decoder.DecodeUART(data[0]);
        }
    }
}