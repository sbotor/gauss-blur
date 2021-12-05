using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Text;
using System.Threading;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;

namespace GaussBlur.GUI
{
    /// <summary>
    /// Interaction logic for ProgressWindow.xaml
    /// </summary>
    public partial class ProgressWindow : Window
    { 
        public ProgressWindow()
        {
            init();
        }

        public ProgressWindow(Window window)
        {
            init();

            Left = window.Left + (window.Width - Width) / 2;
            Top = window.Top + window.Height / 2;

            Width = window.Width / 3;
        }

        private void init()
        {
            InitializeComponent();
            progressStatus.Value = 0;
        }
    }
}
