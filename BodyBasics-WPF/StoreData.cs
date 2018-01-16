using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Microsoft.Kinect;
using System.Windows.Media.Media3D;
using System;
using System.Drawing;

public class StoreData {
   
        // function to process test number
      
        #region Create Jagged Array functions

        // tools to build jagged arrays to store results
        public static T CreateJaggedArray<T>(params int[] lengths)
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

        #endregion

        // error results
        public int[] ind_cal_stop = null;
        public int[] ind_fr = null;
        public double[][] eyes_ar = null;
        public double[][][] eyes_raw = null;
        public double[][][] hands_raw = null;
        /// <summary>
        /// tt[timestamp or timer][test number][frame]
        /// </summary>
        public int[][][] tt = null;
        public int button_number = 0;
        public double[][][] body_lean = null;



        // hold eye data
        public double[][] eyes_filtered = null;

        //public UnityEngine.Vector4[][] floor_plane = null;
         //also try 
         public Vector4[][] floor_plane = null;

    //  joint results: test# | joint# | frame#
    // public Vector3D[][][] joint_locations = null;
    public Vector3D[][][] joint_locations = null;

    // orientation results: test# | joint# | frame#
    public JointOrientation[][][] joint_orientation = null;

        // holder test#, joint#, frame# for use in writing the results to file
    public int test_count, joint_count, frame_count, actual_length;

        internal void write_ind_cal_stop(int index, int test)
        {
            ind_cal_stop[test] = index;
        }

        // initialize everything to a correct size
        public void allocate_all(int len, int test, int joints)
        {
            // we know the errors we are tracking
            int errors = 6;
            // they are jagged arrays, use a helperfunction from Results0

            eyes_ar = CreateJaggedArray<double[][]>(test, len);

            eyes_raw = CreateJaggedArray<double[][][]>(test, 2, len);
            hands_raw = CreateJaggedArray<double[][][]>(test, 2, len);
            body_lean = CreateJaggedArray<double[][][]>(test, 2, len);
            floor_plane = CreateJaggedArray<Vector4[][]>(test, len);
       

            // time between frames! use to generate time stamps for saving data
            // elapsed time in milliseconds, it seems
            tt = CreateJaggedArray<int[][][]>(2, test, len);

            // when the calibration stopped, when the test started. save this as well
            ind_cal_stop = new int[test];
            ind_fr = new int[test];

            // do the same for the joint location array
            joint_locations = CreateJaggedArray<Vector3D[][][]>(test, joints, len);
            eyes_filtered = CreateJaggedArray<Double[][]>(test, len);

            // build orientation array
            joint_orientation = CreateJaggedArray<JointOrientation[][][]>(test, joints, len);

            test_count = test;
            frame_count = len;
            joint_count = joints;
        }

        // empty constructor because it needs to be defined before being instantiated
        public StoreData()
        {
            // empty
        }


        // Eyes
        internal void write_eyes(double val, int i)
        {
            eyes_ar[button_number][i] = val;
            // testing
            // Console.WriteLine("{0}", eyes_ar[button_number][i]);
        }

        // Time data
        internal void write_tt(long val, long kinFr, int i)
        {
            tt[0][button_number][i] = (int)val;
            tt[1][button_number][i] = (int)kinFr;
        }
        // raw data from kinect library on eyes. answers question 'is eye closed?'
        // 0 means no, 1 means yes, 2 is maybe, 3 is unknown, -1 default value
        internal void write_eye_raw(double val_left, double val_right, int i)
        {
            eyes_raw[button_number][0][i] = val_left;
            eyes_raw[button_number][1][i] = val_right;
        }
        // raw data from kinect library on hands
        internal void write_hand_raw(double val_left, double val_right, int i)
        {
            hands_raw[button_number][0][i] = val_left;
            hands_raw[button_number][1][i] = val_right;
        }
        // store body lean data, -1 to 1 represents -45 deg to 45deg
        internal void write_body_lean(Microsoft.Kinect.PointF lean, int i)
        {
            body_lean[button_number][0][i] = lean.X;
            body_lean[button_number][1][i] = lean.Y;
        }

        /// <summary>
        /// Set the actual jagged array length after a test is completed
        /// </summary>
        /// <param name="num_frames"></param>
        internal void set_actual_length(int num_frames, int test_num)
        {

            for (int i = 0; i < joint_locations[test_num].Length; i++)
            {
                joint_locations[test_num][i] = joint_locations[test_num][i].Take(num_frames).ToArray();
                joint_orientation[test_num][i] = joint_orientation[test_num][i].Take(num_frames).ToArray();
                floor_plane[test_num] = floor_plane[test_num].Take(num_frames).ToArray();
            }

            tt[0][test_num] = tt[0][test_num].Take(num_frames).ToArray();
            tt[1][test_num] = tt[1][test_num].Take(num_frames).ToArray();
            /// We actually need these to be larger since the interpolation is resizing everything!
  
        }
    

	

}
