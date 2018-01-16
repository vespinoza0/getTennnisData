///----------------------------------------------------------------------
/// This class writes the tracking data to a CSV file
///
/// Classes referenced: Environment(), StreamWriter(), TextWriter()
///----------------------------------------------------------------------

using Microsoft.Kinect;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AAPS
{
    class WriteTrackingData
    {

        //  Tracking state array
        private static int[][] TrackingStateArr;

        static int ind;

        public static void allocate(int len)
        {
            TrackingStateArr = null;
            TrackingStateArr = CreateJaggedArray<int[][]>(21, len);
        }


        /// <summary>
        /// WARNING: Do not use where the thumb and handtip states will be recorded. Will result in an index exception.
        /// </summary>
        /// <param name="TS"></param>
        /// <param name="len"></param>
        /// <param name="fr"></param>
        public static void WriteTrackingState(TrackingState TS, int len, int fr)
        {

            ind++;

            if (fr == 0)
            {
                allocate(len);
                ind = 0;
            }

            if (ind == 21) ind = 0;

            switch (TS)
            {
                case TrackingState.Tracked:
                    TrackingStateArr[ind][fr] = 1;
                    return;
                case TrackingState.Inferred:
                    TrackingStateArr[ind][fr] = 0;
                    return;
                case TrackingState.NotTracked:
                    TrackingStateArr[ind][fr] = -1;
                    return;
            }

        }

        public static void TrackingStateToCSV(string dir, int test_num, int num_frames)
        {
            string save_file;

            save_file = (dir + '/' + sub_info.cur_ID + '_' + sub_info.Session_type_str + "_Session_" + sub_info.session_num + "_tracking_states" + test_num.ToString() + ".csv");

            TextWriter textW = new StreamWriter(save_file);

            textW.Write("SpineBase,SpineMid,Neck,Head,ShoulderLeft,ElbowLeft,WristLeft,Handleft,ShoulderRight,ElbowRight,WristRight,HandRight," +
                "HipLeft,KneeLeft,AnkleLeft,FootLeft,HipRight,KneeRight,AnkleRight,FootRight,SpineShoulder");

            textW.Write(Environment.NewLine);

            for (int i = 0; i < num_frames; i++)
            {
                for (int j = 0; j < 20; j++)
                {
                    textW.Write(TrackingStateArr[j][i].ToString() + ",");

                }

                textW.Write(TrackingStateArr[20][i].ToString());

                textW.Write(Environment.NewLine);
            }

            textW.Flush();
            textW.Close();

        }


        // tools to build jagged arrays to store results

        static T CreateJaggedArray<T>(params int[] lengths)
        {
            return (T)InitializeJaggedArray(typeof(T).GetElementType(), 0, lengths);
        }

        static object InitializeJaggedArray(Type type, int index, int[] lengths)
        {
            Array array = Array.CreateInstance(type, lengths[index]);
            Type elementType = type.GetElementType();

            if (elementType != null)
            {
                for (int i = 0; i < lengths[index]; i++)
                {
                    array.SetValue(
                        InitializeJaggedArray(elementType, index + 1, lengths), i);
                }
            }

            return array;
        }

    }
}
