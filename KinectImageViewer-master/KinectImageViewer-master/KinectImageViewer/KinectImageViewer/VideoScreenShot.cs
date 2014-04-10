using System;
using System.Collections.Generic;
using System.Threading;
using System.Windows;
using System.Windows.Media;
using System.Windows.Media.Imaging;

namespace KinectImageViewer
{
	public class VideoScreenShot
	{
		public delegate void CaptureWorkerDelegate(BitmapFrame frame, object state);
		public static void CaptureScreenAsync(Uri source, TimeSpan timeSpan, object state, CaptureWorkerDelegate finalWorkerPrimary)
		{
			CaptureScreenAsync(source, timeSpan, state, -1, finalWorkerPrimary, null);
		}

		public static void CaptureScreenAsync(Uri source, Dictionary<TimeSpan, object> captureList, CaptureWorkerDelegate finalWorkerPrimary)
		{
			CaptureScreenAsync(source, captureList, -1, finalWorkerPrimary, null);
		}

		public static void CaptureScreenAsync(Uri source, TimeSpan timeSpan, object state, double scale, CaptureWorkerDelegate finalWorkerPrimary, CaptureWorkerDelegate finalWorkerThumbnail)
		{
			CaptureScreenAsync(source, new Dictionary<TimeSpan, object> { { timeSpan, state } }, scale, finalWorkerPrimary, finalWorkerThumbnail);
		}

		public static void CaptureScreenAsync(Uri source, Dictionary<TimeSpan, object> captureList, double scale, CaptureWorkerDelegate finalWorkerPrimary, CaptureWorkerDelegate finalWorkerThumbnail)
		{
			ThreadPool.QueueUserWorkItem(delegate { CaptureScreen(source, captureList, scale, finalWorkerPrimary, finalWorkerThumbnail); });
		}

		public static void CaptureScreen(Uri source, TimeSpan timeSpan, object state, CaptureWorkerDelegate finalWorkerPrimary)
		{
			CaptureScreen(source, timeSpan, state, -1, finalWorkerPrimary, null);
		}

		public static void CaptureScreen(Uri source, Dictionary<TimeSpan, object> captureList, CaptureWorkerDelegate finalWorkerPrimary)
		{
			CaptureScreen(source, captureList, -1, finalWorkerPrimary, null);
		}

		public static void CaptureScreen(Uri source, TimeSpan timeSpan, object state, double scale, CaptureWorkerDelegate finalWorkerPrimary, CaptureWorkerDelegate finalWorkerThumbnail)
		{
			CaptureScreen(source, new Dictionary<TimeSpan, object> { { timeSpan, state } }, scale, finalWorkerPrimary, finalWorkerThumbnail);
		}

		public static void CaptureScreen(Uri source, Dictionary<TimeSpan, object> captureList, double scale, CaptureWorkerDelegate finalWorkerPrimary, CaptureWorkerDelegate finalWorkerThumbnail)
		{
			var mutexLock = new Mutex(false, source.GetHashCode().ToString());
			mutexLock.WaitOne();

			var player = new MediaPlayer { Volume = 0, ScrubbingEnabled = true };

			player.Open(source);
			player.Pause();
			foreach (var pair in captureList)
			{
				var timeSpan = pair.Key;
				var state = pair.Value;

				player.Position = timeSpan;
				Thread.Sleep(1000);

				var width = player.NaturalVideoWidth;
				var height = player.NaturalVideoHeight;

				var rtb = new RenderTargetBitmap(width, height, 96, 96, PixelFormats.Pbgra32);
				var dv = new DrawingVisual();

				using (DrawingContext dc = dv.RenderOpen())
					dc.DrawVideo(player, new Rect(0, 0, width, height));

				rtb.Render(dv);
				var frame = BitmapFrame.Create(rtb).GetCurrentValueAsFrozen();
				if (finalWorkerPrimary != null)
					finalWorkerPrimary(frame as BitmapFrame, state);

				if (scale > 0 && finalWorkerThumbnail != null)
				{
					var thumbnailFrame =
						BitmapFrame.Create(new TransformedBitmap(frame as BitmapSource, new ScaleTransform(scale, scale))).
							GetCurrentValueAsFrozen();
					var encoder = new JpegBitmapEncoder();
					encoder.Frames.Add(thumbnailFrame as BitmapFrame);

					finalWorkerThumbnail(thumbnailFrame as BitmapFrame, state);
				}
			}
			player.Close();
			mutexLock.ReleaseMutex();
		}
	}
}
