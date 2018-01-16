using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Emgu.CV;
using Emgu.CV.Structure;
using Microsoft.Kinect;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.IO;
using System.Drawing;


namespace Microsoft.Samples.Kinect.BodyBasics
{
    class vidRec
    {
        public static bool PauseXef;
        public static bool ResumeXef;
        byte[] pixels = null, temp;
        ///<summary>
        /// dimentions
        ///</summary>
        static public int width, height;

        public static List<int> timeInterval = new List<int>();

        ///<summary>
        /// list to save all the needed images frame by frame
        ///</summary>
        // public static List<ImageSource> ISimages = new List<ImageSource>();
        public static byte[] data;

        Image<Bgr, byte> img;

        public VideoWriter VideoW;

        double firstFrame = 0;

        int stride = width * PixelFormats.Bgra32.BitsPerPixel / 8;
        int newstride = width * PixelFormats.Bgr24.BitsPerPixel / 8;
        int size = 0;
        int tmpLastTimeInterval = 0;

        /// <summary>
        /// number of each picture
        /// </summary>
        public static int video_frame = 0;
        public static int kinect_frame = 0;

        ///<summary>
        /// list to save corresponding timestamps
        ///</summary>
        //public static double[] TimeStamps;
        public static List<int> timeStamps = new List<int>();

        Mat frame;

        bool running = false;

        /// <summary>
        /// save image to array?
        /// </summary>
        /// <param name="Col_Frame"></param>
        public void Col_Handler(ColorFrame Col_Frame)
        {
            PauseXef = false;
            ResumeXef = false;
            pixels = new byte[size];


            if (Col_Frame == null) return;

            /// If the frame has been correctly acquired, move on 
            if (Col_Frame.RawColorImageFormat == ColorImageFormat.Bgra)
            {
                Col_Frame.CopyRawFrameDataToArray(pixels);
            }
            else
            {
                Col_Frame.CopyConvertedFrameDataToArray(pixels, ColorImageFormat.Bgra);
            }

            //get frame dimentions
            width = Col_Frame.FrameDescription.Width;
            height = Col_Frame.FrameDescription.Height;
            int stride = width * PixelFormats.Bgra32.BitsPerPixel / 8;
            int newstride = width * PixelFormats.Bgr24.BitsPerPixel / 8;

            ///disply the video
            // color_Image.Source = bmpSource;
            ///Aquire the time stamps and framerates

            if (kinect_frame == 0 ) //&& !Video_Func.done
            {
                firstFrame = Col_Frame.RelativeTime.TotalMilliseconds;
                timeStamps.Add(0);
                timeInterval.Add(30);
            }
            else
            {
                // Console.WriteLine( "as (long) "+(long)(Col_Frame.RelativeTime.TotalMilliseconds -firstFrame) +" as int " + (int)(Col_Frame.RelativeTime.TotalMilliseconds - firstFrame));
                tmpLastTimeInterval = (int)(Col_Frame.RelativeTime.TotalMilliseconds - firstFrame);

                //  if (tmpLastTimeStamp != 0)
                timeInterval.Add(tmpLastTimeInterval - timeStamps[timeStamps.Count - 1]);
                Console.WriteLine(kinect_frame + "th element's timestamp is " + (tmpLastTimeInterval) + "  sample period in ms " + timeInterval[kinect_frame]);

                timeStamps.Add(tmpLastTimeInterval);
            }

            temp = new byte[stride / 2];

            // cycles through each row and only copies the center pixel data to a new array
            // cuts off a quarter of each frame on either side
            for (int i = 0; i < Col_Frame.FrameDescription.Height; i++)
            {
                int halfstep = stride / 2;
                int srcIndex = (i * stride) + (halfstep / 2);

                Array.ConstrainedCopy(pixels, srcIndex, temp, 0, halfstep);

                for (int j = 0; j < temp.Length / 4; j++)
                {
                    Array.ConstrainedCopy(temp, j * 4, data, (j * 3) + (i * newstride / 2), 3);
                }
            }

            // save the image to array
            PauseXef = true;

            SaveResults();
            ResumeXef = true;
            video_frame ++;
            kinect_frame ++;
        }


        // counts the number of times saveresults has been called
        int cc = 0;

        /// <summary>
        /// save MAT file to video when vide is NOT running
        /// </summary>
        public void SaveResults()
        {
            if (data != null && !running)
            {
                running = true;
                img.Bytes = data;
                img.Flip(Emgu.CV.CvEnum.FlipType.Horizontal);
                frame = img.Mat;

                //Action action = delegate { image.Source = BitmapToImageSource(frame.Bitmap); };
                //image.Dispatcher.Invoke(action)
                for (int i = 0; i < timeInterval[cc] / 30; i++)
                {
                    VideoW.Write(frame);
                }

                running = false;
                Console.WriteLine("video frames written " + video_frame);
                frame.Dispose();
                cc++;
            }
        }
        /// <summary>
        /// Use this to convert mat frames into bitmaps so we can display
        /// </summary>
        /// <param name="bitmap"></param>
        /// <returns> BitmapImage </returns>
        BitmapImage BitmapToImageSource(Bitmap bitmap)
        {
            using (MemoryStream memory = new MemoryStream())
            {
                bitmap.Save(memory, System.Drawing.Imaging.ImageFormat.Bmp);
                memory.Position = 0;
                BitmapImage bitmapimage = new BitmapImage();
                bitmapimage.BeginInit();
                bitmapimage.StreamSource = memory;
                bitmapimage.CacheOption = BitmapCacheOption.OnLoad;
                bitmapimage.EndInit();
                return bitmapimage;
            }
        }

    }
}
