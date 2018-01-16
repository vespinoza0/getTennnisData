///----------------------------------------------------------------------
/// This class writes the joint data to a CSV file
///
/// Classes referenced: Environment(), StreamWriter(), StoreData(), 
///                     sub_info(), TextWriter()
///----------------------------------------------------------------------

using Microsoft.Kinect;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Media.Media3D;

namespace Microsoft.Samples.Kinect.BodyBasics
{
    public class WriteBodyData
    {

        private StoreData SD;

        /// <summary>
        /// Function to write stored joint data to file
        /// </summary>
        /// <param name="dir"></param> 
        public void writeJoints(string dir, int test_num, int num_frames, StoreData storedata)
        //public void writeJoints(string dir, int num_frames, StoreData storeData)
        {
            string save_file;
            //int num_frames;
            SD = storedata;

            // write a file for each test, six tests iterate to 0->5
            // for (int i = 0; i < test_count; i++)
            //  {
            //save_file = (dir + '/' + sub_info.cur_ID + '_' + sub_info.Session_type_str + "_Session_" + sub_info.session_num + "_raw_data_test_" + test_num + ".csv");
            save_file = @"C:\Users\vespinoza\Desktop\XEFvideos\"+ MainWindow.textbox + ".csv";
            // the data is stored as Vector3D, so it'll need to be split out to save as CSVs
            TextWriter tw = new StreamWriter(save_file);
            // write header information first
            writeJointHeader(tw, SD.ind_cal_stop[test_num]);
            // write actual data second
            writeJointLines(tw, test_num, num_frames);
            //  }
        }

        /// <summary>
        /// Function to handle writing lines of data
        /// </summary>
        /// <param name="tw"></param>
        /// <param name="test_index"></param>
        private void writeJointLines(TextWriter tw, int test_index, int num_frames)
        //private void writeJointLines(TextWriter tw, int num_frames)
        //private void writeJointLines(TextWriter tw)
        {
            Vector3D[][] joint_data = SD.joint_locations[test_index];
            JointOrientation[][] orientation_data = SD.joint_orientation[test_index];
            Vector4[] floor_normal = SD.floor_plane[test_index];

            for (int f = 0; f<num_frames; f++)
            {
                tw.Write(SD.tt[1][test_index][f] + ",");

                // write joint data
                for (int j = 0; j < SD.joint_count; j++)
                {
                    // write x, y, then z. remember they are stored in Vector3D!
                    tw.Write(joint_data[j][f].X + "," + joint_data[j][f].Y + "," + joint_data[j][f].Z + ",");
                }

                // write eye data
                tw.Write(SD.eyes_raw[test_index][0][f] + "," + SD.eyes_raw[test_index][1][f] + ",");
                // write hand data
                tw.Write(SD.hands_raw[test_index][0][f] + "," + SD.hands_raw[test_index][1][f] + ",");
                // write lean data
                tw.Write(SD.body_lean[test_index][0][f] + "," + SD.body_lean[test_index][1][f] + ",");


                // write orientation data
                for (int j = 0; j < SD.joint_count; j++)
                {
                    // write w, x, y, z. remember these are stored as jointorientation type!
                    tw.Write(orientation_data[j][f].Orientation.W + "," + orientation_data[j][f].Orientation.X + "," +
                        orientation_data[j][f].Orientation.Y + "," + orientation_data[j][f].Orientation.Z);
                    if (j < SD.joint_count - 1)
                    {
                        tw.Write(",");
                    }
                }

                // write the plane of the floor out
                tw.Write("," + floor_normal[f].W + "," + floor_normal[f].X + "," + floor_normal[f].Y + "," + floor_normal[f].Z);
                tw.Write("," + SD.tt[0][test_index][f] + ",");

                // new line, next frame!
                tw.Write(Environment.NewLine);
            }
            // These are necessary to guarantee the correct output file completion. If the streamwriter object gets disposed before the file has been completely 
            // wrtiten then the last line in the file might not be complete.
            tw.Flush();
            tw.Close();
        }

        /// <summary>
        /// Write header function for joint file
        /// </summary>
        /// <param name="tw"></param>
        /// <param name="calibration_complete"></param>
        private void writeJointHeader(TextWriter tw, int calibration_complete)
        {
            tw.WriteLine("cal_end: " + calibration_complete);
            tw.WriteLine(Environment.NewLine);
            // should be 25, one for each tracked joint
            string[] joint_labels =
                { "WRI_L_","HIP_L_","ANK_L_",
                  "WRI_R_","HIP_R_","ANK_R_",
                  "SPB_C_","SPT_C_","SPM_C_",
                  "SHD_L_","SHD_R_",
                  "ELB_L_","ELB_R_","HED_C_",
                  "KNE_L_","KNE_R_","NCK_C_",
                  "THM_L_","THM_R_",
                  "TOE_L_","TOE_R_",
                  "HND_L_","HND_R_",
                  "FIN_L_","FIN_R_"
            };
            string[] non_joint_labels =
                { "EYE_L_STATE","EYE_R_STATE","GES_L_STATE","GES_R_STATE","TRUN_X_LEAN","TRUN_Y_LEAN"};
            string[] orientation_labels =
                { "WRI_L_","HIP_L_","ANK_L_",
                  "WRI_R_","HIP_R_","ANK_R_",
                  "SPB_C_","SPT_C_","SPM_C_",
                  "SHD_L_","SHD_R_",
                  "ELB_L_","ELB_R_","HED_C_",
                  "KNE_L_","KNE_R_","NCK_C_",
                  "THM_L_","THM_R_",
                  "TOE_L_","TOE_R_",
                  "HND_L_","HND_R_",
                  "FIN_L_","FIN_R_"};
            string[] floor_labels =
                { "floor" };

            tw.Write("cal_end at sample number: " + calibration_complete);
            tw.Write(Environment.NewLine);
            //write the time column header
            tw.Write("Time (ms),");

            // write the header for each column: joint name and x/y/z tag
            for (int i = 0; i < joint_labels.Length; i++)
            {
                tw.Write(joint_labels[i] + "X_POS," + joint_labels[i] + "Y_POS," + joint_labels[i] + "Z_POS,");
            }
            // write non-joint labels
            for (int i = 0; i < non_joint_labels.Length; i++)
            {
                tw.Write(non_joint_labels[i] + ",");
            }
            // write orientation labels
            for (int i = 0; i < orientation_labels.Length; i++)
            {
                tw.Write(orientation_labels[i] + "W_ORI," + orientation_labels[i] + "X_ORI," + orientation_labels[i] + "Y_ORI," + orientation_labels[i] + "Z_ORI,");
            }
            for (int i = 0; i < floor_labels.Length; i++)
            {
                tw.Write(floor_labels[i] + "W_ORI," + floor_labels[i] + "X_ORI," + floor_labels[i] + "Y_ORI," + floor_labels[i] + "Z_ORI");
            }
            tw.Write(", Time (ms)");

            tw.Write(Environment.NewLine);

        }
    }
}
