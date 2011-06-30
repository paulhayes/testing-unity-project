using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using KinectViaTcp;
using System.Runtime.Serialization;
using System.Runtime.Serialization.Formatters.Binary;
using System.IO;

namespace KinectDataReceiverToFile
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
         Dictionary<KinectJointID,Brush> jointColors = new Dictionary<KinectJointID,Brush>() { 
            {KinectJointID.HipCenter, new SolidColorBrush(Color.FromRgb(169, 176, 155))},
            {KinectJointID.Spine, new SolidColorBrush(Color.FromRgb(169, 176, 155))},
            {KinectJointID.ShoulderCenter, new SolidColorBrush(Color.FromRgb(168, 230, 29))},
            {KinectJointID.Head, new SolidColorBrush(Color.FromRgb(200, 0,   0))},
            {KinectJointID.ShoulderLeft, new SolidColorBrush(Color.FromRgb(79,  84,  33))},
            {KinectJointID.ElbowLeft, new SolidColorBrush(Color.FromRgb(84,  33,  42))},
            {KinectJointID.WristLeft, new SolidColorBrush(Color.FromRgb(255, 126, 0))},
            {KinectJointID.HandLeft, new SolidColorBrush(Color.FromRgb(215,  86, 0))},
            {KinectJointID.ShoulderRight, new SolidColorBrush(Color.FromRgb(33,  79,  84))},
            {KinectJointID.ElbowRight, new SolidColorBrush(Color.FromRgb(33,  33,  84))},
            {KinectJointID.WristRight, new SolidColorBrush(Color.FromRgb(77,  109, 243))},
            {KinectJointID.HandRight, new SolidColorBrush(Color.FromRgb(37,   69, 243))},
            {KinectJointID.HipLeft, new SolidColorBrush(Color.FromRgb(77,  109, 243))},
            {KinectJointID.KneeLeft, new SolidColorBrush(Color.FromRgb(69,  33,  84))},
            {KinectJointID.AnkleLeft, new SolidColorBrush(Color.FromRgb(229, 170, 122))},
            {KinectJointID.FootLeft, new SolidColorBrush(Color.FromRgb(255, 126, 0))},
            {KinectJointID.HipRight, new SolidColorBrush(Color.FromRgb(181, 165, 213))},
            {KinectJointID.KneeRight, new SolidColorBrush(Color.FromRgb(71, 222,  76))},
            {KinectJointID.AnkleRight, new SolidColorBrush(Color.FromRgb(245, 228, 156))},
            {KinectJointID.FootRight, new SolidColorBrush(Color.FromRgb(77,  109, 243))}
        };



        public MainWindow()
        {
            InitializeComponent();

            // Give Server time to start ups
            System.Threading.Thread.Sleep(1000);

            Console.WriteLine("Starting data receiver.");

            // Create a new receiver
            KinectDataReceiver dataReceiver = new KinectDataReceiver("127.0.0.1");
            dataReceiver.UpdatedSkeletonDataEvent += new KinectViaTcp.EventHandler(NewKinectDataReceived);
        }

        // This will be called whenever the list changes.
        private void NewKinectDataReceived(object sender, SkeletonData skeleton)
        {
            Console.WriteLine("Received... " + skeleton.UserIndex);

            //SkeletonData skeleton = new SkeletonData(originalSkeleton); // Copy the skeleton

             Brush[] brushes = new Brush[6];
            brushes[0] = new SolidColorBrush(Color.FromRgb(255, 0, 0));
            brushes[1] = new SolidColorBrush(Color.FromRgb(0, 255, 0));
            brushes[2] = new SolidColorBrush(Color.FromRgb(64, 255, 255));
            brushes[3] = new SolidColorBrush(Color.FromRgb(255, 255, 64));
            brushes[4] = new SolidColorBrush(Color.FromRgb(255, 64, 255));
            brushes[5] = new SolidColorBrush(Color.FromRgb(128, 128, 255));

            
            skeletonCanvas.Dispatcher.BeginInvoke(
    System.Windows.Threading.DispatcherPriority.Normal,
    new Action(
      delegate()
      {
          skeletonCanvas.Children.Clear();

          Brush brush = brushes[0];
          skeletonCanvas.Children.Add(getBodySegment(skeleton, brush, KinectJointID.HipCenter, KinectJointID.Spine, KinectJointID.ShoulderCenter, KinectJointID.Head));
          skeletonCanvas.Children.Add(getBodySegment(skeleton, brush, KinectJointID.ShoulderCenter, KinectJointID.ShoulderLeft, KinectJointID.ElbowLeft, KinectJointID.WristLeft, KinectJointID.HandLeft));
          skeletonCanvas.Children.Add(getBodySegment(skeleton, brush, KinectJointID.ShoulderCenter, KinectJointID.ShoulderRight, KinectJointID.ElbowRight, KinectJointID.WristRight, KinectJointID.HandRight));
          skeletonCanvas.Children.Add(getBodySegment(skeleton, brush, KinectJointID.HipCenter, KinectJointID.HipLeft, KinectJointID.KneeLeft, KinectJointID.AnkleLeft, KinectJointID.FootLeft));
          skeletonCanvas.Children.Add(getBodySegment(skeleton, brush, KinectJointID.HipCenter, KinectJointID.HipRight, KinectJointID.KneeRight, KinectJointID.AnkleRight, KinectJointID.FootRight));

          // Draw joints
          foreach (KinectJoint joint in skeleton.Joints)
          {
              Point jointPos = getDisplayPosition(joint);
              Line jointLine = new Line();
              jointLine.X1 = jointPos.X - 3;
              jointLine.X2 = jointLine.X1 + 6;
              jointLine.Y1 = jointLine.Y2 = jointPos.Y;
              jointLine.Stroke = jointColors[joint.ID];
              jointLine.StrokeThickness = 6;
              skeletonCanvas.Children.Add(jointLine);
          }

      }
  ));
            
           

          
        }


        Polyline getBodySegment(SkeletonData skeleton, Brush brush, params KinectJointID[] ids)
        {
            PointCollection points = new PointCollection(ids.Length);
            for (int i = 0; i < ids.Length; ++i)
            {
                points.Add(getDisplayPosition(skeleton.GetJoint(ids[i])));
            }

            Polyline polyline = new Polyline();
            polyline.Points = points;
            polyline.Stroke = brush;
            polyline.StrokeThickness = 5;
            return polyline;
        }

        private Point getDisplayPosition(KinectJoint joint)
        {
            float depthX, depthY;
            //nuiDevices[1].SkeletonEngine.SkeletonToDepthImage(joint.Position, out depthX, out depthY);
            depthX = Math.Max(0, Math.Min(joint.Position.X * 320, 320));  //convert to 320, 240 space
            depthY = Math.Max(0, Math.Min(joint.Position.Y * 240, 240));  //convert to 320, 240 space
            int colorX, colorY;
            //ImageViewArea iv = new ImageViewArea();
            // only ImageResolution.Resolution640x480 is supported at this point
            //nuiDevices[1].NuiCamera.GetColorPixelCoordinatesFromDepthPixel(ImageResolution.Resolution640x480, iv, (int)depthX, (int)depthY, (short)0, out colorX, out colorY);

            // map back to skeleton.Width & skeleton.Height
            return new Point((int)depthX, (int)depthY);
        }


         
    }
    
}
