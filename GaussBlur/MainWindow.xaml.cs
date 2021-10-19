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
    }
}
