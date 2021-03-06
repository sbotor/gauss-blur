using System;
using System.Windows;
using System.Windows.Media.Imaging;
using System.IO;
using System.Drawing;
using System.Diagnostics;
using System.Text.RegularExpressions;
using Microsoft.Win32;

using GaussBlur.ImageProc;
using GaussBlur.Threading;

namespace GaussBlur.GUI
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {   
        private double stdDev = 1;
        private int threadCount = 1;
        private int repeatCount = 1;

        //private const string inpFileDir;

       //private const string outFileDir;
        
        private static Regex numRegex = new Regex(@"[0-9.]+");

        private ImageContainer inputImage;

        public MainWindow()
        {
            InitializeComponent();

            inpFilenameBox.Text = @"";
            outFilenameBox.Text = Path.Join(Directory.GetCurrentDirectory(), "blurred.jpg");

            inputImage = new ImageContainer();

            threadCountBox.Text = stdDev.ToString();
            stdDevBox.Text = threadCount.ToString();
            repeatCountBox.Text = repeatCount.ToString();

            Debug.WriteLine("Started.");
        }
        
        private void inpFilenameBox_LostFocus(object sender, RoutedEventArgs e)
        {
            string newFileDir = inpFilenameBox.Text;
            
            if (newFileDir != null && newFileDir != "")
            {
                try
                {
                    loadInpPreview(newFileDir);
                }
                catch (FileNotFoundException)
                {
                    return;
                }
                catch (UriFormatException)
                {
                    return;
                }
                catch (ArgumentException exc)
                {
                    MessageBox.Show(exc.Message);
                }
            }
        }

        private bool checkParams()
        {
            if (!int.TryParse(threadCountBox.Text, out threadCount)
                && threadCount > 0 && threadCount <= 64)
            {
                MessageBox.Show("Invalid thread count.");
                return false;
            }


            if (!double.TryParse(stdDevBox.Text, out stdDev) && stdDev > 0.0 && stdDev <= 5000.0)
            {
                MessageBox.Show("Invalid standard deviation.");
                return false;
            }

            if (!int.TryParse(repeatCountBox.Text, out repeatCount)
                && repeatCount > 0 && repeatCount <= 64)
            {
                MessageBox.Show("Invalid repeat count.");
                return false;
            }

            return true;
        }

        private bool processImage(ThreadFactory factory)
        {
            bool success = false;
            inputImage.LockImage();
            if (inputImage.ImageData != null)
            {
                BlurTask blurTask = new BlurTask(inputImage.ImageData, threadCount, repeatCount);
                try
                {
                    ProgressWindow prog = new ProgressWindow(this);
                    blurTask.RunWorker(factory, prog);
                    prog.ShowDialog();
                }
                finally
                {
                    if (blurTask.Worker != null && (blurTask.Worker.IsBusy || !blurTask.Finished))
                    {
                        blurTask.Worker.CancelAsync();
                    }
                    else
                    {
                        MessageBox.Show($"Finished in {blurTask.RuntimeStopwatch.ElapsedMilliseconds / 1000.0 } seconds.");
                        success = true;
                    }
                }
            }
            inputImage.UnlockImage();

            return success;
        }

        private void blurButton_Click(object sender, RoutedEventArgs e)
        {
            string outDir = outFilenameBox.Text,
                inpDir = inpFilenameBox.Text;

            try
            {
                loadInpPreview(inpDir);

                if (checkParams())
                {
                    ThreadFactory? factory = getFactory();
                    if (factory == null)
                    {
                        MessageBox.Show("No library chosen.");
                        return;
                    }

                    if (processImage(factory!))
                    {
                        inputImage.Save(outDir);
                        loadOutPreview(outDir);
                    }
                }
            }
            catch (UriFormatException exc)
            {
                MessageBox.Show(exc.Message);
            }
            catch (FileNotFoundException exc)
            {
                MessageBox.Show(exc.Message);
            }
        }
        
        private ThreadFactory? getFactory()
        {
            switch (libraryBox.SelectedIndex)
            {
                case 0:
                    return new CFactory(stdDev);

                case 1:
                    return new AsmFactory(stdDev);

                case 2:
                    return new YMMAsmFactory(stdDev);
                
                default:
                    return null;
            }
        }

        private void loadInpPreview(string inpDir)
        {
            inpImagePreview.Source = inputImage.LoadBitmapSource(inpDir);
        }
        
        private void loadOutPreview(string outDir)
        {   
            try
            {
                outImagePreview.Source = ImageContainer.CreateBitmapSource(outFilenameBox.Text);
            }
            catch (FileNotFoundException e)
            {
                MessageBox.Show(e.Message);
            }
            catch (ArgumentException e)
            {
                MessageBox.Show("Cannot determine the pixel format of the image.");
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

        private void browseInpButton_Click(object sender, RoutedEventArgs e)
        {
            OpenFileDialog fileDialog = new OpenFileDialog();
            fileDialog.Filter = "Image files|*.jpg;*.png;*.bmp";

            if (fileDialog.ShowDialog() == true)
            {
                try
                {
                    loadInpPreview(fileDialog.FileName);
                    inpFilenameBox.Text = fileDialog.FileName;
                }
                catch (ArgumentException exc)
                {
                    MessageBox.Show(exc.Message);
                }
            }
        }

        private void browseOutButton_Click(object sender, RoutedEventArgs e)
        {
            SaveFileDialog fileDialog = new SaveFileDialog();
            fileDialog.Filter = "Image file|*.jpg|" +
                "Image file|*.png|" +
                "Bitmap file|*.bmp";
            
            if (fileDialog.ShowDialog() == true)
            {
                outFilenameBox.Text = fileDialog.FileName;
            }
        }

        protected override void OnClosed(EventArgs e)
        {
            base.OnClosed(e);
            Close();
            Environment.Exit(0);
        }
    }
}
