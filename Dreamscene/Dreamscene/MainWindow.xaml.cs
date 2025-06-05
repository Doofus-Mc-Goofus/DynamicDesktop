using System;
using System.IO;
using System.Runtime.InteropServices;
using System.Windows;
using System.Windows.Interop;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Threading;
using Microsoft.Win32;

namespace Dreamscene
{
    public partial class MainWindow : Window
    {
        private readonly DispatcherTimer timer = new DispatcherTimer();
        private bool showwindow = true;
        private ControlPanel publicpanel;
        private void OnDispatcherUnhandledException(object sender, DispatcherUnhandledExceptionEventArgs e)
        {
            HKCU_AddKey(@"SOFTWARE\Dreamscene", "Wallp", "BM");
            HKCU_AddKey(@"SOFTWARE\Dreamscene", "Fit", "10");
            HKCU_AddKey(@"SOFTWARE\Dreamscene", "ActiveDesktop", "false");
            e.Handled = true;
            Application.Current.Shutdown();
        }
        public MainWindow()
        {
            Dispatcher.UnhandledException += OnDispatcherUnhandledException;
            InitializeComponent();
            HKCU_AddKey(@"SOFTWARE\Microsoft\Internet Explorer\Main\FeatureControl\FEATURE_BROWSER_EMULATION", "DynamicDesktop.exe", 11000);
            if (HKCU_GetString(@"SOFTWARE\Dreamscene", "Wallp") == "")
            {
                HKCU_AddKey(@"SOFTWARE\Dreamscene", "Wallp", "BM");
                HKCU_AddKey(@"SOFTWARE\Dreamscene", "Fit", "10");
                HKCU_AddKey(@"SOFTWARE\Dreamscene", "ActiveDesktop", "false");
            }
            SystemEvents.UserPreferenceChanged += (s, ee) => Update(true);
            timer.Tick += (s, ee) => Update(false);
            timer.Interval = TimeSpan.FromSeconds(1);
            timer.Start();
        }
        private void CreateSetupWindow()
        {
            ControlPanel settingsWindow = new ControlPanel(this);
            settingsWindow.Show();
            publicpanel = settingsWindow;
        }

        [Flags]
        private enum SendMessageTimeoutFlags : uint
        {
            SMTO_NORMAL = 0u,
            SMTO_BLOCK = 1u,
            SMTO_ABORTIFHUNG = 2u,
            SMTO_NOTIMEOUTIFNOTHUNG = 8u,
            SMTO_ERRORONEXIT = 0x20u
        }
        private void UpdatePreview()
        {
            RenderTargetBitmap renderTargetBitmap = new RenderTargetBitmap((int)SystemParameters.PrimaryScreenWidth, (int)SystemParameters.PrimaryScreenHeight, 96, 96, PixelFormats.Pbgra32);
            renderTargetBitmap.Render(Video);
            renderTargetBitmap.Render(ActiveDesktop);
            BitmapImage bitmapImage = new BitmapImage();
            PngBitmapEncoder pngImage = new PngBitmapEncoder();
            pngImage.Frames.Add(BitmapFrame.Create(renderTargetBitmap));
            using (MemoryStream stream = new MemoryStream())
            {
                pngImage.Save(stream);
                _ = stream.Seek(0, SeekOrigin.Begin);
                bitmapImage.BeginInit();
                bitmapImage.CacheOption = BitmapCacheOption.OnLoad;
                bitmapImage.StreamSource = stream;
                bitmapImage.EndInit();
            }
            publicpanel.Update(bitmapImage);
        }

        private void Update(bool isUpdated)
        {
            Aurora.Visibility = Visibility.Collapsed;
            System.Drawing.Color bg = System.Drawing.SystemColors.Desktop;
            Background = new SolidColorBrush(Color.FromArgb(bg.A, bg.R, bg.G, bg.B));
            string pst;
            if (HKCU_GetString(@"SOFTWARE\Dreamscene", "Wallp")[1].ToString() == "M")
            {
                pst = null;
            }
            else
            {
                pst = new Uri(HKCU_GetString(@"SOFTWARE\Dreamscene", "Wallp")).ToString();
            }
            if (bool.Parse(HKCU_GetString(@"SOFTWARE\Dreamscene", "ActiveDesktop")))
            {
                Video.Visibility = Visibility.Hidden;
                ActiveDesktop.Visibility = Visibility.Visible;
                try
                {
                    if (new Uri(pst) != ActiveDesktop.Source)
                    {
                        try
                        {
                            ActiveDesktop.Source = new Uri(pst);
                        }
                        catch
                        {
                            ActiveDesktop.Source = null;
                        }
                    }
                }
                catch
                {
                    // try harder
                    if (pst != null)
                    {
                        try
                        {
                            ActiveDesktop.Source = new Uri(pst);
                        }
                        catch
                        {
                            ActiveDesktop.Source = null;
                        }
                    }
                }
            }
            else
            {
                Video.Visibility = Visibility.Visible;
                ActiveDesktop.Visibility = Visibility.Hidden;
                try
                {
                    if (pst != Video.Source.ToString())
                    {
                        try
                        {
                            Video.Source = new Uri(HKCU_GetString(@"SOFTWARE\Dreamscene", "Wallp"));
                        }
                        catch
                        {
                            Video.Source = null;
                        }
                    }
                }
                catch
                {
                    // try harder
                    if (pst != null)
                    {
                        try
                        {
                            Video.Source = new Uri(HKCU_GetString(@"SOFTWARE\Dreamscene", "Wallp"));
                        }
                        catch
                        {
                            Video.Source = null;
                        }
                    }
                }
                finally
                {
                    if (HKCU_GetString(@"SOFTWARE\Dreamscene", "Wallp") == "aur")
                    {
                        Aurora.Visibility = Visibility.Visible;
                    }
                    // Ugly hack, but fuck it.
                    Video.Stretch = Stretch.None;
                    _ = Video.Width;
                    _ = Video.Height;
                    double vidrat = Video.Width / Video.Height;
                    switch (HKCU_GetString(@"SOFTWARE\Dreamscene", "Fit"))
                    {
                        case "10":
                            Video.Stretch = Stretch.UniformToFill;
                            break;
                        case "6":
                            Video.Stretch = Stretch.Uniform;
                            break;
                        case "2":
                            Video.Stretch = Stretch.Fill;
                            break;
                        case "22":
                            Video.Stretch = Stretch.UniformToFill;
                            break;
                        default:
                            Video.Stretch = Stretch.None;
                            break;
                    }
                    if (Video.Source != null)
                    {
                        switch (HKCU_GetString(@"SOFTWARE\Dreamscene", "Fit"))
                        {
                            case "10":
                                Video.Height = Video.Width / vidrat;
                                Video.VerticalAlignment = VerticalAlignment.Center;
                                break;
                            default:
                                Video.Height = double.NaN;
                                Video.VerticalAlignment = VerticalAlignment.Stretch;
                                break;
                        }
                    }
                    else
                    {
                        Video.Height = double.NaN;
                        Video.VerticalAlignment = VerticalAlignment.Stretch;
                    }
                }
            }
            UpdatePreview();

            if (isUpdated)
            {
                _ = DwmIsCompositionEnabled(out bool aeroEnabled);
                if (!aeroEnabled)
                {
                    _ = MessageBox.Show("Dreamscene needs the Aero theme to be applied in order to run. Please enable the Aero theme and restart the program.", "Compositing Error", MessageBoxButton.OK, MessageBoxImage.Exclamation);
                    Close();
                }
            }
        }

        private delegate bool EnumWindowsProc(IntPtr hWnd, IntPtr lParam);

        [DllImport("user32.dll", SetLastError = true)]
        private static extern IntPtr FindWindow(string lpClassName, string lpWindowName);
        [DllImport("user32.dll", CharSet = CharSet.Auto, SetLastError = true)]
        private static extern IntPtr SendMessageTimeout(IntPtr windowHandle, uint Msg, IntPtr wParam, IntPtr lParam, SendMessageTimeoutFlags flags, uint timeout, out IntPtr result);
        [DllImport("user32.dll", SetLastError = true)]
        public static extern IntPtr FindWindowEx(IntPtr parentHandle, IntPtr hWndChildAfter, string className, string windowTitle);

        [DllImport("user32.dll")]
        [return: MarshalAs(UnmanagedType.Bool)]
        private static extern bool EnumWindows(EnumWindowsProc lpEnumFunc, IntPtr lParam);
        [DllImport("user32.dll")]
        private static extern bool ShowWindow(IntPtr hWnd, int nCmdShow);
        [DllImport("user32.dll", SetLastError = true)]
        private static extern IntPtr SetParent(IntPtr hWndChild, IntPtr hWndNewParent);
        [DllImport("dwmapi.dll")]
        public static extern IntPtr DwmIsCompositionEnabled(out bool pfEnabled);

        private void Grid_Loaded(object sender, RoutedEventArgs e)
        {
            IntPtr intPtr = FindWindow("Progman", null);
            IntPtr result = IntPtr.Zero;
            _ = SendMessageTimeout(intPtr, 1324u, new IntPtr(0), IntPtr.Zero, SendMessageTimeoutFlags.SMTO_NORMAL, 1000u, out result);
            IntPtr workerw = IntPtr.Zero;
            _ = EnumWindows(delegate (IntPtr tophandle, IntPtr topparamhandle)
            {
                IntPtr intPtr2 = FindWindowEx(tophandle, IntPtr.Zero, "SHELLDLL_DefView", "");
                if (intPtr2 != IntPtr.Zero)
                {
                    workerw = FindWindowEx(IntPtr.Zero, tophandle, "WorkerW", "");
                }
                return true;
            }, IntPtr.Zero);
            if (workerw != IntPtr.Zero)
            {
                IntPtr handle = new WindowInteropHelper(Application.Current.MainWindow).Handle;
                _ = ShowWindow(workerw, 0);
                _ = SetParent(handle, intPtr);
                base.Left = 0.0;
                base.Top = 0.0;
                base.Height = SystemParameters.PrimaryScreenHeight;
                base.Width = SystemParameters.PrimaryScreenWidth;
            }
            string[] args = Environment.GetCommandLineArgs();
            for (int index = 1; index < args.Length; index += 2)
            {
                if (args[index] == "-hide")
                {
                    showwindow = false;
                }
            }
            if (showwindow)
            {
                CreateSetupWindow();
            }
            Update(false);
        }

        private void MediaElement_MediaEnded(object sender, RoutedEventArgs e)
        {
            Video.Stop();
            Video.Position = new TimeSpan(0);
            Video.Play();
        }
        public string HKCU_GetString(string path, string key)
        {
            try
            {
                RegistryKey rk = Registry.CurrentUser.OpenSubKey(path);
                return rk == null ? "" : (string)rk.GetValue(key);
            }
            catch { return ""; }
        }

        public void HKCU_AddKey(string path, string key, object value)
        {
            RegistryKey rk = Registry.CurrentUser.CreateSubKey(path);
            rk.SetValue(key, value);
        }

        private void Video_MediaFailed(object sender, ExceptionRoutedEventArgs e)
        {
            if (!File.Exists(HKCU_GetString(@"SOFTWARE\Dreamscene", "Wallp")))
            {
                switch (HKCU_GetString(@"SOFTWARE\Dreamscene", "Wallp"))
                {
                    case "aur":
                        NonFatalError.Content = "SaT";
                        break;
                    case "BM":
                        NonFatalError.Content = "Select a file";
                        break;
                    default:
                        NonFatalError.Content = "Unable to locate video file, please locate it and try again.";
                        break;
                }
            }
            else
            {
                NonFatalError.Content = "Unable to play video file.";
            }

        }

        private void Video_MediaOpened(object sender, RoutedEventArgs e)
        {
            NonFatalError.Content = "";
        }
    }
}
