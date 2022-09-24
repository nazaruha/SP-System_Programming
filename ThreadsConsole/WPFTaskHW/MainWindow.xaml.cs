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
        private CancellationTokenSource ctSourcePhoto;
        private CancellationTokenSource ctSourcePhoto2; // for label "lbPhotoDownloading" view
        private CancellationTokenSource ctSourceUser;
        private CancellationTokenSource ctSourceUser2; // for label "lbUserDownloading" view
        private CancellationToken token;  // треба для скасування любого потока який захочем
        private CancellationToken token2;
        private CancellationToken token3;
        private CancellationToken token4;
        private Task photoDownloadTask;
        private Task lbPhotoDownloadTask; // for label "lbPhotoDownloading" view
        private Task userDownloadTask;
        private Task lbUserDownloadTask; // for label "lbUserDownloading" view
        private static ManualResetEvent photoDownloadMRE = new ManualResetEvent(false); // Вміє лочить потік. Тіпа ставити на паузу
        private static ManualResetEvent lbPhotoDownloadMRE = new ManualResetEvent(false); // Вміє лочить потік. Тіпа ставити на паузу
        private static ManualResetEvent userDownloadMRE = new ManualResetEvent(false); // Вміє лочить потік. Тіпа ставити на паузу
        private static ManualResetEvent lbUserDownloadMRE = new ManualResetEvent(false); // Вміє лочить потік. Тіпа ставити на паузу
        private bool isPausedPhoto = false;
        private bool isPausedUser = false;
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
            string path = Directory.GetCurrentDirectory() + "\\Photo Images";
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
            this.Dispatcher.Invoke(() => { lbPhotoDownloading.Visibility = Visibility.Visible; });
            this.Dispatcher.Invoke(() => { txtPhotoCounter.IsEnabled = false; });
            this.Dispatcher.Invoke(() => { btnPhotoCancel.IsEnabled = true; });
            this.Dispatcher.Invoke(() => { btnPhotoPause.IsEnabled = true; });
            this.Dispatcher.Invoke(() => { btnPhotoDownload.IsEnabled = false; });
            ctSourcePhoto = new CancellationTokenSource();
            ctSourcePhoto2 = new CancellationTokenSource();
            token = ctSourcePhoto.Token;
            token2 = ctSourcePhoto2.Token;
            photoCount = int.Parse(txtPhotoCounter.Text);

            photoDownloadTask = new Task(() => DownloadImages(photoCount), token);
            photoDownloadTask.Start();
            photoDownloadMRE.Set();

            lbPhotoDownloadTask = new Task(() => lbPhotoDownloadView(), token2);
            lbPhotoDownloadTask.Start();
            lbPhotoDownloadMRE.Set();
            
        }

        private void btnUserDownload_Click(object sender, RoutedEventArgs e)
        {
            this.Dispatcher.Invoke(() => { btnUserDownload.IsEnabled = false; });
            this.Dispatcher.Invoke(() => { btnUserPause.IsEnabled = true; });
            this.Dispatcher.Invoke(() => { btnUserCancel.IsEnabled = true; });
            this.Dispatcher.Invoke(() => { txtUserCounter.IsEnabled = false; });
            this.Dispatcher.Invoke(() => { lbUserDownloading.Visibility = Visibility.Visible; });
            ctSourceUser = new CancellationTokenSource();
            ctSourceUser2 = new CancellationTokenSource();
            token3 = ctSourceUser.Token;
            token4 = ctSourceUser2.Token;
            UserService userService = new UserService();
            userDownloadMRE.Set();
            userService.AddCurrentUserEvent += UserService_AddCurrentUserEvent; // так ми робим підсику
            userService.InsertRandomUserAsync(userCount, userDownloadMRE, token3); // на ружу повідомляємо скільки користувачів буде заінсерчено
        }

        private void UserService_AddCurrentUserEvent(int count, ManualResetEvent manualResetEvent, CancellationToken token)
        {
            // і тут ми на UI можем виводити інформацію ту, що робиться в сервісі
            if (token.IsCancellationRequested)
            {
                ResetUserForm(count);
                return;
            }
            else if (count == userCount)
            {
                ResetUserForm(count);
                return;
            }
            this.Dispatcher.Invoke(() => { userProgressBar.Value += 1; });
            this.Dispatcher.Invoke(() => { lbUserCounter.Content = $"{count}/{userCount}"; });
        }

        private void lbPhotoDownloadView()
        {
            while (true)
            {
                lbPhotoDownloadMRE.WaitOne(Timeout.Infinite);
                if (token.IsCancellationRequested)
                {
                    ctSourcePhoto2.Cancel();
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
            if (isPausedPhoto)
            {
                this.Dispatcher.Invoke(() => { res = MessageBox.Show("Are you sure?", "Cancel", MessageBoxButton.YesNo, MessageBoxImage.Stop); });
                if (res == MessageBoxResult.No)
                {
                    lbPhotoDownloadMRE.Set();
                    return;
                }
                btnPhotoDownload.IsEnabled = true;
                photoDownloadMRE.Set();
                isPausedPhoto = !isPausedPhoto;
                ctSourcePhoto.Cancel();
            }
            else
            {
                photoDownloadMRE.Reset();
                lbPhotoDownloadMRE.Reset();
                isPausedPhoto = !isPausedPhoto;
                this.Dispatcher.Invoke(() => { res = MessageBox.Show("Are you sure?", "Cancel", MessageBoxButton.YesNo, MessageBoxImage.Stop); });
                photoDownloadMRE.Set();
                lbPhotoDownloadMRE.Reset();
                isPausedPhoto = !isPausedPhoto;
                if (res == MessageBoxResult.No) return;
                ctSourcePhoto.Cancel();
            }

        }

        private void btnUserCancel_Click(object sender, RoutedEventArgs e)
        {
            MessageBoxResult res = MessageBoxResult.Yes;
            if (isPausedUser)
            {
                this.Dispatcher.Invoke(() => { res = MessageBox.Show("Are you sure?", "Cancel", MessageBoxButton.YesNo, MessageBoxImage.Stop); });
                if (res == MessageBoxResult.No)
                {
                    lbUserDownloadMRE.Set();
                    return;
                }
                btnUserDownload.IsEnabled = true;
                userDownloadMRE.Set();
                isPausedUser = !isPausedUser;
                ctSourceUser.Cancel();
            }
            else
            {
                userDownloadMRE.Reset();
                lbUserDownloadMRE.Reset();
                isPausedUser = !isPausedUser;
                this.Dispatcher.Invoke(() => { res = MessageBox.Show("Are you sure?", "Cancel", MessageBoxButton.YesNo, MessageBoxImage.Stop); });
                userDownloadMRE.Set();
                lbUserDownloadMRE.Reset();
                isPausedUser = !isPausedUser;
                if (res == MessageBoxResult.No) return;
                ctSourceUser.Cancel();
            }
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
                        string path = Directory.GetCurrentDirectory() + $"\\Photo Images\\{fileName}.png";
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

        private void ResetUserForm(int count)
        {
            this.Dispatcher.Invoke(() => { lbUserDownloading.Visibility = Visibility.Hidden; });
            this.Dispatcher.Invoke(() => { txtUserCounter.IsEnabled = true; });
            this.Dispatcher.Invoke(() => { btnUserCancel.IsEnabled = false; });
            this.Dispatcher.Invoke(() => { btnUserPause.IsEnabled = false; });
            this.Dispatcher.Invoke(() => { btnUserPause.Content = "Pause"; });
            this.Dispatcher.Invoke(() => { btnUserDownload.IsEnabled = true; });
            this.Dispatcher.Invoke(() => { lbUserCounter.Content = "0/0"; });
            this.Dispatcher.Invoke(() => { userProgressBar.Value = 0; });
            this.Dispatcher.Invoke(() => { txtUserCounter.Clear(); });
            MessageBox.Show($"{count} of {this.userCount} users are successfully added to database!", "SUCCESS DOWNLOAD", MessageBoxButton.OK, MessageBoxImage.Exclamation);
        }

        private void btnPhotoPause_Click(object sender, RoutedEventArgs e)
        {
            if (isPausedPhoto) // Якщо потік був залочений
            {
                photoDownloadMRE.Set(); // Пускаємо потік далі, змінюємо кнопку 
                this.Dispatcher.Invoke(() => { btnPhotoPause.Content = "Pause"; });
            }
            else
            {
                photoDownloadMRE.Reset(); // Залочити потік
                this.Dispatcher.Invoke(() => { btnPhotoPause.Content = "Continue"; });
            }
            isPausedPhoto = !isPausedPhoto;
        }

        private void btnUserPause_Click(object sender, RoutedEventArgs e)
        {
            if (isPausedUser)
            {
                userDownloadMRE.Set();
                this.Dispatcher.Invoke(() => { btnUserPause.Content = "Pause"; });
            }
            else
            {
                userDownloadMRE.Reset(); // Залочити потік
                this.Dispatcher.Invoke(() => { btnUserPause.Content = "Continue"; });
            }
            isPausedUser = !isPausedUser;
        }
    }
}
