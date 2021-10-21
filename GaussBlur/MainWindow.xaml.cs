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

namespace GaussBlur
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        private bool firstClick = true;
        private int counter = 0;

        private string inpFile = @"D:\OneDrive - Politechnika Śląska\Studia\JA\gauss-blur\kaiki.png";
        //private string inpFile = @"D:\OneDrive - Politechnika Śląska\Studia\JA\gauss-blur\test.png";
        private string outFile = @"D:\OneDrive - Politechnika Śląska\Studia\JA\gauss-blur\blurred.png";

        private int kernelSize = 256;
        private double stdDev = 8;

        public MainWindow()
        {
            InitializeComponent();
            InpFilenameBox.Text = inpFile;
            //InpFilenameBox.Text = @"D:\OneDrive - Politechnika Śląska\Studia\JA\gauss-blur\test.png";
        }

        private void TestButton_Click(object sender, RoutedEventArgs e)
        {
#if ASMTEST
            if (firstClick)
            {
                TestLabel.Content = AsmTest.sanityTest();
                firstClick = false;
            }
            else
            {
                //float[] first = new float[4] { 1, 2, 3, 4 },
                //    second = new float[4] { 2, 2, 2, 2 },
                //    result = AsmTest.TestSIMDS(first, second);

                //TestLabel.Content = $"{result[0]}, {result[1]}, {result[2]}, {result[3]}";

                float[] first = new float[4] { 1, 2, 3, 4 },
                    second = new float[4] { 2, 2, 2, 2 };

                AsmTest.TestSIMDInPlaceS(first, second);

                TestLabel.Content = $"{second[0]}, {second[1]}, {second[2]}, {second[3]}";
            }

        }
#endif
        //private void testCLib(RGBArray rgbArr)
        //{
        //    double[] helperArray = new double[rgbArr.Length];
        //    GaussKernel kernel = new GaussKernel(kernelSize, stdDev);
            
        //    unsafe
        //    {
        //        fixed (double* rgbArrP = rgbArr.Data,
        //            helperArrayP = helperArray,
        //            kernelP = kernel.Data)
        //        {
                    
        //        }
        //    }
        //}

        private void testCLibThreads(RGBArray rgbArr, int n)
        {
            int[] slices = rgbArr.Slice(n);
            double[] helperArray = new double[rgbArr.Length];
            Thread[] threads = new Thread[n];
            GaussKernel kernel = new GaussKernel(kernelSize, stdDev);
            ImgProcThread.ThreadCount = n;

            unsafe
            {
                fixed (double* rgbArrP = rgbArr.Data,
                    helperArrayP = helperArray,
                    kernelP = kernel.Data)
                {
                    for (int i = 0; i < n; i++)
                    {
                        ImgProcThread param = new ImgProcThread(
                            rgbArrP,
                            helperArrayP,
                            slices[i],
                            slices[i + 1],
                            rgbArr.Stride,
                            rgbArr.Height,
                            kernelP,
                            kernel.Size);
                        
                        threads[i] = new Thread(new ThreadStart(param.Run));
                        threads[i].Start();
                    }
                    for (int i = 0; i < n; i++)
                    {
                        threads[i].Join();
                    }
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
                        OutpFilenameBox.Text = image.UriSource.AbsolutePath;
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
                        testCLibThreads(rgbArr, 64);

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
