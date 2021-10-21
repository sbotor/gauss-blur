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

namespace GaussBlur
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        private bool firstClick = true;
        private int counter = 0;
        
        public MainWindow()
        {
            InitializeComponent();
            InpFilenameBox.Text = @"D:\repos\img-processing\kaiki31.png";
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
#elif CTEST
            //int first = 1, second = 3;
            TestLabel.Content = CTest.addTest(counter++, 1);
#endif
        }

        private void testCLib(RGBArray rgbArr)
        {
            unsafe
            {
                fixed (double* rgbArrP = rgbArr.Data)
                {
                    CTest.blur(
                        rgbArrP,
                        0,
                        rgbArr.Length,
                        rgbArr.Stride,
                        rgbArr.Height,
                        32,
                        4);
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
                        testCLib(rgbArr);

                        Marshal.Copy(rgbArr.ToByteArray(), 0, imageData.Scan0, rgbArr.Length);
                        image.UnlockBits(imageData);

                        image.Save("blurred.png");
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
