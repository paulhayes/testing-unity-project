using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Research.Kinect.Nui;
using System.Net;
using System.Net.Sockets;
using System.Threading;
using System.Timers;

namespace KinectViaTcp
{
    class KinectDataSender
    {
        
        static int PORT = 12345;

        //static int totalFrames = 0;
        //static int lastFrames = 0;
        //static DateTime lastTime = DateTime.MaxValue;
        static System.Timers.Timer heyhey;
        static System.Timers.Timer slooow;

        static Socket socket;
        static Runtime nui;
        //static bool connected = false;
        static int buffersize = 1024;
        static byte[] buffer = new byte[buffersize];

        static List<int> trackedSkeletons = new List<int>();

        static ManualResetEvent sendDone = new ManualResetEvent(false);

        static void Main(string[] args)
        {
            StartNui();
            WaitForConnectionAsync();

            //type enter to end the program
            Console.WriteLine("Type ENTER to end the program.");
            Console.ReadLine();
        }

        // Accept one client connection asynchronously.
        public static void WaitForConnectionAsync()
        {
            // Start to listen for connections from a client.
            Console.WriteLine("Waiting for a connection...");

            socket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);

            //socket.SetSocketOption(SocketOptionLevel.Socket, SocketOptionName.ReuseAddress, true);
            IPEndPoint iep = new IPEndPoint(IPAddress.Any, PORT);
            socket.Bind(iep);
            socket.Listen(5);
            socket.BeginAccept(new AsyncCallback(DoAcceptTcpClientCallback), socket);

            heyhey = new System.Timers.Timer(2000);
            heyhey.Enabled = true;
            heyhey.AutoReset = true;
            heyhey.Elapsed += new ElapsedEventHandler(OnTimedEvent);
            heyhey.Start();
            Console.WriteLine("Enabled timer.");
        }

        // Process the client connection.
        public static void DoAcceptTcpClientCallback(IAsyncResult ar)
        {
            Console.WriteLine("Client connected.");

            // End the operation and display the received data on 
            // the console.
            socket = socket.EndAccept(ar);

            startReceiving();

            // Process the connection here. (Add the client to a
            // server table, read data, etc.)

        }

        public static void startReceiving()
        {
            Console.WriteLine("Running StartReceiving method.");
            int offset = 0;
            socket.BeginReceive(buffer, offset, buffersize, SocketFlags.None, new AsyncCallback(DoReceiveTcpClientCallback), socket);
            Console.WriteLine("Finished running StartReceiving method.");
        }

        public static void DoReceiveTcpClientCallback(IAsyncResult ar)
        {
            Console.WriteLine("Received message.");
            int read = socket.EndReceive(ar);

            startReceiving();
        }

        public static void OnTimedEvent(object source, ElapsedEventArgs e)
        {
            if (socket.Connected)
            {
                string message = "yo yo yo";
                //sendThroughSocket(message);
                //Console.WriteLine("Sent: " + message);
            }
            else
            {
                Console.WriteLine("Didn't send timed message because socket was not connected.");
            }
        }

        static public void StartNui()
        {
            nui = new Runtime();

            try
            {
                nui.Initialize(RuntimeOptions.UseDepthAndPlayerIndex | RuntimeOptions.UseSkeletalTracking | RuntimeOptions.UseColor);
            }
            catch (InvalidOperationException)
            {
                Console.WriteLine("Runtime initialization failed. Please make sure Kinect device is plugged in.");
                return;
            }


            try
            {
                nui.VideoStream.Open(ImageStreamType.Video, 2, ImageResolution.Resolution640x480, ImageType.Color);
                nui.DepthStream.Open(ImageStreamType.Depth, 2, ImageResolution.Resolution320x240, ImageType.DepthAndPlayerIndex);
            }
            catch (InvalidOperationException)
            {
                Console.WriteLine("Failed to open stream. Please make sure to specify a supported image type and resolution.");
                return;
            }

            //lastTime = DateTime.Now;

            nui.DepthFrameReady += new EventHandler<ImageFrameReadyEventArgs>(nui_DepthFrameReady);
            nui.SkeletonFrameReady += new EventHandler<SkeletonFrameReadyEventArgs>(nui_SkeletonFrameReady);
            nui.VideoFrameReady += new EventHandler<ImageFrameReadyEventArgs>(nui_ColorFrameReady);
        }

        static string getLineFromJoint(Joint joint)
        {
            float X = joint.Position.X;
            float Y = joint.Position.Y;
            float Z = joint.Position.Z;

            return "|" + joint.ID + " " + X + " " + Y + " " + Z;
        }

        static void nui_SkeletonFrameReady(object sender, SkeletonFrameReadyEventArgs e)
        {
            SkeletonFrame skeletonFrame = e.SkeletonFrame;
            int iSkeleton = 0;          
            foreach (SkeletonData data in skeletonFrame.Skeletons)
            {
                //Console.WriteLine("User: " + data.UserIndex + " is: " + data.TrackingState);

                if (SkeletonTrackingState.Tracked == data.TrackingState)
                {
                    // Add skeleton to currently tracked skeletons
                    if (!trackedSkeletons.Contains(iSkeleton))
                    {
                        trackedSkeletons.Add(iSkeleton);
                        Send(socket, "#AddUser|" + iSkeleton); 
                    }
                    
                    // data.UserIndex is possibly broken, what does it?? Use iSkeleton instead
                    string jointText = "#User|" + iSkeleton;
                    foreach (Joint joint in data.Joints)
                    {
                        jointText += getLineFromJoint(joint);
                    }
                    Send(socket, jointText);
                        
                }
                else
                {
                    if (trackedSkeletons.Contains(iSkeleton))
                    {
                        trackedSkeletons.Remove(iSkeleton);
                        Send(socket, "#RemoveUser|" + iSkeleton);
                    }
                }
                iSkeleton++;
            } // for each skeleton

        }


        private static void Send(Socket client, String data)
        {
            try
            {
                if (socket.Connected)
                {
                    // Convert the string data to byte data using ASCII encoding.
                    byte[] byteData = Encoding.ASCII.GetBytes(data);

                    // Begin sending the data to the remote device.
                    client.BeginSend(byteData, 0, byteData.Length, SocketFlags.None,
                        new AsyncCallback(SendCallback), client);
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine("Disconnected.  Error: " + ex.ToString());
            }
        }
          
        private static void SendCallback(IAsyncResult ar)
        {
            try
            {
                // Retrieve the socket from the state object.
                Socket client = (Socket)ar.AsyncState;

                // Complete sending the data to the remote device.
                int bytesSent = client.EndSend(ar);
                Console.WriteLine("Sent {0} bytes to server.", bytesSent);

                // Signal that all bytes have been sent.
                sendDone.Set();
            }
            catch (Exception e)
            {
                Console.WriteLine(e.ToString());
            }
        }

        static void nui_DepthFrameReady(object sender, ImageFrameReadyEventArgs e)
        {
            //Nothing, Lebowski, nothing!
        }

        static void nui_ColorFrameReady(object sender, ImageFrameReadyEventArgs e)
        {
            //this method is an abyss
        }
    }

}
