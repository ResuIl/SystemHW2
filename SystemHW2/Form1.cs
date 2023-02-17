namespace SystemHW2
{
    public partial class Form1 : Form
    {
        bool CanAbort = false;
        private ManualResetEvent _suspendEvent = new ManualResetEvent(true);
        public Form1() => InitializeComponent();

        private void btn_From_Click(object sender, EventArgs e)
        {
            if (openFileDialog1.ShowDialog() == DialogResult.OK)
                textBox1.Text = openFileDialog1.FileName;
        }

        private void button1_Click(object sender, EventArgs e)
        {
            if (folderBrowserDialog1.ShowDialog() == DialogResult.OK)
                textBox2.Text = folderBrowserDialog1.SelectedPath;
        }

        private void btn_Copy_Click(object sender, EventArgs e)
        {
            Thread trd = new Thread(new ThreadStart(CopyTask));
            trd.IsBackground = true;
            trd.Start();
        }

        private void CopyTask()
        {
            CanAbort = false;
            var file = textBox1.Text.Split("\\");
            if (!File.Exists(textBox1.Text) && textBox2.Text.Length > 0)
                return;

            var len = 10;
            byte[] buffer = new byte[len];

            using (FileStream source = new FileStream(textBox1.Text, FileMode.Open, FileAccess.Read))
            {
                long fileLength = source.Length;
                using (FileStream dest = new FileStream(textBox2.Text + "\\" + file[file.Length - 1], FileMode.CreateNew, FileAccess.Write))
                {
                    long totalBytes = 0;
                    int currentBlockSize = 0;

                    while ((currentBlockSize = source.Read(buffer, 0, buffer.Length)) > 0)
                    {
                        _suspendEvent.WaitOne();  // Rucnoy olaraq sonsuz while da yaza bilerdim bura.
                        totalBytes += currentBlockSize;
                        double percentage = (double)totalBytes * 100.0 / fileLength;
                        dest.Write(buffer, 0, currentBlockSize);

                        UpdateProgressBar(Convert.ToInt32(percentage));
                        Thread.Sleep(5);

                        if (CanAbort)
                        {
                            dest.Close();
                            File.Delete(textBox2.Text + "\\" + file[file.Length - 1]);
                            MessageBox.Show("Aborted.");
                            UpdateProgressBar(0);
                            break;
                        }
                    }
                }
            }
        }

        private void UpdateProgressBar(int value)
        {
            if (InvokeRequired)
            {
                Invoke(new Action<int>(UpdateProgressBar), value);
                return;
            }

            progressBar1.Value = value;
        }

        private void btn_Abort_Click(object sender, EventArgs e)
        {
            CanAbort = true;
            _suspendEvent.Set();
        }

        private void btn_Suspend_Click(object sender, EventArgs e) => _suspendEvent.Reset();
        private void btn_Resume_Click(object sender, EventArgs e) => _suspendEvent.Set();
    }
}