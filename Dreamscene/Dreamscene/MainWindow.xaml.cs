using System;
using System.Diagnostics;
using System.Drawing;
using System.IO;
using System.Runtime.InteropServices;
using System.Threading.Tasks;
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
        private readonly Process process = new Process();
        private readonly System.Windows.Forms.NotifyIcon notifyIcon1 = new System.Windows.Forms.NotifyIcon();
        private void OnDispatcherUnhandledException(object sender, DispatcherUnhandledExceptionEventArgs e)
        {
            HKCU_AddKey(@"SOFTWARE\Dreamscene", "Wallp", "BM");
            HKCU_AddKey(@"SOFTWARE\Dreamscene", "Fit", "10");
            HKCU_AddKey(@"SOFTWARE\Dreamscene", "Startup", "true");
            HKCU_AddKey(@"SOFTWARE\Dreamscene", "ActiveDesktop", "false");
            HKCU_AddKey(@"SOFTWARE\Dreamscene", "PauseBattery", "false");
            HKCU_AddKey(@"SOFTWARE\Microsoft\Windows\CurrentVersion\Run", "Wallpaper++", "\"" + System.Reflection.Assembly.GetExecutingAssembly().Location + "\" -hide 1");
            e.Handled = true;
            try
            {
                _ = MessageBox.Show(e.Exception.Message + "\n" + e.Exception.InnerException);
            }
            catch
            {
                try
                {
                    _ = MessageBox.Show(e.Exception.Message);
                }
                catch
                {
                    _ = MessageBox.Show("Unknown error occurred.");
                }
            }
            Application.Current.Shutdown();
        }
        public MainWindow()
        {
            Dispatcher.UnhandledException += OnDispatcherUnhandledException;
            InitializeComponent();
            HKCU_AddKey(@"SOFTWARE\Microsoft\Internet Explorer\Main\FeatureControl\FEATURE_BROWSER_EMULATION", "WallpaperPlusPlus.exe", 11000);
            string swaus = "\"" + System.Reflection.Assembly.GetExecutingAssembly().Location + "\" -hide 1";
            if (HKCU_GetString(@"SOFTWARE\Dreamscene", "Wallp") == "")
            {
                HKCU_AddKey(@"SOFTWARE\Dreamscene", "Wallp", "BM");
                HKCU_AddKey(@"SOFTWARE\Dreamscene", "Fit", "10");
                HKCU_AddKey(@"SOFTWARE\Dreamscene", "Startup", "true");
                HKCU_AddKey(@"SOFTWARE\Dreamscene", "ActiveDesktop", "false");
                HKCU_AddKey(@"SOFTWARE\Dreamscene", "PauseBattery", "false");
                HKCU_AddKey(@"SOFTWARE\Microsoft\Windows\CurrentVersion\Run", "Wallpaper++", swaus);
            }
            notifyIcon1.Icon = new Icon(Application.GetResourceStream(new Uri("pack://application:,,,/ico.ico")).Stream);
            notifyIcon1.Text = "Wallpaper++";
            notifyIcon1.Visible = true;
            notifyIcon1.MouseClick += CreateSetupWindowFunc;
            notifyIcon1.ContextMenuStrip = new System.Windows.Forms.ContextMenuStrip();
            _ = notifyIcon1.ContextMenuStrip.Items.Add("Open Settings", null, (s, ee) => CreateSetupWindow(false));
            _ = notifyIcon1.ContextMenuStrip.Items.Add("Adjust Volume", null, SndVol);
            _ = notifyIcon1.ContextMenuStrip.Items.Add("Check for Updates", null, UpdateWindowOpen);
            _ = notifyIcon1.ContextMenuStrip.Items.Add("Exit Wallpaper++", null, (s, ee) => Application.Current.Shutdown());
            SystemEvents.UserPreferenceChanged += (s, ee) => Update(true);
            timer.Tick += (s, ee) => Update(false);
            timer.Interval = TimeSpan.FromSeconds(1);
            timer.Start();
        }

        private async void UpdateWindowOpen(object sender, EventArgs e)
        {
            process.StartInfo = new ProcessStartInfo
            {
                FileName = @"explorer",
                Arguments = @"https://github.com/Doofus-Mc-Goofus/WallpaperPlusPlus"
            };
            _ = await Task.Run(process.Start);
            await Task.Run(process.WaitForExit);
            await Task.Run(process.Close);
        }

        private async void SndVol(object sender, EventArgs e)
        {
            process.StartInfo = new ProcessStartInfo
            {
                FileName = @"sndvol"
            };
            _ = await Task.Run(process.Start);
            await Task.Run(process.WaitForExit);
            await Task.Run(process.Close);
        }

        private void CreateSetupWindowFunc(object sender, System.Windows.Forms.MouseEventArgs e)
        {
            if (e.Button == System.Windows.Forms.MouseButtons.Left)
            {
                CreateSetupWindow(true);
            }

        }

        private void CreateSetupWindow(bool FUCK)
        {
            foreach (Window window in Application.Current.Windows)
            {
                if (window.GetType() == typeof(ControlPanel))
                {
                    _ = window.Activate();
                    _ = window.Focus();
                    return;
                }
            }
            publicpanel = new ControlPanel(this);
            publicpanel.Show();
            publicpanel.Closed += (s, ee) => publicpanel = null;
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
            if (publicpanel != null)
            {
                RenderTargetBitmap renderTargetBitmap = new RenderTargetBitmap((int)SystemParameters.PrimaryScreenWidth, (int)SystemParameters.PrimaryScreenHeight, 96, 96, PixelFormats.Pbgra32);
                renderTargetBitmap.Render(Video);
                renderTargetBitmap.Render(Aurora);
                renderTargetBitmap.Freeze();
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
                    RenderOptions.SetBitmapScalingMode(bitmapImage, BitmapScalingMode.NearestNeighbor);
                    bitmapImage.Freeze();
                }
                publicpanel.Update(bitmapImage);
                pngImage.Frames.Clear();
            }
        }
        public void Update(bool isUpdated)
        {
            Aurora.Visibility = Visibility.Collapsed;
            System.Drawing.Color bg = System.Drawing.SystemColors.Desktop;
            Background = new SolidColorBrush(System.Windows.Media.Color.FromArgb(bg.A, bg.R, bg.G, bg.B));
            string pst = HKCU_GetString(@"SOFTWARE\Dreamscene", "Wallp")[1].ToString() == "M" || HKCU_GetString(@"SOFTWARE\Dreamscene", "Wallp") == "aur"
                ? null
                : new Uri(HKCU_GetString(@"SOFTWARE\Dreamscene", "Wallp")).ToString();
            if (bool.Parse(HKCU_GetString(@"SOFTWARE\Dreamscene", "ActiveDesktop")))
            {
                Video.Visibility = Visibility.Collapsed;
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
                ActiveDesktop.Visibility = Visibility.Collapsed;
                try
                {
                    if (pst != Video.Source.ToString())
                    {
                        try
                        {
                            Video.Source = new Uri(HKCU_GetString(@"SOFTWARE\Dreamscene", "Wallp"));
                            Video.Play();
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
                            Video.Play();
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
                    if (Video.Source == null)
                    {
                        MediaFail();
                    }
                }
            }
            UpdatePreview();
            if (isUpdated)
            {
                if (Height != SystemParameters.PrimaryScreenHeight || Width != SystemParameters.PrimaryScreenWidth)
                {
                    publicpanel?.Close();
                    Visibility = Visibility.Collapsed;
                    _ = MessageBox.Show("Wallpaper++ could not hook into explorer. This may be for a variety of reasons, such as Aero not being enabled, or your version of Windows is incompatible. Wallpaper++ will now close.", "Hooking Error", MessageBoxButton.OK, MessageBoxImage.Exclamation);
                    Application.Current.Shutdown();
                }
                if ((RenderCapability.Tier >> 16) < 2)
                {
                    MessageBoxResult test = MessageBox.Show("Your graphics card may not be optimal for Wallpaper++. While Wallpaper++ may run, you may notice heavily degraded perfomance. Are you sure you want to continue?", "Performance Warning", MessageBoxButton.YesNo, MessageBoxImage.Exclamation);
                    if (test == MessageBoxResult.No)
                    {
                        Application.Current.Shutdown();
                    }
                }
            }
            try
            {
                if (bool.Parse(HKCU_GetString(@"SOFTWARE\Dreamscene", "ActiveDesktop")))
                {
                    Video.Source = null;
                }
                else
                {
                    if (bool.Parse(HKCU_GetString(@"SOFTWARE\Dreamscene", "PauseBattery")))
                    {
                        System.Windows.Forms.BatteryChargeStatus test = System.Windows.Forms.SystemInformation.PowerStatus.BatteryChargeStatus;
                        if (test == System.Windows.Forms.BatteryChargeStatus.High || test == System.Windows.Forms.BatteryChargeStatus.Low || test == System.Windows.Forms.BatteryChargeStatus.Critical)
                        {
                            Video.Pause();
                        }
                        else
                        {
                            Video.Play();
                        }
                    }
                    else
                    {
                        Video.Play();
                    }
                }
            }
            catch
            {

            }
            GC.Collect();
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
                CreateSetupWindow(true);
            }
            else
            {
                ControlPanel settingsWindow = new ControlPanel(this)
                {
                    Visibility = Visibility.Collapsed
                };
                settingsWindow.Show();
                publicpanel = settingsWindow;
                settingsWindow.Close();
            }
            Update(true);
        }

        private void MediaElement_MediaEnded(object sender, RoutedEventArgs e)
        {
            Video.Position = new TimeSpan(0, 0, 0, 0, 1);
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
            MediaFail();
        }
        private void MediaFail()
        {
            if (!File.Exists(HKCU_GetString(@"SOFTWARE\Dreamscene", "Wallp")))
            {
                switch (HKCU_GetString(@"SOFTWARE\Dreamscene", "Wallp"))
                {
                    case "aur":
                        NonFatalError.Content = "SaT";
                        break;
                    case "BM":
                        NonFatalError.Content = "Welcome to Wallpaper++! Select a file to get started.";
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
            try
            {
                NonFatalError.Content = !Video.HasVideo ? "This file has no video data." : "";
            }
            catch
            {
                NonFatalError.Content = "";
            }
        }

        private void Window_Closing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            notifyIcon1.Dispose();
        }
    }
}
