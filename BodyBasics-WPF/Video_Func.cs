///--------------------------------------------------------------------------------------
/// This class provides functions to record a video clip and to play it back.
///
/// Classes referenced: Console(), DriveInfo(), KStudio(), KStudioClient(),
///                     KStudioPlayback(), MessageBox(), Path(), Process(), Thread()
///--------------------------------------------------------------------------------------

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Kinect;
using Microsoft.Kinect.Tools;
using System.Threading;
using System.Diagnostics;
using System.Windows;
//using System.ServiceProcess;
using System.IO;

namespace Microsoft.Samples.Kinect.BodyBasics
{

    class VideoFunc
    {
        //static public string filePath = "C:" + '\'+ "Users" + '\'+ "vespinoza" + '\' + "Desktop" '\' "XEFvideos";
        static public string filePath = @"C:\Users\vespinoza\Desktop\XEFvideos\";
        //KStudioRecording crap = new KStudioRecording(filePath,23,2);
       

        static public int dur = 120000;  //2 minutes
        // This bool variable is controlled from the main BESS window and it is set to true when the BESS test
        // has been completed. As soon this is set to true, the recording is stopped.
        static public bool done = false;
        // Stopwatch act_rec = new Stopwatch();
   

        static public string c = Path.GetPathRoot(filePath);
        
        const double minimum_size = 2.8E+9;
        string mes = null;

        /// <summary>
        /// Manages the recording errors
        /// </summary>
        static public bool rec_error { get; private set; } = false;

        public void RecordClip()
        {
            //Console.WriteLine("the thing filepath is"+thing.FilePath);
            string xefname = filePath + MainWindow.textbox + ".xef";

            // Make sure to have enough disk space for the recording
            DriveInfo[] allDrives = DriveInfo.GetDrives();
            //const string c = @"G:\";
            try
            {
                foreach (DriveInfo d in allDrives)
                {
                    if (d.IsReady && d.Name == c)
                    {
                        long totfreespace = d.TotalFreeSpace;
                        if (totfreespace < minimum_size)
                        {

                            string size = (minimum_size / 1E9).ToString();
                            mes = "Not enough disk space to record Kinect video, Available memory less than "
                                + size + " GB";
                            throw new System.ArgumentException(mes);
                        }
                    }
                }
            }
            catch (ArgumentException exx)
            {
                Console.WriteLine("{0} Exception caught.", exx);
                done = true;
                //act_rec.Reset();
                rec_error = true;

                if (exx.Message != mes)
                {
                    // Let's restart the Kinect video service
                    // This service once stopped does not seem to restart automatically
                    Process[] kstu = Process.GetProcessesByName("KStudioHostService");
                    ///  kstu[0].Kill(); // Kill the process
                    kstu = Process.GetProcessesByName("KinectService");
                    kstu[0].Kill();
                }

                while (MessageBox.Show(exx.Message, "ERROR", MessageBoxButton.OK,
                       MessageBoxImage.Error) != MessageBoxResult.OK)
                {
                    Thread.Sleep(20);
                }

                // Reset the recording error before exiting
                rec_error = false;
                return;
            }

           // try
          //  {
                using (KStudioClient client = KStudio.CreateClient())
                {
                    client.ConnectToService();

                    KStudioEventStreamSelectorCollection streamCollection = new KStudioEventStreamSelectorCollection();
                    streamCollection.Add(KStudioEventStreamDataTypeIds.Ir);
                    streamCollection.Add(KStudioEventStreamDataTypeIds.Depth);
                    streamCollection.Add(KStudioEventStreamDataTypeIds.Body);
                    streamCollection.Add(KStudioEventStreamDataTypeIds.BodyIndex);
                    streamCollection.Add(KStudioEventStreamDataTypeIds.UncompressedColor);
                    //streamCollection.Add(KStudioEventStreamDataTypeIds.CompressedColor);

                try
                {
                    Console.WriteLine("now the filepath is " + filePath);
                    using (KStudioRecording recording = client.CreateRecording(filePath, streamCollection))
                    {
                        // Introduce a timer to make sure that the recording is never longer than expected
                        //  act_rec.Start();

                        //recording.StartTimed(duration);
                        recording.Start();

                        while (recording.Duration.TotalMilliseconds < dur && done == false)
                        {
                            Thread.Sleep(30);
                            //int si = (int)recording.BufferInUseSizeMegabytes;
                            // Console.WriteLine("Recording Buffer in Megabytes {0} ", si);
                        }

                        recording.Stop();
                        recording.Dispose();                
                    }
                }
                catch (Exception e)
                {
                    Console.WriteLine("{0} Exception caught.", e);
                    done = true;
                    rec_error = true;

                    while (MessageBox.Show("ERROR", e.Message, MessageBoxButton.OK, MessageBoxImage.Error) != MessageBoxResult.OK)
                    {
                        Thread.Sleep(20);
                    }


                    // Reset the recording error before exiting
                    rec_error = false;

                    return;
                }
                finally
                {
                    Thread.Sleep(100);
                    client.DisconnectFromService();
                }
            }
           
            // Make sure to reset the bool done variable once recording is done
        }




        /// <summary>
        /// 
        /// </summary>
        static uint loopCount = 0;

        public void PlaybackClip()
        {
            using (KStudioClient client = KStudio.CreateClient())
            {
                client.ConnectToService();

                using (KStudioPlayback playback = client.CreatePlayback(filePath))
                {
                    playback.LoopCount = loopCount;
                    playback.Start();

                    while (done == false && playback.State == KStudioPlaybackState.Playing)
                    {
                        Thread.Sleep(30);
                    }

                    if (playback.State == KStudioPlaybackState.Error)
                    {
                        throw new InvalidOperationException("Error: Playback failed!");
                    }

                    playback.Stop();
                    // Reset the value of done
                    done = false;
                }

                client.DisconnectFromService();

            }
        }
    }
}

