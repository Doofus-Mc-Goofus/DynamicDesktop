using System;
using System.Windows;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using Microsoft.Win32;

namespace Dreamscene
{
    public partial class ControlPanel : Window
    {
        private readonly MainWindow MainWiw;
        private readonly bool firstTime = true;
        public ControlPanel(MainWindow mainWindow)
        {
            InitializeComponent();
            MainWiw = mainWindow;
            switch (mainWindow.HKCU_GetString(@"SOFTWARE\Dreamscene", "Fit"))
            {
                case "10":
                    FitDropdown.SelectedIndex = 0;
                    break;
                case "6":
                    FitDropdown.SelectedIndex = 1;
                    break;
                case "2":
                    FitDropdown.SelectedIndex = 2;
                    break;
                default:
                    FitDropdown.SelectedIndex = 3;
                    break;
            }
            if (mainWindow.HKCU_GetString(@"SOFTWARE\Dreamscene", "Startup") == "true")
            {
                StartupCheckbox.IsChecked = true;
            }
            SystemEvents.UserPreferenceChanged += (s, ee) => MetricUpdate();
            MetricUpdate();
            firstTime = false;
        }
        private void MetricUpdate()
        {
            System.Drawing.Color bg = System.Drawing.SystemColors.Control;
            System.Drawing.Color bg2 = System.Drawing.SystemColors.Desktop;
            Background = new SolidColorBrush(Color.FromArgb(bg.A, bg.R, bg.G, bg.B));
            DeskBG.Fill = new SolidColorBrush(Color.FromArgb(bg2.A, bg2.R, bg2.G, bg2.B));
        }
        public void Update(BitmapImage image)
        {
            preview.Source = image;
            switch (MainWiw.HKCU_GetString(@"SOFTWARE\Dreamscene", "Wallp"))
            {
                case "BM":
                    URL.Content = "None";
                    break;
                case "aur":
                    URL.Content = "Desktop Aurora by SaT";
                    break;
                default:
                    URL.Content = MainWiw.HKCU_GetString(@"SOFTWARE\Dreamscene", "Wallp");
                    break;
            }
        }

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            OpenFileDialog openDialog = new OpenFileDialog();
            _ = openDialog.ShowDialog();
            _ = MainWiw.Video.Source;
            try
            {
                double vidrat = MainWiw.Video.Width / MainWiw.Video.Height;
                MainWiw.Video.Source = new Uri(openDialog.FileName);
                MainWiw.HKCU_AddKey(@"SOFTWARE\Dreamscene", "Wallp", openDialog.FileName);
                string etg = openDialog.SafeFileName;
                if (etg[etg.Length - 1].ToString() == "/"
                    || (etg[etg.Length - 1].ToString() == "l" && etg[etg.Length - 2].ToString() == "m" && etg[etg.Length - 3].ToString() == "t" && etg[etg.Length - 4].ToString() == "h")
                    || (etg[etg.Length - 1].ToString() == "m" && etg[etg.Length - 2].ToString() == "t" && etg[etg.Length - 3].ToString() == "h"))
                {
                    MainWiw.HKCU_AddKey(@"SOFTWARE\Dreamscene", "ActiveDesktop", "true");
                }
                else
                {
                    MainWiw.HKCU_AddKey(@"SOFTWARE\Dreamscene", "ActiveDesktop", "false");
                }
                switch (MainWiw.HKCU_GetString(@"SOFTWARE\Dreamscene", "Fit"))
                {
                    case "10":
                        MainWiw.Video.Height = MainWiw.Video.Width / vidrat;
                        MainWiw.Video.VerticalAlignment = VerticalAlignment.Center;
                        break;
                    default:
                        MainWiw.Video.Height = double.NaN;
                        MainWiw.Video.VerticalAlignment = VerticalAlignment.Stretch;
                        break;
                }
            }
            catch
            {
                _ = MessageBox.Show("Couldn't apply file", "Error", MessageBoxButton.OK, MessageBoxImage.Exclamation);
            }
            GC.Collect();
        }

        private void FitDropdown_SelectionChanged(object sender, System.Windows.Controls.SelectionChangedEventArgs e)
        {
            switch (FitDropdown.SelectedIndex)
            {
                case 0:
                    MainWiw.HKCU_AddKey(@"SOFTWARE\Dreamscene", "Fit", "10");
                    break;
                case 1:
                    MainWiw.HKCU_AddKey(@"SOFTWARE\Dreamscene", "Fit", "6");
                    break;
                case 2:
                    MainWiw.HKCU_AddKey(@"SOFTWARE\Dreamscene", "Fit", "2");
                    break;
                default:
                    MainWiw.HKCU_AddKey(@"SOFTWARE\Dreamscene", "Fit", "0");
                    break;
            }
            if (!firstTime)
            {
                MainWiw.Update(false);
            }
        }

        private void StartupCheckbox_Checked(object sender, RoutedEventArgs e)
        {
            if (!firstTime)
            {
                MainWiw.HKCU_AddKey(@"SOFTWARE\Dreamscene", "Startup", "true");
                MainWiw.HKCU_AddKey(@"SOFTWARE\Microsoft\Windows\CurrentVersion\Run", "Dynamic Desktop", "\"" + System.Reflection.Assembly.GetExecutingAssembly().Location + "\" -hide 1");
            }
        }

        private void StartupCheckbox_Unchecked(object sender, RoutedEventArgs e)
        {
            if (!firstTime)
            {
                MainWiw.HKCU_AddKey(@"SOFTWARE\Dreamscene", "Startup", "false");
                RegistryKey rk = Registry.CurrentUser.CreateSubKey(@"SOFTWARE\Microsoft\Windows\CurrentVersion\Run");
                rk.DeleteValue("Dynamic Desktop");
            }
        }

        private void Window_Closed(object sender, EventArgs e)
        {
            GC.Collect();
            preview.Source = null;
            Grirg.Children.Clear();
        }
    }
}