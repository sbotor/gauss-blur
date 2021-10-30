﻿#define CTEST

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
        private static double stdDev = 16;
        private static int threadCount = 12;

        private static string inpFileDir = @"C:\Users\sotor\OneDrive - Politechnika Śląska\Studia\JA\gauss-blur\aei.jpg";

        private static string outFileDir = @"C:\Users\sotor\OneDrive - Politechnika Śląska\Studia\JA\gauss-blur\blurred.png";
        
        private static Regex numRegex = new Regex(@"[0-9.]+");

        private FileStream inpStream;
        private Bitmap inpImage;
        private System.Drawing.Imaging.BitmapData inpImageData;

        //private FileStream outStream;

        public MainWindow()
        {
            InitializeComponent();
            
            inpFilenameBox.Text = inpFileDir;
            outFilenameBox.Text = outFileDir;

            threadCountBox.Text = threadCount.ToString();
            stdDevBox.Text = stdDev.ToString();

            Debug.WriteLine("Started.");
        }

        ~MainWindow()
        {
            if (inpStream.CanRead || inpStream.CanWrite)
            {
                inpStream.Close();
            }
        }
        
        private void inpFilenameBox_LostFocus(object sender, RoutedEventArgs e)
        {
            inpFileDir = inpFilenameBox.Text;
            if (inpFileDir != null && inpFileDir != "")
            {
                try
                {
                    if (File.Exists(inpFileDir))
                    {
                        loadInpPreview();
                    }
                    else
                    {
                        Debug.WriteLine($"Cannot open file: {inpFileDir}.");
                    }
                }
                catch (UriFormatException exc)
                {
                    Debug.WriteLine($"Cannot open file: {exc.Data}.");
                }
            }
        }

        private void outFilenameBox_LostFocus(object sender, RoutedEventArgs e)
        {
            outFileDir = outFilenameBox.Text;
        }

        private bool checkThreadCount()
        {
            return int.TryParse(threadCountBox.Text, out threadCount)
                && threadCount > 0 && threadCount <= 64;
        }

        private bool checkStdDev()
        {
            return double.TryParse(stdDevBox.Text, out stdDev) && stdDev > 0;
        }
        
        private void blurButton_Click(object sender, RoutedEventArgs e)
        {
            if (inpFileDir != null && inpFileDir != "")
            {   
                if (!checkThreadCount())
                {
                    // TODO
                    MessageBox.Show("Invalid thread count.");
                }
                else if (!checkStdDev())
                {
                    // TODO
                    MessageBox.Show("Invalid standard deviation.");
                }
                else
                {
                    try
                    {
                        if (inpFileDir == outFileDir)
                        {
                            MessageBox.Show("Input and output files cannot be the same.");
                        }
                        else if (File.Exists(inpFileDir))
                        {
                            loadInpPreview();

                            CBlurThreadFactory factory = new CBlurThreadFactory(threadCount);
                            BlurTask blur = new BlurTask(inpImageData, factory, stdDev);
                            Stopwatch sw = new Stopwatch();

                            sw.Start();
                            blur.Blur();
                            sw.Stop();
                            MessageBox.Show($"Finished in {sw.ElapsedMilliseconds / 1000.0 } seconds.");

                            unlockInpImage();

                            FileStream outStream = File.Open(outFileDir, FileMode.OpenOrCreate);
                            inpImage.Save(outStream, inpImage.RawFormat);
                            outStream.Close();
                            inpImage.Dispose();

                            loadOutPreview();
                        }
                        else
                        {
                            Debug.WriteLine($"Cannot open file: {inpFileDir}.");
                        }
                    }
                    catch (UriFormatException exc)
                    {
                        Debug.WriteLine($"Cannot open file: {exc.Data}.");
                    }
                }
            }
        }

        private void lockInpImage()
        {
            inpImageData = inpImage.LockBits(
                new Rectangle(0, 0, inpImage.Width, inpImage.Height),
                System.Drawing.Imaging.ImageLockMode.ReadWrite,
                inpImage.PixelFormat);
        }

        private void unlockInpImage()
        {
            inpImage.UnlockBits(inpImageData);
        }
        
        private void loadInpPreview()
        {
            try
            {
                if (File.Exists(inpFileDir))
                {
                    if (inpStream != null && inpStream.CanRead)
                    {
                        inpStream.Close();
                    }

                    inpStream = File.Open(inpFileDir, FileMode.Open);
                    //inpImagePreview.Source = new BitmapImage(new Uri(inpFileDir));
                    inpImage = new Bitmap(inpStream);
                    lockInpImage();

                    inpImagePreview.Source = BitmapSource.Create(
                        inpImageData.Width, inpImageData.Height,
                        inpImage.HorizontalResolution, inpImage.VerticalResolution,
                        System.Windows.Media.PixelFormats.Bgr24, null,
                        inpImageData.Scan0, inpImageData.Stride * inpImageData.Height,
                        inpImageData.Stride);

                }
            }
            catch (UriFormatException exc)
            {
                Debug.WriteLine($"Cannot open file: {exc.Data}.");
            }
        }
        
        private void loadOutPreview()
        {
            try
            {
                FileStream outStream = File.Open(outFileDir, FileMode.OpenOrCreate);
                Bitmap outImage = new Bitmap(outStream);
                System.Drawing.Imaging.BitmapData outData = outImage.LockBits(
                    new Rectangle(0, 0, outImage.Width, outImage.Height),
                    System.Drawing.Imaging.ImageLockMode.ReadOnly,
                    outImage.PixelFormat);

                outImagePreview.Source = BitmapSource.Create(
                    outData.Width, outData.Height,
                    outImage.HorizontalResolution, outImage.VerticalResolution,
                    System.Windows.Media.PixelFormats.Bgr24, null,
                    outData.Scan0, outData.Stride * outData.Height,
                    outData.Stride);

                outImage.UnlockBits(outData);
                outStream.Close();
                outImage.Dispose();
            }
            catch (FileNotFoundException exc)
            {
                Debug.WriteLine($"Cannot open file: {exc.Data}.");
            }
        }

        private void numBox_PreviewTextInput(object sender, System.Windows.Input.TextCompositionEventArgs e)
        {
            e.Handled = !numRegex.IsMatch(e.Text);
        }

        private void numBox_Pasting(object sender, DataObjectPastingEventArgs e)
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
