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
  

        public MainWindow()
        {
            InitializeComponent();

            // Give Server time to start ups
            System.Threading.Thread.Sleep(2000);

            Console.WriteLine("Starting data receiver.");

            // Create a new receiver
            KinectDataReceiver dataReceiver = new KinectDataReceiver("127.0.0.1");
            dataReceiver.UpdatedSkeletonDataEvent += new KinectViaTcp.EventHandler(NewKinectDataReceived);
        }

        // This will be called whenever the list changes.
        private void NewKinectDataReceived(object sender, SkeletonData skeleton)
        {
            if (skeleton.State == SkeletonState.New)
            {
                Console.WriteLine("New user detected: " + skeleton.UserIndex);
            }
            else if (skeleton.State == SkeletonState.Removed)
            {
                //Console.WriteLine("User removed: " + skeleton.UserIndex);
            }
            else if (skeleton.State == SkeletonState.ImageOnly)
            {
                Console.WriteLine("Received Image");
            }
         }
         
    }
    
}
