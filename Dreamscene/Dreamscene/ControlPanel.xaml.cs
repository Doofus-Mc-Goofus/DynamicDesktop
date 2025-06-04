using System;
using System.Windows;
using System.Windows.Media.Imaging;
using Microsoft.Win32;

namespace Dreamscene
{
    public partial class ControlPanel : Window
    {
        private readonly MainWindow MainWiw;
        public ControlPanel(MainWindow mainWindow)
        {
            InitializeComponent();
            MainWiw = mainWindow;
        }

        public void Update(BitmapImage image)
        {
            preview.Source = image;
            URL.Content = MainWiw.HKCU_GetString(@"SOFTWARE\Dreamscene", "Wallp");
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
        }
    }
}
