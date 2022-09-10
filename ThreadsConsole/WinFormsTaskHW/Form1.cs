using System.Net;
using System.Runtime.InteropServices;
using Bogus;

namespace WinFormsTaskHW
{
    public partial class Form1 : Form
    {
        private object thisLock = new object();
        public Form1()
        {
            InitializeComponent();
        }

        private void btnPerform_Click(object sender, EventArgs e)
        {
            if (String.IsNullOrWhiteSpace(txtNumber.Text)) return;
            try
            {
                int count = int.Parse(txtNumber.Text);
                btnPerform.Enabled = false;
                progressBar.Value = 0;
                progressBar.Maximum = count;
                lbCounter.Text = $"0/{count}";
                Task task = new Task(() => DownloadPhotos(count));
                task.Start();
            }
            catch
            {
                return;
            }
        }

        private void DownloadPhotos(int count)
        {
            Faker faker = new Faker();
            int j = 0;
            int progress = 0;
            for (int i = 0; i < count; i++)
            {
                using (WebClient client = new WebClient())
                {
                    string fileName = Path.GetRandomFileName();
                    string path = Directory.GetCurrentDirectory() + $"\\Images\\{fileName}.png";
                    client.DownloadFile(new Uri(faker.Image.LoremFlickrUrl()), path);
                }
                if (lbCounter.InvokeRequired)
                {
                    string text = $"{++j}/{count}";
                    lbCounter.Invoke(new MethodInvoker(delegate { lbCounter.Text = text; }));
                }
                if (progressBar.InvokeRequired)
                {
                    progress += 1;
                    progressBar.Invoke(new MethodInvoker(delegate { progressBar.Value = progress; }));
                }
            }
            btnPerform.Enabled = true;
        }
    }
}