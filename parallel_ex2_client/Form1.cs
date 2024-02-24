using System.IO.Pipes;

namespace parallel_ex2_client
{
    public partial class Form1 : Form
    {
        private NamedPipeClientStream pipe = new NamedPipeClientStream(".", "testpipe", PipeDirection.InOut, PipeOptions.Asynchronous);

        public Form1()
        {
            InitializeComponent();
        }

        private void Form1_Load(object sender, EventArgs e)
        {
            pipe.Connect();

            new Task(async () =>
            {
                StreamReader sr = new StreamReader(pipe);
                while (pipe.IsConnected)
                {
                    string? msg = await sr.ReadLineAsync();
                    if (msg != null) {
                        Console.WriteLine(msg);

                        richTextBox1.Invoke(() =>
                        {
                            richTextBox1.Text += $"\n{msg}";
                        });
                    }
                }
            }).Start();
        }

        private void button1_Click(object sender, EventArgs e)
        {
            var msg = textBox1.Text;

            new Task(async () =>
            {
                StreamWriter sw = new StreamWriter(pipe);
                sw.AutoFlush = true;
                sw.WriteLine(msg);
            }).Start();

            textBox1.Text = "";
        }
    }
}
