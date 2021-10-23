#define CTEST

using System;
using System.Windows;
using System.Windows.Media.Imaging;
using System.IO;
using System.Drawing;
using System.Runtime.InteropServices;
using System.Threading;
using System.Diagnostics;
using System.Text.RegularExpressions;

namespace GaussBlur
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        private static int kernelSize = 127;
        private static double stdDev = 8;
        private static int threadCount = 64;

        private static string inpFileDir = @"D:\OneDrive - Politechnika Śląska\Studia\JA\gauss-blur\kaiki.png";

        private static string outFileDir = @"D:\OneDrive - Politechnika Śląska\Studia\JA\gauss-blur\blurred.png";

        private static Regex numRegex = new Regex(@"[0-9]+");

        public MainWindow()
        {
            InitializeComponent();
            inpFilenameBox.Text = inpFileDir;
            outFilenameBox.Text = outFileDir;
        }

        private void testCLibThreads(RGBArray rgbArr, int n)
        {
            int[] slices = rgbArr.Slice(n);
            double[] helperArray = new double[rgbArr.Length];
            Thread[] threads = new Thread[n];
            GaussKernel kernel = new GaussKernel(kernelSize, stdDev);

            Stopwatch sw = new Stopwatch();
            sw.Start();

            unsafe
            {
                fixed (double* rgbArrP = rgbArr.Data,
                    helperArrayP = helperArray,
                    kernelP = kernel.Data)
                {
                    BlurThreadFactory blurFact = new CBlurThreadFactory(
                        rgbArrP,
                        helperArrayP,
                        rgbArr.Stride,
                        rgbArr.Height,
                        kernelP,
                        kernelSize,
                        n);
                    
                    for (int i = 0; i < n; i++)
                    {
                        IBlurThread param = (CBlurThread)blurFact.CreateThread(slices[i], slices[i + 1]);
                        
                        threads[i] = new Thread(new ThreadStart(param.Run));
                        threads[i].Start();
                    }
                    for (int i = 0; i < n; i++)
                    {
                        threads[i].Join();
                    }

                    sw.Stop();
                    MessageBox.Show($"Finished in {sw.Elapsed.TotalMilliseconds / 1000} s.");
                }
            }
        }
        
        private void inpFilenameBox_LostFocus(object sender, RoutedEventArgs e)
        {
            string inpDir = inpFilenameBox.Text;
            if (inpDir != null && inpDir != "")
            {
                try
                {
                    if (File.Exists(inpDir))
                    {
                        inpFileDir = inpDir;
                        loadInpPreview();
                    }
                    else
                    {
                        Console.WriteLine($"Cannot open file: {inpDir}.");
                    }
                }
                catch (UriFormatException exc)
                {
                    Console.WriteLine($"Cannot open file: {exc.Data}.");
                }
            }
        }

        private void outFilenameBox_LostFocus(object sender, RoutedEventArgs e)
        {
            string outDir = inpFilenameBox.Text;
            if (outDir != null && outDir != "")
            {
                try
                {
                    outFileDir = outDir;
                }
                catch (UriFormatException exc)
                {
                    Console.WriteLine($"Cannot open file: {exc.Data}.");
                }
            }
        }

        private void blurButton_Click(object sender, RoutedEventArgs e)
        {
            string inpDir = inpFilenameBox.Text;
            if (inpDir != null && inpDir != "")
            {
                try
                {
                    if (File.Exists(inpDir))
                    {
                        Bitmap image = new Bitmap(inpDir);
                        System.Drawing.Rectangle rect = new System.Drawing.Rectangle(0, 0, image.Width, image.Height);
                        System.Drawing.Imaging.BitmapData imageData =
                            image.LockBits(rect, System.Drawing.Imaging.ImageLockMode.ReadWrite,
                            image.PixelFormat);

                        RGBArray rgbArr = new RGBArray(imageData);
                        int[] slices = rgbArr.Slice(4);
                        //testCLib(rgbArr);
                        //testCLibParam(rgbArr, 4);
                        testCLibThreads(rgbArr, threadCount);

                        Marshal.Copy(rgbArr.ToByteArray(), 0, imageData.Scan0, rgbArr.Length);
                        image.UnlockBits(imageData);

                        image.Save(outFileDir);

                        loadOutPreview();
                    }
                    else
                    {
                        Console.WriteLine($"Cannot open file: {inpDir}.");
                    }
                }
                catch (UriFormatException exc)
                {
                    Console.WriteLine($"Cannot open file: {exc.Data}.");
                }
            }
        }

        private void loadInpPreview()
        {
            try
            {
                if (File.Exists(inpFileDir))
                {
                    inpImagePreview.Source = new BitmapImage(new Uri(inpFileDir));
                }
            }
            catch (UriFormatException exc)
            {
                Console.WriteLine($"Cannot open file: {exc.Data}.");
            }
        }
        
        private void loadOutPreview()
        {
            try
            {
                if (File.Exists(outFileDir))
                {
                    outImagePreview.Source = new BitmapImage(new Uri(outFileDir));
                }
            }
            catch (UriFormatException exc)
            {
                Console.WriteLine($"Cannot open file: {exc.Data}.");
            }
        }

        private void threadCountBox_PreviewTextInput(object sender, System.Windows.Input.TextCompositionEventArgs e)
        {
            e.Handled = !numRegex.IsMatch(e.Text);
        }

        private void threadCountBox_Pasting(object sender, DataObjectPastingEventArgs e)
        {
            if (e.DataObject.GetDataPresent(typeof(string)))
            {
                string text = (string)e.DataObject.GetData(typeof(string));
                if (!numRegex.IsMatch(text))
                {
                    e.CancelCommand();
                }
            }
            else
            {
                e.CancelCommand();
            }
        }
    }
}
