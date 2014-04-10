using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Drawing;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Forms;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using System.IO;
using System.Windows.Threading;

namespace KinectImageViewer
{
    /// <summary>
    /// Interaction logic for FullscreenPics.xaml
    /// </summary>
    public partial class FullscreenPics : Window
    {
        protected string[] picFiles;
        protected int currentImg = 0;
        MainWindow main;

        private DispatcherTimer slide_timer = new DispatcherTimer();
        DispatcherTimer buttonTimer = new DispatcherTimer();

        public FullscreenPics(int shownImg, MainWindow m, string[] pics)
        {
            currentImg = shownImg;
            main = m;
            picFiles = pics;
            InitializeComponent();            
        }

        private void OnLoad(object sender, RoutedEventArgs e)
        {
            ShowCurrentImage();            
        }

        private void previousBtn_Click(object sender, RoutedEventArgs e)
        {
            if (picFiles.Length > 0)
            {
                currentImg = currentImg == 0 ? picFiles.Length - 1 : --currentImg;
                ShowCurrentImage();
                
            }
        }

        private void nextBtn_Click(object sender, System.EventArgs e)
        {
            if (picFiles.Length > 0)
            {
                currentImg = currentImg == picFiles.Length - 1 ? 0 : ++currentImg;
                ShowCurrentImage();
                
                
            }
        }
        public void previous()
        {
            if (picFiles.Length > 0)
            {
                currentImg = currentImg == 0 ? picFiles.Length - 1 : --currentImg;
                ShowCurrentImage();

            }
        }
        public void next()
        {
            if (picFiles.Length > 0)
            {
                currentImg = currentImg == picFiles.Length - 1 ? 0 : ++currentImg;
                ShowCurrentImage();
            }
        }

        protected void ShowCurrentImage()
        {
            if (currentImg >= 0 && currentImg <= picFiles.Length - 1)
            {
                BitmapImage bm = new BitmapImage(new Uri(picFiles[currentImg], UriKind.RelativeOrAbsolute));
                FullscreenImageBox.Source = bm;
            }
        }


        public void slideShow()
        {
            slide_timer.Interval = TimeSpan.FromMilliseconds(2000);
            slide_timer.Tick += nextImage_Timer;
            slide_timer.Start();
        }


        private void nextImage_Timer(object sender, EventArgs e)
        {
            next();
        }

        private void Exit(object sender, System.EventArgs e)
        {
            main.setCurrentImg(currentImg);
            this.Close();
        }

        public void exit()
        {
            main.setCurrentImg(currentImg);
            this.Close();
        }

        private void nextBtn_MouseLeave(object sender, System.Windows.Input.MouseEventArgs e)
        {
            buttonTimer.Tick -= pressNextButton;
            buttonTimer.Stop();
            this.Cursor = System.Windows.Input.Cursors.Hand;
        }

        private void nextBtn_MouseEnter(object sender, System.Windows.Input.MouseEventArgs e)
        {
            buttonTimer.Interval = TimeSpan.FromMilliseconds(2000);
            buttonTimer.Tick += pressNextButton;
            buttonTimer.Start();
            this.Cursor = System.Windows.Input.Cursors.Wait;
        }

        void pressNextButton(object sender, object e)
        {
            if (picFiles.Length > 0)
            {
                currentImg = currentImg == picFiles.Length - 1 ? 0 : ++currentImg;
                ShowCurrentImage();
            }
            buttonTimer.Stop();
            this.Cursor = System.Windows.Input.Cursors.Hand;
        }

        private void previousBtn_MouseLeave(object sender, System.Windows.Input.MouseEventArgs e)
        {
            buttonTimer.Tick -= pressPrevButton;
            buttonTimer.Stop();
            this.Cursor = System.Windows.Input.Cursors.Hand;
        }

        private void previousBtn_MouseEnter(object sender, System.Windows.Input.MouseEventArgs e)
        {
            buttonTimer.Interval = TimeSpan.FromMilliseconds(2000);
            buttonTimer.Tick += pressPrevButton;
            buttonTimer.Start();
            this.Cursor = System.Windows.Input.Cursors.Wait;
        }

        void pressPrevButton(object sender, object e)
        {
            if (picFiles.Length > 0)
            {
                currentImg = currentImg == 0 ? picFiles.Length - 1 : --currentImg;
                ShowCurrentImage();
            }
            buttonTimer.Stop();
            this.Cursor = System.Windows.Input.Cursors.Hand;
        }

        private void exit_MouseLeave(object sender, System.Windows.Input.MouseEventArgs e)
        {
            buttonTimer.Tick -= pressExitButton;
            buttonTimer.Stop();
            this.Cursor = System.Windows.Input.Cursors.Hand;
        }

        private void exit_MouseEnter(object sender, System.Windows.Input.MouseEventArgs e)
        {
            buttonTimer.Interval = TimeSpan.FromMilliseconds(2000);
            buttonTimer.Tick += pressExitButton;
            buttonTimer.Start();
            this.Cursor = System.Windows.Input.Cursors.Wait;
        }

        void pressExitButton(object sender, object e)
        {
            buttonTimer.Stop();
            this.Cursor = System.Windows.Input.Cursors.Hand;
            main.setCurrentImg(currentImg);
            slide_timer.Stop();
            this.Close();
        }
    }
}
