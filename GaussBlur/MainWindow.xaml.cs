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

                        Bitmap bmp = new Bitmap(inpDir);
                        System.Drawing.Rectangle rect = new System.Drawing.Rectangle(0, 0, bmp.Width, bmp.Height);
                        System.Drawing.Imaging.BitmapData bmpData =
                            bmp.LockBits(rect, System.Drawing.Imaging.ImageLockMode.ReadWrite,
                            bmp.PixelFormat);

                        RGBArray rgbarr = new RGBArray(bmpData);
                        rgbarr.SaveF("TestB.txt");
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
