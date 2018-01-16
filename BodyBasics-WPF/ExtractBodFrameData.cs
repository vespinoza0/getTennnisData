///---------------------------------------------------------------------------------------
/// This partial class extracts data specific to the body from the Kinect frames
/// It also turns the joints into 3D data points
///
///
/// Classes referenced: Body(), BodyFrame(), WriteBodyData(), WriteTrackingData()
///---------------------------------------------------------------------------------------

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Kinect;
using System.Windows.Media.Media3D;
using System.Threading;
using System.Diagnostics;

namespace Microsoft.Samples.Kinect.BodyBasics
{
    /// <summary>
    /// Enum to pass to BESSWindown for inaction logic
    /// </summary>
    public enum BodyStatus { Good, JointNotTracked, FarAway, CloseToEdge, FrameRateError, TestComp };

    public partial class ExtractFrameData
    {
        public double ElapsedMilliseconds;

        WriteBodyData Writer;
        Stopwatch caltime = new Stopwatch();
        public int button_num = 2;

        public MultiSourceFrame MSFrame { get; set; }

        public BodyStatus BodyStatus;
        public int fr;
        public static int len = 30 * (121);
        public static long[] Timestamps = new long[len];
        long[] counter; 
        public StoreData StoreData;

        bool JointTS;

        /// <summary>
        /// Array for the bodies
        /// </summary>
        private Body[] bodies = null;

        /// <summary>
        /// Define the single body class to be used in this application
        /// </summary>        
        public Body singlebody = null;

        BodyFrame bodyframe;

        TimeSpan tt;
        double first;

        // floor plane variable
        Vector4 floorplane;

        // Joint used to calculate BESS metrics
        Joint left_wrist;
        Joint left_hip;
        Joint right_wrist;
        Joint right_hip;
        Joint left_ankle;
        Joint right_ankle;
        Joint spineBase;
        Joint spineTop;

        // Additional joints to be saved to a text file for data analysis
        Joint spine_mid;
        Joint shoulder_left;
        Joint shoulder_right;
        Joint elbow_left;
        Joint elbow_right;
        Joint head;
        Joint knee_left;
        Joint knee_right;
        Joint neck;
        Joint thumb_left;
        Joint thumb_right;
        Joint foot_left;
        Joint foot_right;
        Joint hand_left;
        Joint hand_right;
        Joint hand_tip_left;
        Joint hand_tip_right;

        /// <summary>
        /// Presents all of the body data in the ExtractBodFrameData class. It also lets the user decide what to save.
        /// </summary>
        /// <param name="bodyframe">Incoming bodyframe</param>
        //public void Extract()MultiSourceFrame MSFrame)
        public void Extract()
        {
            bodyframe = MSFrame.BodyFrameReference.AcquireFrame();

            ExtractBodyData(bodyframe);

        }

        /// <summary>
        /// test accessing public method to print in console
        /// </summary>
        public void printSTR()
        {
            Console.WriteLine("you have printed from EFD ");
        }

        /// <summary>
        /// extract body data from body frame data
        /// </summary>
        /// <param name="bodyframe"></param>
        void ExtractBodyData(BodyFrame bodyframe)
        {

            if (bodyframe == null) {
                return;
            }

            using (bodyframe)
            {
                // if the array isn't there, make a new one
                if (bodies == null) bodies = new Body[bodyframe.BodyCount];

                // update the array
                bodyframe.GetAndRefreshBodyData(bodies);

                //analyze all bodies and get the closest one
                singlebody = GetClosestBody();

                floorplane = bodyframe.FloorClipPlane;

                tt = bodyframe.RelativeTime;

            }

            // make sure that tere is abody to track
            if (singlebody == null)
            {
                //skipframe = true;
                return;
            }


            // Identify a single body that's been tracked and use its index to track the right joints            
            left_wrist = singlebody.Joints[JointType.WristLeft];
            left_hip = singlebody.Joints[JointType.HipLeft];
            right_wrist = singlebody.Joints[JointType.WristRight];
            right_hip = singlebody.Joints[JointType.HipRight];
            left_ankle = singlebody.Joints[JointType.AnkleLeft];
            right_ankle = singlebody.Joints[JointType.AnkleRight];
            spineBase = singlebody.Joints[JointType.SpineBase];
            spineTop = singlebody.Joints[JointType.SpineShoulder];

            // Additional joints to be saved to a text file for data analysis
            spine_mid = singlebody.Joints[JointType.SpineMid];
            shoulder_left = singlebody.Joints[JointType.ShoulderLeft];
            shoulder_right = singlebody.Joints[JointType.ShoulderRight];
            elbow_left = singlebody.Joints[JointType.ElbowLeft];
            elbow_right = singlebody.Joints[JointType.ElbowRight];
            head = singlebody.Joints[JointType.Head];
            knee_left = singlebody.Joints[JointType.KneeLeft];
            knee_right = singlebody.Joints[JointType.KneeRight];
            neck = singlebody.Joints[JointType.Neck];
            thumb_left = singlebody.Joints[JointType.ThumbLeft];
            thumb_right = singlebody.Joints[JointType.ThumbRight];
            foot_left = singlebody.Joints[JointType.FootLeft];
            foot_right = singlebody.Joints[JointType.FootRight];
            hand_left = singlebody.Joints[JointType.HandLeft];
            hand_right = singlebody.Joints[JointType.HandRight];
            hand_tip_left = singlebody.Joints[JointType.HandTipLeft];
            hand_tip_right = singlebody.Joints[JointType.HandTipRight];

            // setup hand detection data
            //right_hand_status = -2;
            //left_hand_status = -2;


            if (fr == 0)
            {
                first = tt.TotalMilliseconds;
                Timestamps[fr] = 0;
            }
            else
            {
                if ((Timestamps[fr] - Timestamps[fr - 1]) > (1000 / 5)) BodyStatus = BodyStatus.FrameRateError;
            }

            if (!singlebody.IsTracked ||
            (left_wrist.TrackingState == TrackingState.NotTracked ||
           left_hip.TrackingState == TrackingState.NotTracked ||
           right_wrist.TrackingState == TrackingState.NotTracked ||
           right_hip.TrackingState == TrackingState.NotTracked ||
           left_ankle.TrackingState == TrackingState.NotTracked ||
           right_ankle.TrackingState == TrackingState.NotTracked ||
           spineBase.TrackingState == TrackingState.NotTracked ||
           spineTop.TrackingState == TrackingState.NotTracked))
            {
                BodyStatus = BodyStatus.JointNotTracked;
            }
            else if (singlebody.Joints[JointType.SpineBase].Position.Z >= 4)
            {
                BodyStatus = BodyStatus.FarAway;
            }
            else if (singlebody.ClippedEdges != FrameEdges.None)
            {
                BodyStatus = BodyStatus.CloseToEdge;
            }
            else
            {
                BodyStatus = BodyStatus.Good;
            }

            if (BodyStatus != BodyStatus.Good);
                //skipframe = true;

        }


        public void Record(long CalibrationTime)
        {
            // Find the calibration frame number
            if (fr > 0)
            {
                if (CalibrationTime >= Timestamps[fr - 1]) // && cal_done == false)
                {
                    // Index that points to the first frame containing test data point
                    StoreData.ind_cal_stop[StoreData.button_number] = fr;
                    //cal_done = true;
                }
            }

            // This needs to happen on a separate thread.
            Record();
            Thread storing_variables = new Thread(new ThreadStart(Record));
            //Start the new thread
            storing_variables.Start();

        }

        /// <summary>
        /// Store all the joint inforamtion in memory
        /// </summary>
        public void Record()
        {
 
            if (fr == 0)
            {
                //caltime.Start();
            }

            if (floorplane == null)
            {
                StoreData.floor_plane[StoreData.button_number][fr] = StoreData.floor_plane[StoreData.button_number][fr - 1];
            }
            else
            {
                StoreData.floor_plane[StoreData.button_number][fr] = floorplane;
            }


            // Assign all required tracking locations to 3d vectors, enables 3D math if needed
            
            StoreData.joint_locations[StoreData.button_number][0][fr] = jointToVector(left_wrist);
            StoreData.joint_locations[StoreData.button_number][1][fr] = jointToVector(left_hip);
            StoreData.joint_locations[StoreData.button_number][2][fr] = jointToVector(left_ankle);

            StoreData.joint_locations[StoreData.button_number][3][fr] = jointToVector(right_wrist);
            StoreData.joint_locations[StoreData.button_number][4][fr] = jointToVector(right_hip);
            StoreData.joint_locations[StoreData.button_number][5][fr] = jointToVector(right_ankle);

            StoreData.joint_locations[StoreData.button_number][6][fr] = jointToVector(spineBase);
            StoreData.joint_locations[StoreData.button_number][7][fr] = jointToVector(spineTop);

            // Assign additional joints
            StoreData.joint_locations[StoreData.button_number][8][fr] = jointToVector(spine_mid);
            StoreData.joint_locations[StoreData.button_number][9][fr] = jointToVector(shoulder_left);
            StoreData.joint_locations[StoreData.button_number][10][fr] = jointToVector(shoulder_right);
            StoreData.joint_locations[StoreData.button_number][11][fr] = jointToVector(elbow_left);
            StoreData.joint_locations[StoreData.button_number][12][fr] = jointToVector(elbow_right);
            StoreData.joint_locations[StoreData.button_number][13][fr] = jointToVector(head);
            StoreData.joint_locations[StoreData.button_number][14][fr] = jointToVector(knee_left);
            StoreData.joint_locations[StoreData.button_number][15][fr] = jointToVector(knee_right);
            StoreData.joint_locations[StoreData.button_number][16][fr] = jointToVector(neck);
            StoreData.joint_locations[StoreData.button_number][17][fr] = jointToVector(thumb_left);
            StoreData.joint_locations[StoreData.button_number][18][fr] = jointToVector(thumb_right);
            StoreData.joint_locations[StoreData.button_number][19][fr] = jointToVector(foot_left);
            StoreData.joint_locations[StoreData.button_number][20][fr] = jointToVector(foot_right);
            StoreData.joint_locations[StoreData.button_number][21][fr] = jointToVector(hand_left);
            StoreData.joint_locations[StoreData.button_number][22][fr] = jointToVector(hand_right);
            StoreData.joint_locations[StoreData.button_number][23][fr] = jointToVector(hand_tip_left);
            StoreData.joint_locations[StoreData.button_number][24][fr] = jointToVector(hand_tip_right);

            // They have quaternions
            StoreData.joint_orientation[StoreData.button_number][0][fr] = singlebody.JointOrientations[JointType.WristLeft];
            StoreData.joint_orientation[StoreData.button_number][1][fr] = singlebody.JointOrientations[JointType.HipLeft];
            StoreData.joint_orientation[StoreData.button_number][2][fr] = singlebody.JointOrientations[JointType.AnkleLeft];
            StoreData.joint_orientation[StoreData.button_number][3][fr] = singlebody.JointOrientations[JointType.WristRight];
            StoreData.joint_orientation[StoreData.button_number][4][fr] = singlebody.JointOrientations[JointType.HipRight];
            StoreData.joint_orientation[StoreData.button_number][5][fr] = singlebody.JointOrientations[JointType.AnkleRight];
            StoreData.joint_orientation[StoreData.button_number][6][fr] = singlebody.JointOrientations[JointType.SpineBase];
            StoreData.joint_orientation[StoreData.button_number][7][fr] = singlebody.JointOrientations[JointType.SpineShoulder];
            StoreData.joint_orientation[StoreData.button_number][8][fr] = singlebody.JointOrientations[JointType.SpineMid];
            StoreData.joint_orientation[StoreData.button_number][9][fr] = singlebody.JointOrientations[JointType.ShoulderLeft];
            StoreData.joint_orientation[StoreData.button_number][10][fr] = singlebody.JointOrientations[JointType.ShoulderRight];
            StoreData.joint_orientation[StoreData.button_number][11][fr] = singlebody.JointOrientations[JointType.ElbowLeft];
            StoreData.joint_orientation[StoreData.button_number][12][fr] = singlebody.JointOrientations[JointType.ElbowRight];
            StoreData.joint_orientation[StoreData.button_number][13][fr] = singlebody.JointOrientations[JointType.Head];
            StoreData.joint_orientation[StoreData.button_number][14][fr] = singlebody.JointOrientations[JointType.KneeLeft];
            StoreData.joint_orientation[StoreData.button_number][15][fr] = singlebody.JointOrientations[JointType.KneeRight];
            StoreData.joint_orientation[StoreData.button_number][16][fr] = singlebody.JointOrientations[JointType.Neck];
            StoreData.joint_orientation[StoreData.button_number][17][fr] = singlebody.JointOrientations[JointType.ThumbLeft];
            StoreData.joint_orientation[StoreData.button_number][18][fr] = singlebody.JointOrientations[JointType.ThumbRight];
            StoreData.joint_orientation[StoreData.button_number][19][fr] = singlebody.JointOrientations[JointType.FootLeft];
            StoreData.joint_orientation[StoreData.button_number][20][fr] = singlebody.JointOrientations[JointType.FootRight];
            StoreData.joint_orientation[StoreData.button_number][21][fr] = singlebody.JointOrientations[JointType.HandLeft];
            StoreData.joint_orientation[StoreData.button_number][22][fr] = singlebody.JointOrientations[JointType.HandRight];
            StoreData.joint_orientation[StoreData.button_number][23][fr] = singlebody.JointOrientations[JointType.HandTipLeft];
            StoreData.joint_orientation[StoreData.button_number][24][fr] = singlebody.JointOrientations[JointType.HandTipRight];
           

            
            // assign hand status
            //right_hand_status = handStatusSwitch(this.singlebody.HandRightState);
            //left_hand_status = handStatusSwitch(this.singlebody.HandLeftState);
            // Eye data
            //StoreData.eyes_raw[StoreData.button_number][0][fr] = lefteye;
            //StoreData.eyes_raw[StoreData.button_number][1][fr] = righteye;
            // Hand raw
            //StoreData.hands_raw[StoreData.button_number][0][fr] = left_hand_status;
            //StoreData.hands_raw[StoreData.button_number][1][fr] = right_hand_status;
            // Trunk lean
            //StoreData.body_lean[StoreData.button_number][0][fr] = singlebody.Lean.X;
            //StoreData.body_lean[StoreData.button_number][1][fr] = singlebody.Lean.Y;

            ElapsedMilliseconds = (tt.TotalMilliseconds - first);
            Timestamps[fr] = (long)ElapsedMilliseconds;
            counter[fr] = caltime.ElapsedMilliseconds;

            StoreData.write_tt(counter[fr], Timestamps[fr], fr);

            if (JointTS)
            {
                foreach (JointType jointType in singlebody.Joints.Keys)
                {
                    TrackingState trackingState = singlebody.Joints[jointType].TrackingState;

                    if (jointType != JointType.ThumbLeft && jointType != JointType.ThumbRight && jointType != JointType.HandTipLeft &&
                    jointType != JointType.HandTipRight)
                    {
                        //WriteTrackingData.WriteTrackingState(trackingState, len, fr);
                    }
                }
            }

            fr++;  //  <<------
        }
        


         public void Write(string dir)
        {
            // Set the BodyStatus to complete to prevent test errors
            BodyStatus = BodyStatus.TestComp;

            // Save final frame index to storedata
            //StoreData.ind_fr[button_num - 1] = fr;
            StoreData.ind_fr[button_num-1] = fr;

            // This makes sure that the trailing zeros get deleted from the jagged arrays containing the joint positions and orientations
            StoreData.set_actual_length(fr, button_num - 1);

            // At the end of each condition let's save the output csv files
            WriteBodyData WBD = new WriteBodyData();
            WBD.writeJoints(dir, button_num - 1, fr, StoreData);
            //if (JointTS) WriteTrackingData.TrackingStateToCSV(dir, button_num - 1, fr);

            fr = 0;
        } 

        /// <summary>
        /// clean way of turning jointsinto 3d data points
        /// </summary>
        /// <param name="input"></param>
        /// <returns></returns>
        private Vector3D jointToVector(Joint input)
        {
            Vector3D result;

            result = new Vector3D(input.Position.X, input.Position.Y, input.Position.Z);

            return result;
        }

        /// <summary>
        /// compare distances to all the tracked bodiees and return the closest one
        /// </summary>
        /// <returns></returns>
        Body GetClosestBody()
        {
            Body result = null;
            double closestBodyDistance = double.MaxValue;
            // int ind = 0;
            if (bodies != null)
            {
                foreach (var body in bodies)
                {
                    if (body.IsTracked)
                    {
                        var currentLocation = body.Joints[JointType.SpineBase].Position;

                        var currentDistance = new Vector3D(currentLocation.X, currentLocation.Y, currentLocation.Z).Length;

                        if (result == null || currentDistance < closestBodyDistance)
                        {
                            result = body;
                            closestBodyDistance = currentDistance;
                            // closestBodyInd = ind;
                        }
                    }
                    // ind++;
                }
            }
            return result;
        }


        // hand switch statement
        private double handStatusSwitch(HandState hand)
        {
            switch (hand)
            {
                case HandState.Open:
                    return 0;
                case HandState.Closed:
                    return 1;
                case HandState.Lasso:
                    return 2;
                case HandState.Unknown:
                    return 3;
                case HandState.NotTracked:
                    return 4;
                default:
                    return -1;
            }

        }

        public void initialize() {
            int len = 30 * (120 + 1);
            Timestamps = new long[len];
            
        }




    }
  }
