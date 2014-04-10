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
using System.Threading;
using Microsoft.Kinect;
using Coding4Fun.Kinect.Wpf;
using System.Runtime.InteropServices;

namespace KinectImageViewer
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>

    public partial class MainWindow : Window
    {
        progLogic pL = new progLogic();

        protected string[] vidFiles;
        protected int currentVid = 0;

        System.Media.SoundPlayer playerCam = new System.Media.SoundPlayer("../../images/camera-sound.wav");
        System.Media.SoundPlayer playerCount = new System.Media.SoundPlayer("../../images/countdownCam-sound.wav");

        DispatcherTimer ticks = new DispatcherTimer();
        DispatcherTimer pauser = new DispatcherTimer();
        DispatcherTimer waiting = new DispatcherTimer();
        DispatcherTimer buttonTimer = new DispatcherTimer();

        [DllImport("user32")]

        public static extern int SetCursorPos(int x, int y);

        private const int MOUSEEVENTF_MOVE = 0x0001;
        private const int MOUSEEVENTF_LEFTDOWN = 0x0002;
        private const int MOUSEEVENTF_LEFTUP = 0x0004;
        private const int MOUSEEVENTF_RIGHTDOWN = 0x0008;

        [DllImport("user32.dll",
            CharSet = CharSet.Auto, CallingConvention = CallingConvention.StdCall)]

        public static extern void mouse_event(int dwflags, int dx, int dy, int cButtons, int dwExtraInfo);


        protected string[] picFiles;
        protected int currentImg = 0;
        protected bool isPlaying = false;
        protected bool mediaOpened = false;
        protected bool tabOpened = false;
        protected bool checking = true;


        FullscreenPics fPic;
        FullscreenVid fVid;

        protected bool fullPic = false;
        protected bool fullVid = false;

        //Kinect
        bool closing = false;
        const int skeletonCount = 6;
        Skeleton[] allSkeletons = new Skeleton[skeletonCount];

        public MainWindow()
        {
            InitializeComponent();
        }

        private void OnLoad(object sender, RoutedEventArgs e)
        {
            //Kinect
            kinectSensorChooser1.KinectSensorChanged += new DependencyPropertyChangedEventHandler(kinectSensorChooser1_KinectSensorChanged);

            String i = Environment.GetFolderPath(Environment.SpecialFolder.MyPictures);
            string[] ext = { ".jpg", ".jpeg", ".gif", ".png", ".bmp", ".tiff" };
            picFiles = Directory.GetFiles(i, "*.*")
                .Where(f => ext.Contains(new FileInfo(f).Extension.ToLower())).ToArray();
            String v = Environment.GetFolderPath(Environment.SpecialFolder.MyVideos);
            string[] v_ext = { ".mp4", ".wmv", ".wma", ".mov", ".avi" };
            vidFiles = Directory.GetFiles(v, "*.*")
                .Where(g => v_ext.Contains(new FileInfo(g).Extension.ToLower())).ToArray();
            ShowCurrentImage();
            ShowNextImage();
            ShowSecondNextImage();
            ShowPreviousImage();
            ShowSecondPreviousImage();
            myMediaElement.Source = new Uri(vidFiles[currentVid], UriKind.Absolute);
     
        }
       

        private void pause_Media(object sender, EventArgs e)
        {
            myMediaElement.Pause();
            isPlaying = false;
            pauser.Stop();
        }

        private void stop_Waiting(object sender, EventArgs e)
        {
            checking = true;
            waiting.Stop();
        }

        private void previousBtn_Click(object sender, RoutedEventArgs e)
        {
            if (picFiles.Length > 0)
            {
                currentImg = currentImg == 0 ? picFiles.Length - 1 : --currentImg;
                ShowCurrentImage();
                ShowNextImage();
                ShowSecondNextImage();
                ShowPreviousImage();
                ShowSecondPreviousImage();
            }
        }

        private void takePicBtn_Click(object sender, RoutedEventArgs e)
        {
            pauser.Interval = TimeSpan.FromMilliseconds(3000);
            pauser.Tick += pressCapture;
            playerCount.Play();
            pauser.Start();

        }

        void pressCapture(object sender, object e)
        {
            CaptureScreen(476, 195, 795, 577);
            pauser.Stop();
        }

        private void nextBtn_Click(object sender, System.EventArgs e)
        {
            if (picFiles.Length > 0)
            {
                currentImg = currentImg == picFiles.Length - 1 ? 0 : ++currentImg;
                ShowCurrentImage();
                ShowNextImage();
                ShowSecondNextImage();
                ShowPreviousImage();
                ShowSecondPreviousImage();
            }
        }

        protected void ShowCurrentImage()
        {
            if (currentImg >= 0 && currentImg <= picFiles.Length - 1)
            {
                BitmapImage bm = new BitmapImage(new Uri(picFiles[currentImg], UriKind.RelativeOrAbsolute));
                ImageBox.Source = bm;
            }
        }


        protected void ShowCurrentVideo()
        {
            if (currentVid >= 0 && currentVid <= vidFiles.Length - 1)
            {
                myMediaElement.Source = new Uri(vidFiles[currentVid], UriKind.Absolute);
            }
        }

        protected void ShowNextImage()
        {
            int nextImg = currentImg + 1;

            if (nextImg > picFiles.Length - 1)
            {
                BitmapImage bm = new BitmapImage(new Uri(picFiles[0], UriKind.RelativeOrAbsolute));
                NextImageBox.Source = bm;
            }           
            else
            {
                BitmapImage bm = new BitmapImage(new Uri(picFiles[nextImg], UriKind.RelativeOrAbsolute));
                NextImageBox.Source = bm;
            }
        }

        protected void ShowSecondNextImage()
        {
            int Img = currentImg + 2;

            if (Img > picFiles.Length)
            {
                BitmapImage bm = new BitmapImage(new Uri(picFiles[1], UriKind.RelativeOrAbsolute));
                SecondNextImageBox.Source = bm;
            }
            else if (Img > picFiles.Length - 1)
            {
                BitmapImage bm = new BitmapImage(new Uri(picFiles[0], UriKind.RelativeOrAbsolute));
                SecondNextImageBox.Source = bm;
            }
            else
            {
                BitmapImage bm = new BitmapImage(new Uri(picFiles[Img], UriKind.RelativeOrAbsolute));
                SecondNextImageBox.Source = bm;
            }
        }

        protected void ShowSecondPreviousImage()
        {
            int Img = currentImg - 2;

            if (Img < -1)
            {
                BitmapImage bm = new BitmapImage(new Uri(picFiles[picFiles.Length - 2], UriKind.RelativeOrAbsolute));
                SecondPreviousImageBox.Source = bm;
            }
            else if (Img < 0)
            {
                BitmapImage bm = new BitmapImage(new Uri(picFiles[picFiles.Length - 1], UriKind.RelativeOrAbsolute));
                SecondPreviousImageBox.Source = bm;
            }
            else
            {
                BitmapImage bm = new BitmapImage(new Uri(picFiles[Img], UriKind.RelativeOrAbsolute));
                SecondPreviousImageBox.Source = bm;
            }
        }

        protected void ShowPreviousImage()
        {
            int Img = currentImg - 1;

            if (Img < 0)
            {
                BitmapImage bm = new BitmapImage(new Uri(picFiles[picFiles.Length - 1], UriKind.RelativeOrAbsolute));
                PreviousImageBox.Source = bm;
            }
            else
            {
                BitmapImage bm = new BitmapImage(new Uri(picFiles[Img], UriKind.RelativeOrAbsolute));
                PreviousImageBox.Source = bm;
            }
        }


        private void importBtn_Click(object sender, RoutedEventArgs e)
        {
            string[] ext = { ".jpg", ".jpeg", ".gif", ".png", ".bmp", ".tiff" };
            String directoryPath;
            OpenFileDialog openFileDialog = new OpenFileDialog();
            openFileDialog.Multiselect = true;
            openFileDialog.Filter = "JPEG|*.jpg;*.jpeg|Bitmaps|*.bmp|Gif|*.gif|PNG|*.png|TIFF|*.tiff";

            if (openFileDialog.ShowDialog() == System.Windows.Forms.DialogResult.OK)
            {
                directoryPath = System.IO.Path.GetDirectoryName(openFileDialog.FileName);
                picFiles = Directory.GetFiles(directoryPath, "*.*")
                .Where(f => ext.Contains(new FileInfo(f).Extension.ToLower())).ToArray();
                ShowCurrentImage();
                ShowNextImage();
                ShowSecondNextImage();
                ShowPreviousImage();
                ShowSecondPreviousImage();
            }
        }

        private void helpBtn_Click(object sender, RoutedEventArgs e)
        {
            Help h = new Help();
            h.Show();
        }

        private void fullscrnBtn_Click(object sender, RoutedEventArgs e)
        {
            fPic = new FullscreenPics(currentImg, this, picFiles);
            fullPic = true;
            fPic.Show();
        }

        private void fullscrnVidBtn_Click(object sender, RoutedEventArgs e)
        {
            myMediaElement.Pause();
            double timeIn = myMediaElement.Position.TotalMilliseconds;
            double maxTime = DurationSlider.Maximum;
            double volumeIn = volumeSlider.Value;
            double speedIn = speedRatioSlider.Value;
            fVid = new FullscreenVid(vidFiles, currentVid, isPlaying, timeIn, volumeIn, speedIn, maxTime, mediaOpened);
            fVid.Show();
            fullVid = true;
            isPlaying = false;
        }

        private void setFullVid()
        {
            myMediaElement.Pause();
            double timeIn = myMediaElement.Position.TotalMilliseconds;
            double maxTime = DurationSlider.Maximum;
            double volumeIn = volumeSlider.Value;
            double speedIn = speedRatioSlider.Value;
            fVid = new FullscreenVid(vidFiles, currentVid, isPlaying, timeIn, volumeIn, speedIn, maxTime, mediaOpened);
            fVid.Show();
            fullVid = true;
            isPlaying = false;
        }

        void OnMouseDownPlayMedia(object sender, MouseButtonEventArgs args)
        {

            // The Play method will begin the media if it is not currently active or  
            // resume media if it is paused. This has no effect if the media is 
            // already running.
            myMediaElement.Play();
            isPlaying = true;

            // Initialize the MediaElement property values.
            InitializePropertyValues();

        }

        // Pause the media. 
        void OnMouseDownPauseMedia(object sender, MouseButtonEventArgs args)
        {

            // The Pause method pauses the media if it is currently running. 
            // The Play method can be used to resume.
            myMediaElement.Pause();
            isPlaying = false;

        }

        // Pause the media. 
        void OnMouseOverPauseMedia(object sender, MouseButtonEventArgs args)
        {

            // The Pause method pauses the media if it is currently running. 
            // The Play method can be used to resume.
            myMediaElement.Pause();
            isPlaying = false;

        }

        // Stop the media. 
        void OnMouseDownStopMedia(object sender, MouseButtonEventArgs args)
        {

            // The Stop method stops and resets the media to be played from 
            // the beginning.
            myMediaElement.Stop();
            isPlaying = true;

        }

        // Change the volume of the media. 
        private void ChangeMediaVolume(object sender, RoutedPropertyChangedEventArgs<double> args)
        {
            try
            {
                myMediaElement.Volume = (double)volumeSlider.Value;
            }
            catch(NullReferenceException n)
            {
                
            }
                
        }


        // Change the speed of the media. 
        private void ChangeMediaSpeedRatio(object sender, RoutedPropertyChangedEventArgs<double> args)
        {
            myMediaElement.SpeedRatio = (double)speedRatioSlider.Value;
        }

        // When the media opens, initialize the "Seek To" slider maximum value 
        // to the total number of miliseconds in the length of the media clip. 
        private void Element_MediaOpened(object sender, EventArgs e)
        {
            mediaOpened = true;
            //timelineSlider.Maximum = myMediaElement.NaturalDuration.TimeSpan.TotalMilliseconds;
            //DurationSlider.Maximum = myMediaElement.NaturalDuration.TimeSpan.TotalMilliseconds;
            ticks.Interval = TimeSpan.FromMilliseconds(1);
            ticks.Tick += ticks_Tick;
            ticks.Start();
        }

        // When the media playback is finished. Stop() the media to seek to media start. 
        private void Element_MediaEnded(object sender, EventArgs e)
        {
            myMediaElement.Stop();
            isPlaying = false;
        }

        // Jump to different parts of the media (seek to).  
        private void SeekToMediaPosition(object sender, RoutedPropertyChangedEventArgs<double> args)
        {
            if(!isPlaying)
            {
                int SliderValue = (int)DurationSlider.Value;

                // Overloaded constructor takes the arguments days, hours, minutes, seconds, miniseconds. 
                // Create a TimeSpan with miliseconds equal to the slider value.
                TimeSpan ts = new TimeSpan(0, 0, 0, 0, SliderValue);
                myMediaElement.Position = ts;
            }
        }

        //Updating the Slider Value of Media(Video Duration) 
        void ticks_Tick(object sender, object e)
        {
            DurationSlider.Value = myMediaElement.Position.TotalMilliseconds;
            //DurationText.Text = Milliseconds_to_Minute((long)VideoPlayer.Position.TotalMilliseconds);
        }

        void InitializePropertyValues()
        {
            // Set the media's starting Volume and SpeedRatio to the current value of the 
            // their respective slider controls.
            myMediaElement.Volume = (double)volumeSlider.Value;
            myMediaElement.SpeedRatio = (double)speedRatioSlider.Value;
        }


        public void setCurrentImg(int n)
        {
            fullPic = false;
            currentImg = n;
            ShowCurrentImage();
            ShowNextImage();
            ShowSecondNextImage();
            ShowPreviousImage();
            ShowSecondPreviousImage();
        }

        private delegate void setImageDelegate(string controlName, BitmapFrame frame);

        private void setImage(string controlName, BitmapFrame frame)
        {
            var control = FindName(controlName);
            if (control != null)
                ((System.Windows.Controls.Image)control).Source = frame;
        }

        private void makeThumbnails(BitmapFrame frame, object state)
        {
            Dispatcher.Invoke(new setImageDelegate(setImage), (string)state, frame);
        }

        private void makeJpeg(BitmapFrame frame, object state)
        {
            var encoder = new JpegBitmapEncoder();
            encoder.Frames.Add(frame);

            string filename = (string)state + ".jpg";
            using (var fs = new FileStream(filename, FileMode.Create))
                encoder.Save(fs);
        }

        private void image_Button_Click(object sender, RoutedEventArgs e)
        {
            /*
            var source = myMediaElement.Source;

            VideoScreenShot.CaptureScreenAsync(source, new Dictionary<TimeSpan, object> { 
				{TimeSpan.FromSeconds(10), "image0"} ,
				{TimeSpan.FromSeconds(20), "image1"} ,
				{TimeSpan.FromSeconds(30), "image2"} ,
				{TimeSpan.FromSeconds(40), "image3"} ,
			}, .1, makeJpeg, makeThumbnails);
             */
            //VideoScreenShot.CaptureScreenAsync(source, TimeSpan.FromSeconds(10), "image0", .1, makeJpeg, makeThumbnails);
            //VideoScreenShot.CaptureScreenAsync(source, TimeSpan.FromSeconds(43) + TimeSpan.FromMilliseconds(760), "image1", makeThumbnails);
        }

        private void TabItem_MouseDown(object sender, MouseButtonEventArgs e)
        {

        }

        private void TabItem_GotFocus(object sender, RoutedEventArgs e)
        {
            tabOpened = true;
            myMediaElement.Play();
            isPlaying = true;
            if(tabOpened)
            {
                pauser.Interval = TimeSpan.FromMilliseconds(100);
            }
            else
            {
                pauser.Interval = TimeSpan.FromMilliseconds(650);
            }
            pauser.Tick += pause_Media;
            pauser.Start();
            /*
            var source = myMediaElement.Source;

            VideoScreenShot.CaptureScreenAsync(source, new Dictionary<TimeSpan, object> { 
				{TimeSpan.FromSeconds(10), "image0"} ,
				{TimeSpan.FromSeconds(20), "image1"} ,
				{TimeSpan.FromSeconds(30), "image2"} ,
				{TimeSpan.FromSeconds(40), "image3"} ,
			}, .1, makeJpeg, makeThumbnails);
             */ 
            //VideoScreenShot.CaptureScreenAsync(source, TimeSpan.FromSeconds(10), "image0", .1, makeJpeg, makeThumbnails);
            //VideoScreenShot.CaptureScreenAsync(source, TimeSpan.FromSeconds(43) + TimeSpan.FromMilliseconds(760), "image1", makeThumbnails);
        }

        //KINECT CODE
        void kinectSensorChooser1_KinectSensorChanged(object sender, DependencyPropertyChangedEventArgs e)
        {
            KinectSensor old = (KinectSensor)e.OldValue;

            StopKinect(old);

            KinectSensor sensor = (KinectSensor)e.NewValue;

            if (sensor == null)
            {
                return;
            }

            var parameters = new TransformSmoothParameters
            {
                Smoothing = 0.3f,
                Correction = 0.0f,
                Prediction = 0.0f,
                JitterRadius = 1.0f,
                MaxDeviationRadius = 0.5f
            };
            sensor.SkeletonStream.Enable(parameters);

            sensor.SkeletonStream.Enable();

            sensor.AllFramesReady += new EventHandler<AllFramesReadyEventArgs>(sensor_AllFramesReady);
            sensor.DepthStream.Enable(DepthImageFormat.Resolution640x480Fps30);
            sensor.ColorStream.Enable(ColorImageFormat.RgbResolution640x480Fps30);

            try
            {
                sensor.Start();
               
            }
            catch (System.IO.IOException)
            {
                kinectSensorChooser1.AppConflictOccurred();
            }
        }


        void sensor_AllFramesReady(object sender, AllFramesReadyEventArgs e)
        {
            if (closing)
            {
                return;
            }

            //Get a skeleton
            Skeleton first = GetFirstSkeleton(e);

            if (first == null)
            {
                return;
            }


            //set scaled position
            //ScalePosition(headImage, first.Joints[JointType.Head]);
            ScalePosition(leftEllipse, first.Joints[JointType.HandLeft]);
            ScalePosition(rightEllipse, first.Joints[JointType.HandRight]);

            if (checking)
            {
                ProcessGesture(first.Joints[JointType.Head], first.Joints[JointType.HandLeft], first.Joints[JointType.HandRight]);
            }
            GetCameraPoint(first, e);

            Joint ScaledJoint = first.Joints[JointType.HandRight].ScaleTo(1920, 1080, .3f, .3f);

            int topofscreen;
            int leftofscreen;

            leftofscreen = Convert.ToInt32(ScaledJoint.Position.X);
            topofscreen = Convert.ToInt32(ScaledJoint.Position.Y);

            SetCursorPos(leftofscreen, topofscreen);

        }


        //gESTURES
        private void ProcessGesture(Joint head, Joint left, Joint right)
        {

            if (right.Position.Y > head.Position.Y && left.Position.Y > head.Position.Y)
            {
                if (tabPictures.IsSelected)
                {
                    if (fullPic)
                    {
                        fPic.exit();
                        fullPic = false;
                    }
                    else
                    {
                        fPic = new FullscreenPics(currentImg, this, picFiles);
                        fullPic = true;
                        fPic.Show();
                    }
                }
                else if (tabVideos.IsSelected)
                {
                    if (fullVid)
                    {
                        fVid.exit();
                        fullVid = false;
                    }
                    else
                    {
                        setFullVid();
                    }
                }
                checking = false;
                waiting.Interval = TimeSpan.FromMilliseconds(2000);
                waiting.Tick += stop_Waiting;
                waiting.Start();
            }
            else if (right.Position.Y < left.Position.Y && right.Position.X < left.Position.X)
            {
                if (tabPictures.IsSelected)
                {
                    if (picFiles.Length > 0)
                    {
                        if (fullPic == true)
                        {
                            fPic.next();
                        }
                        else
                        {
                            currentImg = currentImg == picFiles.Length - 1 ? 0 : ++currentImg;
                            ShowCurrentImage();
                            ShowNextImage();
                            ShowSecondNextImage();
                            ShowPreviousImage();
                            ShowSecondPreviousImage();
                            
                        }
                    }
                }
                else if (tabVideos.IsSelected)
                {
                    if (vidFiles.Length > 0)
                    {
                        if (fullVid == true)
                        {
                            fVid.next();
                        }
                        else
                        {
                            myMediaElement.Stop();
                            isPlaying = false;

                            currentVid = currentVid == vidFiles.Length - 1 ? 0 : ++currentVid;
                            ShowCurrentVideo();
                        }
                    }
                }


                checking = false;
                waiting.Interval = TimeSpan.FromMilliseconds(1000);
                waiting.Tick += stop_Waiting;
                waiting.Start();

            }
            else if (right.Position.Y > left.Position.Y && right.Position.X < left.Position.X)
            {
                if (tabPictures.IsSelected)
                {
                    if (picFiles.Length > 0)
                    {
                        if (fullPic == true)
                        {
                            fPic.previous();
                        }
                        else
                        {
                            currentImg = currentImg == 0 ? picFiles.Length - 1 : --currentImg;
                            ShowCurrentImage();
                            ShowNextImage();
                            ShowSecondNextImage();
                            ShowPreviousImage();
                            ShowSecondPreviousImage();
                            
                        }
                    }
                }
                else if (tabVideos.IsSelected)
                {
                    if (vidFiles.Length > 0)
                    {
                        if (fullVid == true)
                        {
                            fVid.next();
                        }
                        else
                        {
                            
                            myMediaElement.Stop();
                            isPlaying = false;
                            
                            currentVid = currentVid == 0 ? vidFiles.Length - 1 : --currentVid;
                            ShowCurrentVideo();
                            
                        }
                    }
                }
                checking = false;
                waiting.Interval = TimeSpan.FromMilliseconds(1000);
                waiting.Tick += stop_Waiting;
                waiting.Start();   
            }
    
        }

        public void CaptureScreen(double x, double y, double width, double height)
        {
            int ix, iy, iw, ih;
            ix = Convert.ToInt32(x);
            iy = Convert.ToInt32(y);
            iw = Convert.ToInt32(width);
            ih = Convert.ToInt32(height);

            //  set the kinect hand icon invisible 
            //kinectButton.Visibility = System.Windows.Visibility.Collapsed;
            //kinectButton2.Visibility = System.Windows.Visibility.Collapsed;
            Bitmap image = new Bitmap(iw, ih,
                   System.Drawing.Imaging.PixelFormat.Format32bppArgb);
            Graphics g = Graphics.FromImage(image);
            g.CopyFromScreen(ix, iy, 0,0 ,
                     new System.Drawing.Size(iw, ih),
                     CopyPixelOperation.SourceCopy);
            String i = Environment.GetFolderPath(Environment.SpecialFolder.MyPictures);
            Console.Write(i);
            String fileName = DateTime.Now.ToString("yyyy-MM-dd-HH-mm-ss");
            image.Save(i+ "\\" + fileName +".jpg");
            //desk_input();
        }

        

        void GetCameraPoint(Skeleton first, AllFramesReadyEventArgs e)
        {

            using (DepthImageFrame depth = e.OpenDepthImageFrame())
            {
                if (depth == null ||
                    kinectSensorChooser1.Kinect == null)
                {
                    return;
                }


                //Map a joint location to a point on the depth map
                //head
                DepthImagePoint headDepthPoint =
                    depth.MapFromSkeletonPoint(first.Joints[JointType.Head].Position);
                //left hand
                DepthImagePoint leftDepthPoint =
                    depth.MapFromSkeletonPoint(first.Joints[JointType.HandLeft].Position);
                //right hand
                DepthImagePoint rightDepthPoint =
                    depth.MapFromSkeletonPoint(first.Joints[JointType.HandRight].Position);


                //Map a depth point to a point on the color image
                //head
                ColorImagePoint headColorPoint =
                    depth.MapToColorImagePoint(headDepthPoint.X, headDepthPoint.Y,
                    ColorImageFormat.RgbResolution640x480Fps30);
                //left hand
                ColorImagePoint leftColorPoint =
                    depth.MapToColorImagePoint(leftDepthPoint.X, leftDepthPoint.Y,
                    ColorImageFormat.RgbResolution640x480Fps30);
                //right hand
                ColorImagePoint rightColorPoint =
                    depth.MapToColorImagePoint(rightDepthPoint.X, rightDepthPoint.Y,
                    ColorImageFormat.RgbResolution640x480Fps30);


                //Set location
                //CameraPosition(headImage, headColorPoint);
                //CameraPosition(leftEllipse, leftColorPoint);
                //CameraPosition(rightEllipse, rightColorPoint);
            }
        }


        Skeleton GetFirstSkeleton(AllFramesReadyEventArgs e)
        {
            using (SkeletonFrame skeletonFrameData = e.OpenSkeletonFrame())
            {
                if (skeletonFrameData == null)
                {
                    return null;
                }


                skeletonFrameData.CopySkeletonDataTo(allSkeletons);

                //get the first tracked skeleton
                Skeleton first = (from s in allSkeletons
                                  where s.TrackingState == SkeletonTrackingState.Tracked
                                  select s).FirstOrDefault();

                return first;

            }
        }

        private void StopKinect(KinectSensor sensor)
        {
            if (sensor != null)
            {
                if (sensor.IsRunning)
                {
                    //stop sensor 
                    sensor.Stop();

                    //stop audio if not null
                    if (sensor.AudioSource != null)
                    {
                        sensor.AudioSource.Stop();
                    }


                }
            }
        }

        private void CameraPosition(FrameworkElement element, ColorImagePoint point)
        {
            //Divide by 2 for width and height so point is right in the middle 
            // instead of in top/left corner
            Canvas.SetLeft(element, point.X - element.Width / 2);
            Canvas.SetTop(element, point.Y - element.Height / 2);

        }

        private void ScalePosition(FrameworkElement element, Joint joint)
        {
            //convert the value to X/Y
            //Joint scaledJoint = joint.ScaleTo(1280, 720); 

            //convert & scale (.3 = means 1/3 of joint distance)
            Joint scaledJoint = joint.ScaleTo(1280, 720, .3f, .3f);

            Canvas.SetLeft(element, scaledJoint.Position.X);
            Canvas.SetTop(element, scaledJoint.Position.Y);

        }
        private void vid_Next_Click(object sender, RoutedEventArgs e)
        {
            myMediaElement.Stop();
            isPlaying = false;
            if (vidFiles.Length > 0)
            {
                currentVid = currentVid == vidFiles.Length - 1 ? 0 : ++currentVid;

                ShowCurrentVideo();
            }
            /*
            var source = myMediaElement.Source;

            VideoScreenShot.CaptureScreenAsync(source, new Dictionary<TimeSpan, object> { 
				{TimeSpan.FromSeconds(5), "image1"} ,
			}, .1, makeJpeg, makeThumbnails);
             */
        }

        private void vid_Prev_Click(object sender, RoutedEventArgs e)
        {
            myMediaElement.Stop();
            isPlaying = false;
            if (vidFiles.Length > 0)
            {
                currentVid = currentVid == 0 ? vidFiles.Length - 1 : --currentVid;
                ShowCurrentVideo();
            }
            /*
            var source = myMediaElement.Source;

            VideoScreenShot.CaptureScreenAsync(source, new Dictionary<TimeSpan, object> { 
				{TimeSpan.FromSeconds(5), "image1"} ,
			}, .1, makeJpeg, makeThumbnails);
             */
        }

        private void slideBtn_Click(object sender, RoutedEventArgs e)
        {
            FullscreenPics f = new FullscreenPics(currentImg, this, picFiles);
            f.Show();
            f.slideShow();
        }

        private void ImportVideos_Click(object sender, RoutedEventArgs e)
        {
            string[] v_ext = { ".mp4", ".wmv", ".wma", ".mov", ".avi" };
            String directoryPath;
            OpenFileDialog openFileDialog = new OpenFileDialog();
            openFileDialog.Multiselect = true;
            openFileDialog.Filter = "MP4|*.mp4|WMV|*.wmv|WMA|*.wma|MOV|*.mov|AVI|*.avi";

            if (openFileDialog.ShowDialog() == System.Windows.Forms.DialogResult.OK)
            {
                directoryPath = System.IO.Path.GetDirectoryName(openFileDialog.FileName);
                vidFiles = Directory.GetFiles(directoryPath, "*.*")
                    .Where(g => v_ext.Contains(new FileInfo(g).Extension.ToLower())).ToArray();
                Console.WriteLine(vidFiles.Length);
                Console.WriteLine("hello");
                currentVid = 0;
                myMediaElement.Source = new Uri(vidFiles[currentVid], UriKind.Absolute);
            }
        }


        private void Window_Closing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            closing = true;
            StopKinect(kinectSensorChooser1.Kinect);
        }



        //Sam's Code for mouse
        private void helpBtn_MouseLeave(object sender, System.Windows.Input.MouseEventArgs e)
        {
            buttonTimer.Tick -= pressHelp;
            buttonTimer.Stop();
            this.Cursor = System.Windows.Input.Cursors.Hand;
        }

        private void helpBtn_MouseEnter(object sender, System.Windows.Input.MouseEventArgs e)
        {
            buttonTimer.Interval = TimeSpan.FromMilliseconds(2000);
            buttonTimer.Tick += pressHelp;
            buttonTimer.Start();
            this.Cursor = System.Windows.Input.Cursors.Wait;
        }

        void pressHelp(object sender, object e)
        {
            Help h = new Help();
            h.Show();
            buttonTimer.Stop();
            this.Cursor = System.Windows.Input.Cursors.Hand;
        }

        private void importBtn_MouseLeave(object sender, System.Windows.Input.MouseEventArgs e)
        {
            buttonTimer.Tick -= pressImport;
            buttonTimer.Stop();
            this.Cursor = System.Windows.Input.Cursors.Hand;
        }

        private void importBtn_MouseEnter(object sender, System.Windows.Input.MouseEventArgs e)
        {
            buttonTimer.Interval = TimeSpan.FromMilliseconds(2000);
            buttonTimer.Tick += pressImport;
            buttonTimer.Start();
            this.Cursor = System.Windows.Input.Cursors.Wait;
        }

        void pressImport(object sender, object e)
        {
            string[] ext = { ".jpg", ".jpeg", ".gif", ".png", ".bmp", ".tiff" };
            String directoryPath;
            OpenFileDialog openFileDialog = new OpenFileDialog();
            openFileDialog.Multiselect = true;
            openFileDialog.Filter = "JPEG|*.jpg;*.jpeg|Bitmaps|*.bmp|Gif|*.gif|PNG|*.png|TIFF|*.tiff";

            if (openFileDialog.ShowDialog() == System.Windows.Forms.DialogResult.OK)
            {
                directoryPath = System.IO.Path.GetDirectoryName(openFileDialog.FileName);
                picFiles = Directory.GetFiles(directoryPath, "*.*")
                .Where(f => ext.Contains(new FileInfo(f).Extension.ToLower())).ToArray();
                ShowCurrentImage();
                ShowNextImage();
                ShowSecondNextImage();
                ShowPreviousImage();
                ShowSecondPreviousImage();
            }
            buttonTimer.Stop();
            this.Cursor = System.Windows.Input.Cursors.Hand;
        }

        private void importVideosBtn_MouseLeave(object sender, System.Windows.Input.MouseEventArgs e)
        {
            buttonTimer.Tick -= pressImportVideos;
            buttonTimer.Stop();
            this.Cursor = System.Windows.Input.Cursors.Hand;
        }

        private void importVideosBtn_MouseEnter(object sender, System.Windows.Input.MouseEventArgs e)
        {
            buttonTimer.Interval = TimeSpan.FromMilliseconds(2000);
            buttonTimer.Tick += pressImportVideos;
            buttonTimer.Start();
            this.Cursor = System.Windows.Input.Cursors.Wait;
        }

        void pressImportVideos(object sender, object e)
        {
            string[] v_ext = { ".mp4", ".wmv", ".wma", ".mov", ".avi" };
            String directoryPath;
            OpenFileDialog openFileDialog = new OpenFileDialog();
            openFileDialog.Multiselect = true;
            openFileDialog.Filter = "MP4|*.mp4|WMV|*.wmv|WMA|*.wma|MOV|*.mov|AVI|*.avi";

            if (openFileDialog.ShowDialog() == System.Windows.Forms.DialogResult.OK)
            {
                directoryPath = System.IO.Path.GetDirectoryName(openFileDialog.FileName);
                vidFiles = Directory.GetFiles(directoryPath, "*.*")
                    .Where(g => v_ext.Contains(new FileInfo(g).Extension.ToLower())).ToArray();
                Console.WriteLine(vidFiles.Length);
                Console.WriteLine("hello");
                currentVid = 0;
                myMediaElement.Source = new Uri(vidFiles[currentVid], UriKind.Absolute);
            }
            buttonTimer.Stop();
            this.Cursor = System.Windows.Input.Cursors.Hand;
        }

        private void slideBtn_MouseLeave(object sender, System.Windows.Input.MouseEventArgs e)
        {
            buttonTimer.Tick -= pressSlide;
            buttonTimer.Stop();
            this.Cursor = System.Windows.Input.Cursors.Hand;
        }

        private void slideBtn_MouseEnter(object sender, System.Windows.Input.MouseEventArgs e)
        {
            buttonTimer.Interval = TimeSpan.FromMilliseconds(2000);
            buttonTimer.Tick += pressSlide;
            buttonTimer.Start();
            this.Cursor = System.Windows.Input.Cursors.Wait;
        }

        void pressSlide(object sender, object e)
        {
            FullscreenPics f = new FullscreenPics(currentImg, this, picFiles);
            f.Show();
            f.slideShow();
            buttonTimer.Stop();
            this.Cursor = System.Windows.Input.Cursors.Hand;
        }

        private void toolsTab_MouseLeave(object sender, System.Windows.Input.MouseEventArgs e)
        {
            buttonTimer.Tick -= pressToolsTab;
            buttonTimer.Stop();
            this.Cursor = System.Windows.Input.Cursors.Hand;
        }

        private void toolsTab_MouseEnter(object sender, System.Windows.Input.MouseEventArgs e)
        {
            buttonTimer.Interval = TimeSpan.FromMilliseconds(2000);
            buttonTimer.Tick += pressToolsTab;
            buttonTimer.Start();
            this.Cursor = System.Windows.Input.Cursors.Wait;
        }

        void pressToolsTab(object sender, object e)
        {
            mainTab.SelectedIndex = 3;
            buttonTimer.Stop();
            this.Cursor = System.Windows.Input.Cursors.Hand;
        }

        private void cameraTab_MouseLeave(object sender, System.Windows.Input.MouseEventArgs e)
        {
            buttonTimer.Tick -= pressCameraTab;
            buttonTimer.Stop();
            this.Cursor = System.Windows.Input.Cursors.Hand;
        }

        private void cameraTab_MouseEnter(object sender, System.Windows.Input.MouseEventArgs e)
        {
            buttonTimer.Interval = TimeSpan.FromMilliseconds(2000);
            buttonTimer.Tick += pressCameraTab;
            buttonTimer.Start();
            this.Cursor = System.Windows.Input.Cursors.Wait;
        }

        void pressCameraTab(object sender, object e)
        {
            mainTab.SelectedIndex = 2;
            buttonTimer.Stop();
            this.Cursor = System.Windows.Input.Cursors.Hand;
        }

        private void videoTab_MouseLeave(object sender, System.Windows.Input.MouseEventArgs e)
        {
            buttonTimer.Tick -= pressVideoTab;
            buttonTimer.Stop();
            this.Cursor = System.Windows.Input.Cursors.Hand;
        }

        private void videoTab_MouseEnter(object sender, System.Windows.Input.MouseEventArgs e)
        {
            buttonTimer.Interval = TimeSpan.FromMilliseconds(2000);
            buttonTimer.Tick += pressVideoTab;
            buttonTimer.Start();
            this.Cursor = System.Windows.Input.Cursors.Wait;
        }

        void pressVideoTab(object sender, object e)
        {
            mainTab.SelectedIndex = 1;
            buttonTimer.Stop();
            this.Cursor = System.Windows.Input.Cursors.Hand;
        }

        private void pictureTab_MouseLeave(object sender, System.Windows.Input.MouseEventArgs e)
        {
            buttonTimer.Tick -= pressPictureTab;
            buttonTimer.Stop();
            this.Cursor = System.Windows.Input.Cursors.Hand;
        }

        private void pictureTab_MouseEnter(object sender, System.Windows.Input.MouseEventArgs e)
        {
            buttonTimer.Interval = TimeSpan.FromMilliseconds(2000);
            buttonTimer.Tick += pressPictureTab;
            buttonTimer.Start();
            this.Cursor = System.Windows.Input.Cursors.Wait;
        }

        void pressPictureTab(object sender, object e)
        {
            mainTab.SelectedIndex = 0;
            buttonTimer.Stop();
            this.Cursor = System.Windows.Input.Cursors.Hand;
        }

        private void playBtn_MouseLeave(object sender, System.Windows.Input.MouseEventArgs e)
        {
            buttonTimer.Tick -= pressPlayBtn;
            buttonTimer.Stop();
            this.Cursor = System.Windows.Input.Cursors.Hand;
        }

        private void playBtn_MouseEnter(object sender, System.Windows.Input.MouseEventArgs e)
        {
            buttonTimer.Interval = TimeSpan.FromMilliseconds(2000);
            buttonTimer.Tick += pressPlayBtn;
            buttonTimer.Start();
            this.Cursor = System.Windows.Input.Cursors.Wait;
        }

        void pressPlayBtn(object sender, object e)
        {
            // The Play method will begin the media if it is not currently active or  
            // resume media if it is paused. This has no effect if the media is 
            // already running.
            myMediaElement.Play();
            isPlaying = true;

            // Initialize the MediaElement property values.
            InitializePropertyValues();
            buttonTimer.Stop();
            this.Cursor = System.Windows.Input.Cursors.Hand;
        }

        private void pauseBtn_MouseLeave(object sender, System.Windows.Input.MouseEventArgs e)
        {
            buttonTimer.Tick -= pressPauseBtn;
            buttonTimer.Stop();
            this.Cursor = System.Windows.Input.Cursors.Hand;
        }

        private void pauseBtn_MouseEnter(object sender, System.Windows.Input.MouseEventArgs e)
        {
            buttonTimer.Interval = TimeSpan.FromMilliseconds(2000);
            buttonTimer.Tick += pressPauseBtn;
            buttonTimer.Start();
            this.Cursor = System.Windows.Input.Cursors.Wait;
        }

        void pressPauseBtn(object sender, object e)
        {
            myMediaElement.Pause();
            isPlaying = false;
            buttonTimer.Stop();
            this.Cursor = System.Windows.Input.Cursors.Hand;
        }

        private void stopBtn_MouseLeave(object sender, System.Windows.Input.MouseEventArgs e)
        {
            buttonTimer.Tick -= pressStopBtn;
            buttonTimer.Stop();
            this.Cursor = System.Windows.Input.Cursors.Hand;
        }

        private void stopBtn_MouseEnter(object sender, System.Windows.Input.MouseEventArgs e)
        {
            buttonTimer.Interval = TimeSpan.FromMilliseconds(2000);
            buttonTimer.Tick += pressStopBtn;
            buttonTimer.Start();
            this.Cursor = System.Windows.Input.Cursors.Wait;
        }

        void pressStopBtn(object sender, object e)
        {
            myMediaElement.Stop();
            isPlaying = false;
            buttonTimer.Stop();
            this.Cursor = System.Windows.Input.Cursors.Hand;
        }

        private void cameraButton_MouseLeave(object sender, System.Windows.Input.MouseEventArgs e)
        {
            buttonTimer.Tick -= pressCameraButton;
            buttonTimer.Stop();
            this.Cursor = System.Windows.Input.Cursors.Hand;
        }

        private void cameraButton_MouseEnter(object sender, System.Windows.Input.MouseEventArgs e)
        {
            buttonTimer.Interval = TimeSpan.FromMilliseconds(2000);
            buttonTimer.Tick += pressCameraButton;
            buttonTimer.Start();
            this.Cursor = System.Windows.Input.Cursors.Wait;
        }

        void pressCameraButton(object sender, object e)
        {
            pauser.Interval = TimeSpan.FromMilliseconds(3000);
            pauser.Tick += pressCapture;
            pauser.Start();
            playerCount.Play();
            buttonTimer.Stop();
            this.Cursor = System.Windows.Input.Cursors.Hand;
        }

        private void vidNext_MouseLeave(object sender, System.Windows.Input.MouseEventArgs e)
        {
            buttonTimer.Tick -= pressVidNextButton;
            buttonTimer.Stop();
            this.Cursor = System.Windows.Input.Cursors.Hand;
        }

        private void vidNext_MouseEnter(object sender, System.Windows.Input.MouseEventArgs e)
        {
            buttonTimer.Interval = TimeSpan.FromMilliseconds(2000);
            buttonTimer.Tick += pressVidNextButton;
            buttonTimer.Start();
            this.Cursor = System.Windows.Input.Cursors.Wait;
        }

        void pressVidNextButton(object sender, object e)
        {
            myMediaElement.Stop();
            isPlaying = false;
            if (vidFiles.Length > 0)
            {
                currentVid = currentVid == vidFiles.Length - 1 ? 0 : ++currentVid;

                ShowCurrentVideo();
            }
            buttonTimer.Stop();
            this.Cursor = System.Windows.Input.Cursors.Hand;
        }

        private void vidPrev_MouseLeave(object sender, System.Windows.Input.MouseEventArgs e)
        {
            buttonTimer.Tick -= pressVidPrevButton;
            buttonTimer.Stop();
            this.Cursor = System.Windows.Input.Cursors.Hand;
        }

        private void vidPrev_MouseEnter(object sender, System.Windows.Input.MouseEventArgs e)
        {
            buttonTimer.Interval = TimeSpan.FromMilliseconds(2000);
            buttonTimer.Tick += pressVidPrevButton;
            buttonTimer.Start();
            this.Cursor = System.Windows.Input.Cursors.Wait;
        }

        void pressVidPrevButton(object sender, object e)
        {
            myMediaElement.Stop();
            isPlaying = false;
            if (vidFiles.Length > 0)
            {
                currentVid = currentVid == 0 ? vidFiles.Length - 1 : --currentVid;
                ShowCurrentVideo();
            }
            buttonTimer.Stop();
            this.Cursor = System.Windows.Input.Cursors.Hand;
        }

        private void videoFullsrn_MouseLeave(object sender, System.Windows.Input.MouseEventArgs e)
        {
            buttonTimer.Tick -= pressVideoFullsrnButton;
            buttonTimer.Stop();
            this.Cursor = System.Windows.Input.Cursors.Hand;
        }

        private void videoFullsrn_MouseEnter(object sender, System.Windows.Input.MouseEventArgs e)
        {
            buttonTimer.Interval = TimeSpan.FromMilliseconds(2000);
            buttonTimer.Tick += pressVideoFullsrnButton;
            buttonTimer.Start();
            this.Cursor = System.Windows.Input.Cursors.Wait;
        }

        void pressVideoFullsrnButton(object sender, object e)
        {
            myMediaElement.Pause();
            double timeIn = myMediaElement.Position.TotalMilliseconds;
            double maxTime = DurationSlider.Maximum;
            double volumeIn = volumeSlider.Value;
            double speedIn = speedRatioSlider.Value;
            FullscreenVid v = new FullscreenVid(vidFiles, currentVid, isPlaying, timeIn, volumeIn, speedIn, maxTime, mediaOpened);
            v.Show();
            isPlaying = false;
            buttonTimer.Stop();
            this.Cursor = System.Windows.Input.Cursors.Hand;
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
                ShowNextImage();
                ShowSecondNextImage();
                ShowPreviousImage();
                ShowSecondPreviousImage();
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
                ShowNextImage();
                ShowSecondNextImage();
                ShowPreviousImage();
                ShowSecondPreviousImage();
            }
            buttonTimer.Stop();
            this.Cursor = System.Windows.Input.Cursors.Hand;
        }

        private void fullsrn_MouseLeave(object sender, System.Windows.Input.MouseEventArgs e)
        {
            buttonTimer.Tick -= pressFullsrnButton;
            buttonTimer.Stop();
            this.Cursor = System.Windows.Input.Cursors.Hand;
        }

        private void fullsrn_MouseEnter(object sender, System.Windows.Input.MouseEventArgs e)
        {
            buttonTimer.Interval = TimeSpan.FromMilliseconds(2000);
            buttonTimer.Tick += pressFullsrnButton;
            buttonTimer.Start();
            this.Cursor = System.Windows.Input.Cursors.Wait;
        }

        void pressFullsrnButton(object sender, object e)
        {
            FullscreenPics f = new FullscreenPics(currentImg, this, picFiles);
            f.Show();
            buttonTimer.Stop();
            this.Cursor = System.Windows.Input.Cursors.Hand;
        }


    }

}
