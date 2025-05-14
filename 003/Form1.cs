using System;
using System.Drawing;
using System.Windows.Forms;
using System.Linq;

namespace LogicAnalyzerSim
{
    public partial class Form1 : Form
    {
        private LogicAnalyzer analyzer;
        private Timer timer;
        private const int CHANNELS = 8;
        private const int SAMPLES = 1000;
        private bool isRunning = false;
        private Trigger trigger;
        private double zoomLevel = 1.0;
        private int scrollOffset = 0;
        private int cursor1 = -1;
        private int cursor2 = -1;
        private PictureBox fftPictureBox;
        private PictureBox eyeDiagramPictureBox;
        private TextBox decodePanel;
        private TextBox metricsPanel;
        private TextBox glitchPanel;

        public Form1()
        {
            InitializeComponent();
            analyzer = new LogicAnalyzer(CHANNELS, SAMPLES);
            trigger = new Trigger(CHANNELS);
            timer = new Timer { Interval = 100 };
            timer.Tick += Timer_Tick;
            DoubleBuffered = true;
            this.BackColor = Color.Black;
            this.MaximizeBox = false;
        }

        private void InitializeComponent()
        {
            this.Size = new Size(1050, 840);

            var startButton = new Button
            {
                Text = "Start",
                Location = new Point(10, 10),
                Size = new Size(75, 25),
                BackColor = Color.FromArgb(128, 128, 128),
                ForeColor = Color.White
            };
            startButton.Click += StartButton_Click;

            var stopButton = new Button
            {
                Text = "Stop",
                Location = new Point(90, 10),
                Size = new Size(75, 25),
                BackColor = Color.FromArgb(128, 128, 128),
                ForeColor = Color.White
            };
            stopButton.Click += StopButton_Click;

            var resetButton = new Button
            {
                Text = "Reset",
                Location = new Point(170, 10),
                Size = new Size(75, 25),
                BackColor = Color.FromArgb(128, 128, 128),
                ForeColor = Color.White
            };
            resetButton.Click += ResetButton_Click;

            var sampleRateLabel = new Label
            {
                Text = "Sample Rate (Hz):",
                Location = new Point(250, 12),
                Size = new Size(100, 20),
                BackColor = Color.Black,
                ForeColor = Color.White
            };

            var sampleRateTextBox = new TextBox
            {
                Location = new Point(350, 10),
                Size = new Size(60, 20),
                Text = "1000",
                Font = new Font("MS Sans Serif", 8.25F),
                BackColor = Color.FromArgb(64, 64, 64),
                ForeColor = Color.White
            };
            sampleRateTextBox.TextChanged += (s, e) =>
            {
                if (int.TryParse(sampleRateTextBox.Text, out int rate) && rate > 0)
                    analyzer.SampleRate = rate;
            };

            var triggerChannelLabel = new Label
            {
                Text = "Trigger Channel:",
                Location = new Point(420, 12),
                Size = new Size(100, 20),
                BackColor = Color.Black,
                ForeColor = Color.White
            };

            var triggerChannelBox = new ComboBox
            {
                Location = new Point(520, 10),
                Size = new Size(50, 20),
                Font = new Font("MS Sans Serif", 8.25F),
                BackColor = Color.FromArgb(64, 64, 64),
                ForeColor = Color.White
            };
            for (int i = 0; i < CHANNELS; i++) triggerChannelBox.Items.Add(i);
            triggerChannelBox.SelectedIndex = 0;
            triggerChannelBox.SelectedIndexChanged += (s, e) =>
            {
                trigger.SetChannel(triggerChannelBox.SelectedIndex);
            };

            var triggerTypeBox = new ComboBox
            {
                Location = new Point(580, 10),
                Size = new Size(100, 20),
                Font = new Font("MS Sans Serif", 8.25F),
                BackColor = Color.FromArgb(64, 64, 64),
                ForeColor = Color.White
            };
            triggerTypeBox.Items.AddRange(new[] { "Rising Edge", "Falling Edge", "Pattern" });
            triggerTypeBox.SelectedIndex = 0;
            triggerTypeBox.SelectedIndexChanged += (s, e) =>
            {
                trigger.SetTriggerType(triggerTypeBox.SelectedIndex);
            };

            var zoomInButton = new Button
            {
                Text = "Zoom In",
                Location = new Point(690, 10),
                Size = new Size(75, 25),
                BackColor = Color.FromArgb(128, 128, 128),
                ForeColor = Color.White
            };
            zoomInButton.Click += (s, e) =>
            {
                zoomLevel *= 1.2;
                Invalidate();
            };

            var zoomOutButton = new Button
            {
                Text = "Zoom Out",
                Location = new Point(770, 10),
                Size = new Size(75, 25),
                BackColor = Color.FromArgb(128, 128, 128),
                ForeColor = Color.White
            };
            zoomOutButton.Click += (s, e) =>
            {
                zoomLevel /= 1.2;
                if (zoomLevel < 0.1) zoomLevel = 0.1;
                Invalidate();
            };

            var scrollLeftButton = new Button
            {
                Text = "◄",
                Location = new Point(850, 10),
                Size = new Size(35, 25),
                BackColor = Color.FromArgb(128, 128, 128),
                ForeColor = Color.White
            };
            scrollLeftButton.Click += (s, e) =>
            {
                scrollOffset -= 50;
                if (scrollOffset < 0) scrollOffset = 0;
                Invalidate();
            };

            var scrollRightButton = new Button
            {
                Text = "►",
                Location = new Point(890, 10),
                Size = new Size(35, 25),
                BackColor = Color.FromArgb(128, 128, 128),
                ForeColor = Color.White
            };
            scrollRightButton.Click += (s, e) =>
            {
                scrollOffset += 50;
                Invalidate();
            };

            var cursor1Label = new Label
            {
                Text = "Cursor 1:",
                Location = new Point(930, 12),
                Size = new Size(50, 20),
                BackColor = Color.Black,
                ForeColor = Color.White
            };

            var cursor1TextBox = new TextBox
            {
                Name = "cursor1TextBox",
                Location = new Point(980, 10),
                Size = new Size(50, 20),
                Text = "0",
                Font = new Font("MS Sans Serif", 8.25F),
                BackColor = Color.FromArgb(64, 64, 64),
                ForeColor = Color.White
            };
            cursor1TextBox.TextChanged += (s, e) =>
            {
                if (int.TryParse(cursor1TextBox.Text, out int pos))
                {
                    cursor1 = pos;
                    Invalidate();
                }
            };

            var cursor2Label = new Label
            {
                Text = "Cursor 2:",
                Location = new Point(930, 40),
                Size = new Size(50, 20),
                BackColor = Color.Black,
                ForeColor = Color.White
            };

            var cursor2TextBox = new TextBox
            {
                Name = "cursor2TextBox",
                Location = new Point(980, 38),
                Size = new Size(50, 20),
                Text = "0",
                Font = new Font("MS Sans Serif", 8.25F),
                BackColor = Color.FromArgb(64, 64, 64),
                ForeColor = Color.White
            };
            cursor2TextBox.TextChanged += (s, e) =>
            {
                if (int.TryParse(cursor2TextBox.Text, out int pos))
                {
                    cursor2 = pos;
                    Invalidate();
                }
            };

            var fftChannelLabel = new Label
            {
                Text = "FFT Channel:",
                Location = new Point(10, 40),
                Size = new Size(80, 20),
                BackColor = Color.Black,
                ForeColor = Color.White
            };

            var fftChannelBox = new ComboBox
            {
                Location = new Point(90, 38),
                Size = new Size(50, 20),
                Font = new Font("MS Sans Serif", 8.25F),
                BackColor = Color.FromArgb(64, 64, 64),
                ForeColor = Color.White
            };
            for (int i = 0; i < CHANNELS; i++) fftChannelBox.Items.Add(i);
            fftChannelBox.SelectedIndex = 0;
            fftChannelBox.SelectedIndexChanged += (s, e) => UpdateFFTDisplay();

            var metricsLabel = new Label
            {
                Text = "Signal Metrics & Statistics:",
                Location = new Point(10, 450),
                Size = new Size(150, 20),
                BackColor = Color.Black,
                ForeColor = Color.White
            };

            metricsPanel = new TextBox
            {
                Name = "metricsPanel",
                Location = new Point(10, 470),
                Size = new Size(480, 150),
                Multiline = true,
                ReadOnly = true,
                ScrollBars = ScrollBars.Vertical,
                BackColor = Color.FromArgb(32, 32, 32),
                ForeColor = Color.Lime,
                Font = new Font("MS Sans Serif", 8.25F)
            };

            var glitchLabel = new Label
            {
                Text = "Detected Glitches:",
                Location = new Point(500, 450),
                Size = new Size(150, 20),
                BackColor = Color.Black,
                ForeColor = Color.White
            };

            glitchPanel = new TextBox
            {
                Name = "glitchPanel",
                Location = new Point(500, 470),
                Size = new Size(470, 150),
                Multiline = true,
                ReadOnly = true,
                ScrollBars = ScrollBars.Vertical,
                BackColor = Color.FromArgb(32, 32, 32),
                ForeColor = Color.Lime,
                Font = new Font("MS Sans Serif", 8.25F)
            };

            var fftLabel = new Label
            {
                Text = "FFT/DFT Spectrum:",
                Location = new Point(10, 620),
                Size = new Size(150, 20),
                BackColor = Color.Black,
                ForeColor = Color.White
            };

            fftPictureBox = new PictureBox
            {
                Location = new Point(10, 640),
                Size = new Size(480, 150),
                BackColor = Color.FromArgb(32, 32, 32),
                BorderStyle = BorderStyle.FixedSingle
            };

            var eyeDiagramLabel = new Label
            {
                Text = "Eye Diagram:",
                Location = new Point(500, 620),
                Size = new Size(150, 20),
                BackColor = Color.Black,
                ForeColor = Color.White
            };

            eyeDiagramPictureBox = new PictureBox
            {
                Name = "eyeDiagramPictureBox",
                Location = new Point(500, 640),
                Size = new Size(470, 150),
                BackColor = Color.FromArgb(32, 32, 32),
                BorderStyle = BorderStyle.FixedSingle
            };

            var decodeLabel = new Label
            {
                Text = "Decoded UART Data:",
                Location = new Point(10, 300),
                Size = new Size(150, 20),
                BackColor = Color.Black,
                ForeColor = Color.White
            };

            decodePanel = new TextBox
            {
                Name = "decodePanel",
                Location = new Point(10, 320),
                Size = new Size(960, 120),
                Multiline = true,
                ReadOnly = true,
                ScrollBars = ScrollBars.Vertical,
                BackColor = Color.FromArgb(32, 32, 32),
                ForeColor = Color.Lime,
                Font = new Font("MS Sans Serif", 8.25F)
            };

            this.Controls.AddRange(new Control[]
            {
                startButton, stopButton, resetButton, sampleRateLabel, sampleRateTextBox,
                triggerChannelLabel, triggerChannelBox, triggerTypeBox,
                zoomInButton, zoomOutButton, scrollLeftButton, scrollRightButton,
                cursor1Label, cursor1TextBox, cursor2Label, cursor2TextBox,
                fftChannelLabel, fftChannelBox,
                metricsLabel, metricsPanel,
                glitchLabel, glitchPanel,
                fftLabel, fftPictureBox,
                eyeDiagramLabel, eyeDiagramPictureBox,
                decodeLabel, decodePanel
            });
            this.Paint += Form1_Paint;
        }

        private void StartButton_Click(object sender, EventArgs e)
        {
            if (!isRunning)
            {
                analyzer.SetTrigger(trigger);
                analyzer.Start();
                timer.Start();
                isRunning = true;
            }
        }

        private void StopButton_Click(object sender, EventArgs e)
        {
            if (isRunning)
            {
                analyzer.Stop();
                timer.Stop();
                isRunning = false;
                Invalidate();
            }
        }

        private void ResetButton_Click(object sender, EventArgs e)
        {
            analyzer.Stop();
            timer.Stop();
            isRunning = false;
            zoomLevel = 1.0;
            scrollOffset = 0;
            cursor1 = -1;
            cursor2 = -1;
            decodePanel.Text = "";
            metricsPanel.Text = "";
            glitchPanel.Text = "";
            eyeDiagramPictureBox.Image = null;
            var cursor1TextBox = (TextBox)Controls.Find("cursor1TextBox", true).First();
            var cursor2TextBox = (TextBox)Controls.Find("cursor2TextBox", true).First();
            cursor1TextBox.Text = "0";
            cursor2TextBox.Text = "0";
            fftPictureBox.Image = null;
            analyzer = new LogicAnalyzer(CHANNELS, SAMPLES);
            trigger = new Trigger(CHANNELS);
            Invalidate();
        }

        private void Timer_Tick(object sender, EventArgs e)
        {
            var decodedData = analyzer.GetDecodedData();
            var metricsData = analyzer.GetMetricsData();
            var glitchData = analyzer.GetGlitchData();
            var fftChannelBox = Controls.OfType<ComboBox>().Skip(1).First();
            var eyeDiagramBitmap = analyzer.GetEyeDiagramData(fftChannelBox.SelectedIndex, eyeDiagramPictureBox.Width, eyeDiagramPictureBox.Height);

            decodePanel.Text = string.Join(Environment.NewLine, decodedData);
            metricsPanel.Text = string.Join(Environment.NewLine, metricsData);
            glitchPanel.Text = string.Join(Environment.NewLine, glitchData);
            eyeDiagramPictureBox.Image = eyeDiagramBitmap;

            UpdateFFTDisplay();
            Invalidate();
        }

        private void UpdateFFTDisplay()
        {
            var fftChannelBox = Controls.OfType<ComboBox>().Skip(1).First();
            int channel = fftChannelBox.SelectedIndex;
            var fftData = analyzer.GetFFTResults(channel);
            if (fftData == null || fftData.Count <= 1)
            {
                fftPictureBox.Image = null;
                return;
            }

            int width = fftPictureBox.Width;
            int height = fftPictureBox.Height;
            Bitmap bmp = new Bitmap(width, height);
            using (Graphics g = Graphics.FromImage(bmp))
            {
                g.Clear(Color.FromArgb(32, 32, 32));

                double[] frequencies = new double[fftData.Count - 1];
                double[] magnitudes = new double[fftData.Count - 1];
                double maxMagnitude = 0;
                double maxFrequency = 0;

                for (int i = 1; i < fftData.Count; i++)
                {
                    string line = fftData[i];
                    int freqStart = line.IndexOf("Freq:") + 5;
                    int freqEnd = line.IndexOf("Hz,");
                    int magStart = line.IndexOf("Magnitude:") + 10;

                    if (freqStart < 0 || freqEnd < 0 || magStart < 0) continue;

                    string freqStr = line.Substring(freqStart, freqEnd - freqStart).Trim();
                    string magStr = line.Substring(magStart).Trim();

                    if (double.TryParse(freqStr, out double freq) && double.TryParse(magStr, out double mag))
                    {
                        frequencies[i - 1] = freq;
                        magnitudes[i - 1] = mag;
                        maxMagnitude = Math.Max(maxMagnitude, mag);
                        maxFrequency = Math.Max(maxFrequency, freq);
                    }
                }

                if (maxMagnitude == 0 || maxFrequency == 0)
                {
                    fftPictureBox.Image = null;
                    return;
                }

                Pen axisPen = new Pen(Color.White);
                g.DrawLine(axisPen, 30, height - 30, width - 10, height - 30); // X-axis
                g.DrawLine(axisPen, 30, 10, 30, height - 30); // Y-axis

                g.DrawString("Freq (Hz)", Font, Brushes.White, width / 2 - 30, height - 20);
                g.DrawString("Mag", Font, Brushes.White, 5, height / 2 - 10);

                int numTicks = 5;
                for (int i = 0; i <= numTicks; i++)
                {
                    float x = 30 + i * (width - 40) / numTicks;
                    float freq = (float)(i * maxFrequency / numTicks);
                    g.DrawLine(axisPen, x, height - 30, x, height - 25);
                    g.DrawString($"{freq:F0}", Font, Brushes.White, x - 10, height - 15);
                }

                for (int i = 0; i <= numTicks; i++)
                {
                    float y = height - 30 - i * (height - 40) / numTicks;
                    float mag = (float)(i * maxMagnitude / numTicks);
                    g.DrawLine(axisPen, 25, y, 30, y);
                    g.DrawString($"{mag:F1}", Font, Brushes.White, 0, y - 5);
                }

                Pen fftPen = new Pen(Color.Lime);
                float barWidth = (width - 40) / (float)frequencies.Length;
                for (int i = 0; i < frequencies.Length; i++)
                {
                    float x = 30 + i * barWidth;
                    float barHeight = (float)(magnitudes[i] / maxMagnitude * (height - 40));
                    float y = height - 30 - barHeight;
                    g.DrawLine(fftPen, x, height - 30, x, y);
                }

                axisPen.Dispose();
                fftPen.Dispose();
            }

            fftPictureBox.Image = bmp;
        }

        private void Form1_Paint(object sender, PaintEventArgs e)
        {
            Graphics g = e.Graphics;
            g.Clear(Color.Black);

            Pen gridPen = new Pen(Color.DarkGray);
            for (int x = 50; x < Width; x += (int)(50 * zoomLevel))
                g.DrawLine(gridPen, x, 50, x, 280);
            for (int y = 50; y < 280; y += 50)
                g.DrawLine(gridPen, 50, y, Width, y);
            gridPen.Dispose();

            int yOffset = 50;
            int channelHeight = 30;
            var glitchData = analyzer.GetGlitchData();
            Pen signalPen = new Pen(Color.White, 2);
            Pen glitchPen = new Pen(Color.Yellow, 2);
            for (int ch = 0; ch < CHANNELS; ch++)
            {
                bool[] data = analyzer.GetChannelData(ch);
                int yBase = yOffset + ch * channelHeight;
                g.DrawString($"CH{ch}", Font, Brushes.White, 10, yBase + 5);

                for (int i = 1; i < data.Length; i++)
                {
                    int x1 = 50 + (int)(((i - 1 - scrollOffset) * (Width - 100) / data.Length) * zoomLevel);
                    int x2 = 50 + (int)(((i - scrollOffset) * (Width - 100) / data.Length) * zoomLevel);
                    if (x1 < 50 || x2 > Width) continue;
                    int y1 = yBase + (data[i - 1] ? 0 : channelHeight - 10);
                    int y2 = yBase + (data[i] ? 0 : channelHeight - 10);

                    bool isGlitch = glitchData.Any(glitch => glitch.Contains($"Channel {ch}") && glitch.Contains($"at sample {i}"));
                    Pen pen = isGlitch ? glitchPen : signalPen;

                    if (data[i - 1] != data[i])
                        g.DrawLine(pen, x2, y1, x2, y2);
                    g.DrawLine(pen, x1, y1, x2, y2);
                }
            }
            signalPen.Dispose();
            glitchPen.Dispose();

            if (cursor1 >= 0)
            {
                int x1 = 50 + (int)(((cursor1 - scrollOffset) * (Width - 100) / SAMPLES) * zoomLevel);
                if (x1 >= 50 && x1 <= Width)
                    g.DrawLine(Pens.Red, x1, 50, x1, 280);
            }
            if (cursor2 >= 0)
            {
                int x2 = 50 + (int)(((cursor2 - scrollOffset) * (Width - 100) / SAMPLES) * zoomLevel);
                if (x2 >= 50 && x2 <= Width)
                    g.DrawLine(Pens.Blue, x2, 50, x2, 280);
            }

            if (cursor1 >= 0 && cursor2 >= 0)
            {
                double timeDiff = Math.Abs(cursor1 - cursor2) / (double)analyzer.SampleRate;
                g.DrawString($"Δt: {timeDiff:F6}s", Font, Brushes.White, 10, 290);
            }
        }
    }
}