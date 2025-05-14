using System;

namespace LogicAnalyzerSim
{
    public class Trigger
    {
        private int channel;
        private int triggerType; // 0: Rising, 1: Falling, 2: Pattern
        private int channels;

        public Trigger(int channels)
        {
            this.channels = channels;
            this.channel = 0;
            this.triggerType = 0;
        }

        public void SetChannel(int channel)
        {
            if (channel >= 0 && channel < channels)
                this.channel = channel;
        }

        public void SetTriggerType(int type)
        {
            if (type >= 0 && type <= 2)
                this.triggerType = type;
        }

        public int FindTriggerPoint(bool[][] data)
        {
            if (triggerType == 0) // Rising edge
            {
                for (int i = 1; i < data[channel].Length; i++)
                {
                    if (!data[channel][i - 1] && data[channel][i])
                        return i;
                }
            }
            else if (triggerType == 1) // Falling edge
            {
                for (int i = 1; i < data[channel].Length; i++)
                {
                    if (data[channel][i - 1] && !data[channel][i])
                        return i;
                }
            }
            else if (triggerType == 2) // Pattern (e.g., start bit of UART on channel 0)
            {
                for (int i = 0; i < data[channel].Length; i++)
                {
                    if (!data[channel][i]) // Simulate UART start bit
                        return i;
                }
            }
            return -1; // No trigger found
        }
    }
}