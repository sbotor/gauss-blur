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
        private static int kernelSize = 3;
        private static double stdDev = 16;
        private static int threadCount = 12;

        private static string inpFileDir = @"D:\OneDrive - Politechnika Śląska\Studia\JA\gauss-blur\kaiki.png";

        private static string outFileDir = @"D:\OneDrive - Politechnika Śląska\Studia\JA\gauss-blur\blurred.png";
        
        private static Regex numRegex = new Regex(@"[0-9.]+");

        public MainWindow()
        {
            InitializeComponent();
            
            inpFilenameBox.Text = inpFileDir;
            outFilenameBox.Text = outFileDir;

            threadCountBox.Text = threadCount.ToString();
            kernelSizeBox.Text = kernelSize.ToString();
            stdDevBox.Text = stdDev.ToString();

            Debug.WriteLine("Started.");
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

        private bool checkKernelSize()
        {
            return int.TryParse(kernelSizeBox.Text, out kernelSize) &&
                kernelSize > 0 && kernelSize % 2 == 1;
        }

        private bool checkStdDev()
        {
            return double.TryParse(stdDevBox.Text, out stdDev) && stdDev >= 0;
        }
        
        private void blurButton_Click(object sender, RoutedEventArgs e)
        {
            string inpDir = inpFilenameBox.Text;
            if (inpDir != null && inpDir != "")
            {   
                if (!checkThreadCount())
                {
                    // TODO
                    MessageBox.Show("Invalid thread count.");
                }
                else if (!checkKernelSize())
                {
                    // TODO
                    MessageBox.Show("Invalid kernel size.");
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
                        if (File.Exists(inpDir))
                        {
                            Bitmap image = new Bitmap(inpDir);
                            
                            CBlurThreadFactory factory = new CBlurThreadFactory(threadCount);
                            BlurTask blur = new BlurTask(factory, image, new GaussKernel(kernelSize, stdDev));
                            Stopwatch sw = new Stopwatch();

                            sw.Start();
                            blur.Blur();
                            sw.Stop();
                            MessageBox.Show($"Finished in {sw.ElapsedMilliseconds / 1000.0 } seconds.");

                            image.Save(outFileDir);

                            loadOutPreview();
                        }
                        else
                        {
                            Debug.WriteLine($"Cannot open file: {inpDir}.");
                        }
                    }
                    catch (UriFormatException exc)
                    {
                        Debug.WriteLine($"Cannot open file: {exc.Data}.");
                    }
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
                Debug.WriteLine($"Cannot open file: {exc.Data}.");
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
