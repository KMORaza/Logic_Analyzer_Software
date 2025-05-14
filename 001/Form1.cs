using System;
using System.Drawing;
using System.Linq;
using System.Windows.Forms;

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

        public Form1()
        {
            InitializeComponent();
            analyzer = new LogicAnalyzer(CHANNELS, SAMPLES);
            trigger = new Trigger(CHANNELS);
            timer = new Timer { Interval = 100 };
            timer.Tick += Timer_Tick;
            DoubleBuffered = true;
            this.BackColor = Color.Black;
        }

        private void InitializeComponent()
        {
            this.Size = new Size(1000, 700);
            this.Font = new Font("MS Sans Serif", 8.25F);

            // Control Panel (Top Section)
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

            // Zoom Controls
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

            // Scroll Controls
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

            // Cursor Controls
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

            // Decode Panel with Label
            var decodeLabel = new Label
            {
                Text = "Decoded UART Data:",
                Location = new Point(10, 480),
                Size = new Size(150, 20),
                BackColor = Color.Black,
                ForeColor = Color.White
            };

            var decodePanel = new TextBox
            {
                Location = new Point(10, 500),
                Size = new Size(960, 150),
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
            var decodePanel = Controls.OfType<TextBox>().First();
            decodePanel.Text = "";
            var cursor1TextBox = Controls.OfType<TextBox>().Skip(1).First();
            var cursor2TextBox = Controls.OfType<TextBox>().Skip(2).First();
            cursor1TextBox.Text = "0";
            cursor2TextBox.Text = "0";
            analyzer = new LogicAnalyzer(CHANNELS, SAMPLES); // Reset analyzer
            trigger = new Trigger(CHANNELS); // Reset trigger
            Invalidate();
        }

        private void Timer_Tick(object sender, EventArgs e)
        {
            var decodedData = analyzer.GetDecodedData();
            var decodePanel = Controls.OfType<TextBox>().First();
            decodePanel.Text = string.Join(Environment.NewLine, decodedData);
            Invalidate();
        }

        private void Form1_Paint(object sender, PaintEventArgs e)
        {
            Graphics g = e.Graphics;
            g.Clear(Color.Black);

            // Draw grid
            using (Pen gridPen = new Pen(Color.DarkGray))
            {
                for (int x = 50; x < Width; x += (int)(50 * zoomLevel))
                    g.DrawLine(gridPen, x, 50, x, 450);
                for (int y = 50; y < 450; y += 50)
                    g.DrawLine(gridPen, 50, y, Width, y);
            }

            // Draw timing diagrams
            int yOffset = 50;
            int channelHeight = 50;
            using (Pen signalPen = new Pen(Color.White, 2))
            {
                for (int ch = 0; ch < CHANNELS; ch++)
                {
                    bool[] data = analyzer.GetChannelData(ch);
                    int yBase = yOffset + ch * channelHeight;
                    g.DrawString($"CH{ch}", Font, Brushes.White, 10, yBase + 10);

                    for (int i = 1; i < data.Length; i++)
                    {
                        int x1 = 50 + (int)(((i - 1 - scrollOffset) * (Width - 100) / data.Length) * zoomLevel);
                        int x2 = 50 + (int)(((i - scrollOffset) * (Width - 100) / data.Length) * zoomLevel);
                        if (x1 < 50 || x2 > Width) continue;
                        int y1 = yBase + (data[i - 1] ? 0 : channelHeight - 10);
                        int y2 = yBase + (data[i] ? 0 : channelHeight - 10);

                        if (data[i - 1] != data[i])
                            g.DrawLine(signalPen, x2, y1, x2, y2);
                        g.DrawLine(signalPen, x1, y1, x2, y2);
                    }
                }
            }

            // Draw cursors
            if (cursor1 >= 0)
            {
                int x1 = 50 + (int)(((cursor1 - scrollOffset) * (Width - 100) / SAMPLES) * zoomLevel);
                if (x1 >= 50 && x1 <= Width)
                    g.DrawLine(Pens.Red, x1, 50, x1, 450);
            }
            if (cursor2 >= 0)
            {
                int x2 = 50 + (int)(((cursor2 - scrollOffset) * (Width - 100) / SAMPLES) * zoomLevel);
                if (x2 >= 50 && x2 <= Width)
                    g.DrawLine(Pens.Blue, x2, 50, x2, 450);
            }

            // Display time difference
            if (cursor1 >= 0 && cursor2 >= 0)
            {
                double timeDiff = Math.Abs(cursor1 - cursor2) / (double)analyzer.SampleRate;
                g.DrawString($"Δt: {timeDiff:F6}s", Font, Brushes.White, 10, 470);
            }
        }
    }
}