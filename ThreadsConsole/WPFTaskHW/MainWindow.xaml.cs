using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using System.Text.RegularExpressions;
using System.IO;
using System.Net;
using Bogus;
using Path = System.IO.Path;
using System.Threading;

namespace WPFTaskHW
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        private CancellationTokenSource ctSource;
        private CancellationTokenSource ctSource2; // for label "lbDownload" view
        private CancellationToken token;  // треба для скасування любого потока який захочем
        private CancellationToken token2;
        private Task task;
        private Task task2; // for label "lbDownload" view
        private bool isPaused = false;
        private int count = 0;
        public MainWindow()
        {
            InitializeComponent();
            checkImageForlder();
            lbDownloading.Visibility = Visibility.Hidden;
        }

        private void checkImageForlder()
        {
            string path = Directory.GetCurrentDirectory() + "\\Images";
            if (Directory.Exists(path)) return;
            Directory.CreateDirectory(path);
        }

        private void txtCounter_GotFocus(object sender, RoutedEventArgs e)
        {
            if (txtCounter.Text != "Input number...") return;
            txtCounter.Clear();
            txtCounter.Foreground = Brushes.Black;
        }

        private void txtCounter_LostFocus(object sender, RoutedEventArgs e)
        {
            if (!String.IsNullOrWhiteSpace(txtCounter.Text)) return;
            txtCounter.Foreground = Brushes.Gray;
            txtCounter.Text = "Input number...";
        }

        private void NumberValidationTextBox(object sender, TextCompositionEventArgs e)
        {
            Regex regex = new Regex("[^0-9]+");
            e.Handled = regex.IsMatch(e.Text);
        }

        private void Label_MouseDown(object sender, MouseButtonEventArgs e)
        {
            txtCounter.Focus();
        }

        private void txtCounter_TextChanged(object sender, TextChangedEventArgs e)
        {
            if (lbCounter == null) return;
            if (String.IsNullOrWhiteSpace(txtCounter.Text))
            {
                btnDownload.IsEnabled = false;
                lbCounter.Content = "0/0";
                return;
            }
            try
            {
                count = int.Parse(txtCounter.Text);
                if (count == 0) return;
                lbCounter.Content = $"0/{count}";
                progressBar.Maximum = count;
                btnDownload.IsEnabled = true;
            }
            catch
            {
                btnDownload.IsEnabled = false;
                lbCounter.Content = "0/0";
                return;
            }
            
        }

        private void btnDownload_Click(object sender, RoutedEventArgs e)
        {
            lbDownloading.Visibility = Visibility.Visible;
            txtCounter.IsEnabled = false;
            btnCancel.IsEnabled = true;
            btnPause.IsEnabled = true;
            btnDownload.IsEnabled = false;
            ctSource = new CancellationTokenSource();
            ctSource2 = new CancellationTokenSource();
            token = ctSource.Token;
            token2 = ctSource2.Token;
            count = int.Parse(txtCounter.Text);
            task = new Task(() => DownloadImages(count), token);
            task.Start();
            task2 = new Task(() => DownloadingView(), token2);
            task2.Start();
        }

        private void DownloadingView()
        {
            while (true)
            {
                if (token.IsCancellationRequested)
                {
                    ctSource2.Cancel();
                    return;
                }
                if (!isPaused)
                {
                    Thread.Sleep(500);
                    this.Dispatcher.Invoke(() => { lbDownloading.Content = "Downloading   "; });
                    Thread.Sleep(500);
                    this.Dispatcher.Invoke(() => { lbDownloading.Content = "Downloading.  "; });
                    Thread.Sleep(500);
                    this.Dispatcher.Invoke(() => { lbDownloading.Content = "Downloading.. "; });
                    Thread.Sleep(500);
                    this.Dispatcher.Invoke(() => { lbDownloading.Content = "Downloading..."; });
                }
            }
        }

        private void btnCancel_Click(object sender, RoutedEventArgs e)
        {
            MessageBoxResult res = MessageBoxResult.Yes;
            if (isPaused)
            {
                this.Dispatcher.Invoke(() => { res = MessageBox.Show("Are you sure?", "Cancel", MessageBoxButton.YesNo, MessageBoxImage.Stop); });
                if (res == MessageBoxResult.No) return;
                this.Dispatcher.Invoke(() => { isPaused = false; });
                ctSource.Cancel();
            }
            else
            {
                this.Dispatcher.Invoke(() => { isPaused = true; });
                this.Dispatcher.Invoke(() => { res = MessageBox.Show("Are you sure?", "Cancel", MessageBoxButton.YesNo, MessageBoxImage.Stop); });
                this.Dispatcher.Invoke(() => { isPaused = false; });
                if (res == MessageBoxResult.No) return;
                ctSource.Cancel();
            }
        }

        private void DownloadImages(int count)
        {
            Faker faker = new Faker();
            int j = 0;
            int progress = 0;
            for (int i = 0; i < count; i++)
            {
                if (isPaused) while (isPaused) { }
                Thread.Sleep(500);
                if (token.IsCancellationRequested)
                {
                    ResetForm(j);
                    return;
                }
                using (WebClient client = new WebClient())
                {
                    string fileName = Path.GetRandomFileName();
                    string path = Directory.GetCurrentDirectory() + $"\\Images\\{fileName}.png";
                    client.DownloadFile(new Uri(faker.Image.LoremFlickrUrl()), path);
                }
                this.Dispatcher.Invoke(() =>
                {
                    lbCounter.Content = $"{++j}/{txtCounter.Text}";
                });
                this.Dispatcher.Invoke(() =>
                {
                    progressBar.Value = ++progress;
                });
            }
            ResetForm(j);
        }

        private void ResetForm(int count)
        {
            this.Dispatcher.Invoke(() => { lbDownloading.Visibility = Visibility.Hidden; });
            this.Dispatcher.Invoke(() => { txtCounter.IsEnabled = true; });
            this.Dispatcher.Invoke(() => { btnCancel.IsEnabled = false; });
            this.Dispatcher.Invoke(() => { btnPause.IsEnabled = false; });
            this.Dispatcher.Invoke(() => { btnResume.IsEnabled = false; });
            this.Dispatcher.Invoke(() => { btnDownload.IsEnabled = true; });
            this.Dispatcher.Invoke(() => { lbCounter.Content = "0/0"; });
            this.Dispatcher.Invoke(() => { progressBar.Value = 0; });
            this.Dispatcher.Invoke(() => { txtCounter.Clear(); });
            MessageBox.Show($"{count} of {this.count} photos are successfully downloaded!", "SUCCESS DOWNLOAD", MessageBoxButton.OK, MessageBoxImage.Exclamation);
        }

        private void btnPause_Click(object sender, RoutedEventArgs e)
        {
            this.Dispatcher.Invoke(() => { btnResume.IsEnabled = true; });
            this.Dispatcher.Invoke(() => { btnPause.IsEnabled = false; });
            this.Dispatcher.Invoke(() => { isPaused = true; });
        }

        private void btnResume_Click(object sender, RoutedEventArgs e)
        {
            this.Dispatcher.Invoke(() => { btnResume.IsEnabled = false; });
            this.Dispatcher.Invoke(() => { btnPause.IsEnabled = true; });
            this.Dispatcher.Invoke(() => { isPaused = false; });
        }
    }
}
