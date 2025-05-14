using System;

namespace LogicAnalyzerSim
{
    public class Trigger
    {
        private int channels;
        private int channel;
        private int type; // 0: Rising Edge, 1: Falling Edge, 2: Pattern

        public Trigger(int channels)
        {
            this.channels = channels;
            this.channel = 0;
            this.type = 0;
        }

        public void SetChannel(int ch)
        {
            if (ch >= 0 && ch < channels)
                channel = ch;
        }

        public void SetTriggerType(int t)
        {
            if (t >= 0 && t <= 2)
                type = t;
        }

        public int FindTrigger(bool[][] data)
        {
            if (data == null || data.Length == 0 || data[0] == null)
                return 0;

            int samples = data[0].Length;

            // Rising Edge (type 0) or Falling Edge (type 1)
            if (type == 0 || type == 1)
            {
                bool expectedBefore = (type == 0) ? false : true; // Rising: low->high, Falling: high->low
                bool expectedAfter = (type == 0) ? true : false;

                for (int i = 1; i < samples; i++)
                {
                    if (data[channel][i - 1] == expectedBefore && data[channel][i] == expectedAfter)
                    {
                        return i;
                    }
                }
            }
            // Pattern (type 2) - For simplicity, we'll look for a specific pattern (e.g., alternating 0s and 1s)
            else if (type == 2)
            {
                for (int i = 1; i < samples - 1; i++)
                {
                    if (data[channel][i - 1] == false && data[channel][i] == true && data[channel][i + 1] == false)
                    {
                        return i;
                    }
                }
            }

            return 0; // Trigger not found
        }
    }
}