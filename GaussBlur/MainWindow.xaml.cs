#define CTEST

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
using System.IO;
using System.Drawing;
using System.Runtime.InteropServices;
using System.Threading;
using System.Diagnostics;

namespace GaussBlur
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        private string inpFile = @"D:\OneDrive - Politechnika Śląska\Studia\JA\gauss-blur\kaiki.png";
        //private string inpFile = @"D:\OneDrive - Politechnika Śląska\Studia\JA\gauss-blur\test.png";
        //private string inpFile = @"D:\OneDrive - Politechnika Śląska\Studia\JA\gauss-blur\4k.jpg";
        
        private string outFile = @"D:\OneDrive - Politechnika Śląska\Studia\JA\gauss-blur\blurred.png";

        private int kernelSize = 255;
        private double stdDev = 8;
        private int threadCount = 12;

        public MainWindow()
        {
            InitializeComponent();
            InpFilenameBox.Text = inpFile;
            //InpFilenameBox.Text = @"D:\OneDrive - Politechnika Śląska\Studia\JA\gauss-blur\test.png";
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
        
        private void InpFilenameBox_LostFocus(object sender, RoutedEventArgs e)
        {
            string inpDir = InpFilenameBox.Text;
            if (inpDir != null && inpDir != "")
            {
                try
                {
                    if (File.Exists(inpDir))
                    {
                        Uri imageUri = new Uri(inpDir);
                        BitmapImage image = new BitmapImage(imageUri);
                        InputImagePreview.Source = image;
                        OutFilenameBox.Text = outFile;
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

        private void BlurButton_Click(object sender, RoutedEventArgs e)
        {
            string inpDir = InpFilenameBox.Text;
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

                        image.Save(outFile);

                        Uri imageUri = new Uri(outFile);
                        BitmapImage imgPrev = new BitmapImage(imageUri);
                        OutputImagePreview.Source = imgPrev;
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
    }
}
