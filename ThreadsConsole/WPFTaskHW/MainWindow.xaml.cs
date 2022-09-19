using System;
using Services;
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

/*
 *  public Task InsertRandomUserAsync(int count)
        {
            return Task.Run(() => InsertRandomUser(count));
        }
        //Додавання рандомних даних в БД
        public void InsertRandomUser(int count)
        {
            for (int i = 0; i < count; i++)
            {
                Thread.Sleep(100);
            }
        }
 */

namespace WPFTaskHW
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        private object thisLock = new object();
        private CancellationTokenSource ctSource;
        private CancellationTokenSource ctSource2; // for label "lbDownload" view
        private CancellationToken token;  // треба для скасування любого потока який захочем
        private CancellationToken token2;
        private Task photoDownloadTask;
        private Task lbPhotoDownloadTask; // for label "lbDownload" view
        private static ManualResetEvent photoDownloadMRE = new ManualResetEvent(false); // Вміє лочить потік. Тіпа ставити на паузу
        private static ManualResetEvent lbPhotoDownloadMRE = new ManualResetEvent(false); // Вміє лочить потік. Тіпа ставити на паузу
        private bool isPaused = false;
        private int photoCount = 0;
        private int userCount = 0;
        public MainWindow()
        {
            InitializeComponent();
            checkImageForlder();
            lbPhotoDownloading.Visibility = Visibility.Hidden;
            lbUserDownloading.Visibility = Visibility.Hidden;
        }

        private void checkImageForlder() // creates image directory if it doesn't exist
        {
            string path = Directory.GetCurrentDirectory() + "\\Images";
            if (Directory.Exists(path)) return;
            Directory.CreateDirectory(path);
        }

        private void txtCounter_GotFocus(object sender, RoutedEventArgs e)
        {
            if ((sender as TextBox).Name == "txtPhotoCounter")
            {
                if (txtPhotoCounter.Text != "Input number...") return;
                txtPhotoCounter.Clear();
                txtPhotoCounter.Foreground = Brushes.Black;
            }
            else if ((sender as TextBox).Name == "txtUserCounter")
            {
                if (txtUserCounter.Text != "Input number...") return;
                txtUserCounter.Clear();
                txtUserCounter.Foreground = Brushes.Black;
            }
            
        }

        private void txtCounter_LostFocus(object sender, RoutedEventArgs e)
        {
            if ((sender as TextBox).Name == "txtPhotoCounter")
            {
                if (!String.IsNullOrWhiteSpace(txtPhotoCounter.Text)) return;
                txtPhotoCounter.Foreground = Brushes.Gray;
                txtPhotoCounter.Text = "Input number...";
            }
            else if ((sender as TextBox).Name == "txtUserCounter")
            {
                if (!String.IsNullOrWhiteSpace(txtUserCounter.Text)) return;
                txtUserCounter.Foreground = Brushes.Gray;
                txtUserCounter.Text = "Input number...";
            }
        }

        private void NumberValidationTextBox(object sender, TextCompositionEventArgs e)
        {
            Regex regex = new Regex("[^0-9]+");
            e.Handled = regex.IsMatch(e.Text);
        }

        private void Label_MouseDown(object sender, MouseButtonEventArgs e)
        {
            if ((sender as Label).Name == "photoLabel")
                txtPhotoCounter.Focus();
            else if ((sender as Label).Name == "userLabel")
                txtUserCounter.Focus();
        }

        private void txtCounter_TextChanged(object sender, TextChangedEventArgs e)
        {
            if ((sender as TextBox).Name == "txtPhotoCounter")
            {
                if (lbPhotoCounter == null) return;
                if (String.IsNullOrWhiteSpace(txtPhotoCounter.Text))
                {
                    btnPhotoDownload.IsEnabled = false;
                    lbPhotoCounter.Content = "0/0";
                    return;
                }
                try
                {
                    photoCount = int.Parse(txtPhotoCounter.Text);
                    if (photoCount == 0) return;
                    lbPhotoCounter.Content = $"0/{photoCount}";
                    photoProgressBar.Maximum = photoCount;
                    btnPhotoDownload.IsEnabled = true;
                }
                catch
                {
                    btnPhotoDownload.IsEnabled = false;
                    lbPhotoCounter.Content = "0/0";
                    return;
                }
            }
            else if ((sender as TextBox).Name == "txtUserCounter")
            {
                if (lbUserCounter == null) return;
                if (String.IsNullOrWhiteSpace(txtUserCounter.Text))
                {
                    btnUserDownload.IsEnabled = false;
                    lbUserCounter.Content = "0/0";
                    return;
                }
                try
                {
                    userCount = int.Parse(txtUserCounter.Text);
                    if (userCount == 0) return;
                    lbUserCounter.Content = $"0/{userCount}";
                    userProgressBar.Maximum = userCount;
                    btnUserDownload.IsEnabled = true;
                }
                catch
                {
                    btnUserDownload.IsEnabled = false;
                    lbUserCounter.Content = "0/0";
                    return;
                }
            }
        }

        private void btnPhotoDownload_Click(object sender, RoutedEventArgs e)
        {
            lbPhotoDownloading.Visibility = Visibility.Visible;
            txtPhotoCounter.IsEnabled = false;
            btnPhotoCancel.IsEnabled = true;
            btnPhotoPause.IsEnabled = true;
            btnPhotoDownload.IsEnabled = false;
            ctSource = new CancellationTokenSource();
            ctSource2 = new CancellationTokenSource();
            token = ctSource.Token;
            token2 = ctSource2.Token;
            photoCount = int.Parse(txtPhotoCounter.Text);

            photoDownloadTask = new Task(() => DownloadImages(photoCount), token);
            photoDownloadTask.Start();
            photoDownloadMRE.Set();

            lbPhotoDownloadTask = new Task(() => lbDownloadView(), token2);
            lbPhotoDownloadTask.Start();
            lbPhotoDownloadMRE.Set();
            
        }

        private void btnUserDownload_Click(object sender, RoutedEventArgs e)
        {
            //User_Service.UserService userService = new User_Service.UserService();
            //userService.AddCurrentUserEvent += UserService_AddCurrentUserEvent; // так ми робим підписку
            //userService.InsertRandomUserAsync(100); // на ружу повідомляємо скільки користувачів буде заінсерчено
            UserService userService = new UserService();
            userService.AddCurrentUserEvent += UserService_AddCurrentUserEvent;
            userService.InsertRandomUserAsync(100);
        }

        private void UserService_AddCurrentUserEvent(int count)
        {
            // і тут ми на UI можем виводити інформацію ту, що робиться в сервісі
            this.Dispatcher.Invoke(() => { lbUserCounter.Content = $"{count}/{userCount}"; });
        }

        private void lbDownloadView()
        {
            while (true)
            {
                lbPhotoDownloadMRE.WaitOne(Timeout.Infinite);
                if (token.IsCancellationRequested)
                {
                    ctSource2.Cancel();
                    return;
                }
                Thread.Sleep(500);
                this.Dispatcher.Invoke(() => { lbPhotoDownloading.Content = "Downloading   "; });
                Thread.Sleep(500);
                this.Dispatcher.Invoke(() => { lbPhotoDownloading.Content = "Downloading.  "; });
                Thread.Sleep(500);
                this.Dispatcher.Invoke(() => { lbPhotoDownloading.Content = "Downloading.. "; });
                Thread.Sleep(500);
                this.Dispatcher.Invoke(() => { lbPhotoDownloading.Content = "Downloading..."; });
            }
        }

        private void btnPhotoCancel_Click(object sender, RoutedEventArgs e)
        {
            MessageBoxResult res = MessageBoxResult.Yes;
            if (isPaused)
            {
                this.Dispatcher.Invoke(() => { res = MessageBox.Show("Are you sure?", "Cancel", MessageBoxButton.YesNo, MessageBoxImage.Stop); });
                if (res == MessageBoxResult.No)
                {
                    lbPhotoDownloadMRE.Set();
                    return;
                }
                btnPhotoDownload.IsEnabled = true;
                photoDownloadMRE.Set();
                isPaused = !isPaused;
                ctSource.Cancel();
            }
            else
            {
                photoDownloadMRE.Reset();
                lbPhotoDownloadMRE.Reset();
                isPaused = !isPaused;
                this.Dispatcher.Invoke(() => { res = MessageBox.Show("Are you sure?", "Cancel", MessageBoxButton.YesNo, MessageBoxImage.Stop); });
                photoDownloadMRE.Set();
                lbPhotoDownloadMRE.Reset();
                isPaused = !isPaused;
                if (res == MessageBoxResult.No) return;
                ctSource.Cancel();
            }
        }

        private void btnUserCancel_Click(object sender, RoutedEventArgs e)
        {

        }

        private void DownloadImages(int count)
        {
            lock (thisLock)
            {
                Faker faker = new Faker();
                int j = 0;
                int progress = 0;
                for (int i = 0; i < count; i++)
                {
                    photoDownloadMRE.WaitOne(Timeout.Infinite); //Якщо був залочений потік то ми чекаємо поки його розлочать
                    Thread.Sleep(500);
                    if (token.IsCancellationRequested)
                    {
                        ResetPhotoForm(j);
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
                        lbPhotoCounter.Content = $"{++j}/{txtPhotoCounter.Text}";
                    });
                    this.Dispatcher.Invoke(() =>
                    {
                        photoProgressBar.Value = ++progress;
                    });
                }
                ResetPhotoForm(j);
            }
        }

        private void ResetPhotoForm(int count)
        {
            this.Dispatcher.Invoke(() => { lbPhotoDownloading.Visibility = Visibility.Hidden; });
            this.Dispatcher.Invoke(() => { txtPhotoCounter.IsEnabled = true; });
            this.Dispatcher.Invoke(() => { btnPhotoCancel.IsEnabled = false; });
            this.Dispatcher.Invoke(() => { btnPhotoPause.IsEnabled = false; });
            this.Dispatcher.Invoke(() => { btnPhotoPause.Content = "Pause"; });
            this.Dispatcher.Invoke(() => { btnPhotoDownload.IsEnabled = true; });
            this.Dispatcher.Invoke(() => { lbPhotoCounter.Content = "0/0"; });
            this.Dispatcher.Invoke(() => { photoProgressBar.Value = 0; });
            this.Dispatcher.Invoke(() => { txtPhotoCounter.Clear(); });
            MessageBox.Show($"{count} of {this.photoCount} photos are successfully downloaded!", "SUCCESS DOWNLOAD", MessageBoxButton.OK, MessageBoxImage.Exclamation);
        }

        private void btnPhotoPause_Click(object sender, RoutedEventArgs e)
        {
            if (isPaused) // Якщо потік був залочений
            {
                photoDownloadMRE.Set(); // Пускаємо потік далі, змінюємо кнопку 
                this.Dispatcher.Invoke(() => { btnPhotoPause.Content = "Pause"; });
            }
            else
            {
                photoDownloadMRE.Reset(); // Залочити потік
                this.Dispatcher.Invoke(() => { btnPhotoPause.Content = "Continue"; });
            }
            isPaused = !isPaused;
        }

        private void btnUserPause_Click(object sender, RoutedEventArgs e)
        {

        }
    }
}
