using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Research.Kinect.Nui;
using System.Net;
using System.Net.Sockets;
using System.Threading;
using System.Timers;
using System.Runtime.Serialization.Formatters.Binary;
using System.IO;

namespace KinectViaTcp
{
    class KinectDataSender
    {
        // We want to control how depth data gets converted into false-color data
        // for more intuitive visualization, so we keep 32-bit color frame buffer versions of
        // these, to be updated whenever we receive and process a 16-bit frame.
        const int RED_IDX = 2;
        const int GREEN_IDX = 1;
        const int BLUE_IDX = 0;
        static byte[] depthFrame32 = new byte[320 * 240 * 4];




        static int PORT = 12345;

        //static int totalFrames = 0;
        //static int lastFrames = 0;
        //static DateTime lastTime = DateTime.MaxValue;
        static System.Timers.Timer timer;

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
            Console.WriteLine("Hit ENTER to end the program.");
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

            timer = new System.Timers.Timer(2000);
            timer.Enabled = true;
            timer.AutoReset = true;
            timer.Elapsed += new ElapsedEventHandler(OnTimedEvent);
            timer.Start();
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
                string message = "heartbeat";
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
                nui.Initialize(RuntimeOptions.UseDepthAndPlayerIndex | RuntimeOptions.UseSkeletalTracking);
            }
            catch (InvalidOperationException)
            {
                Console.WriteLine("Runtime initialization failed. Please make sure Kinect device is plugged in.");
                return;
            }


            try
            {
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
        }
        /*
        static string getLineFromJoint(Joint joint)
        {
            float X = joint.Position.X;
            float Y = joint.Position.Y;
            float Z = joint.Position.Z;

            return "|" + joint.ID + " " + X + " " + Y + " " + Z;
        }
         */

        static void nui_SkeletonFrameReady(object sender, SkeletonFrameReadyEventArgs e)
        {
            SkeletonFrame skeletonFrame = e.SkeletonFrame;
            int iSkeleton = 0;
            foreach (Microsoft.Research.Kinect.Nui.SkeletonData data in skeletonFrame.Skeletons)
            {
                //Console.WriteLine("User: " + data.UserIndex + " is: " + data.TrackingState);


                if (SkeletonTrackingState.Tracked == data.TrackingState)
                {
                    var skeletonToSend = new KinectViaTcp.SkeletonData(iSkeleton);
                    bool sendPacket = false;

                    // If skeleton is clipped at all then user has walked off screen or still coming on
                    if (data.Quality == SkeletonQuality.ClippedLeft
                        || data.Quality == SkeletonQuality.ClippedRight
                        || data.Quality == SkeletonQuality.ClippedBottom
                        || data.Quality == SkeletonQuality.ClippedTop)
                    {
                        // Bad quality, so remove skeleton if it already exists as user has walked off
                        if (trackedSkeletons.Contains(iSkeleton))
                        {
                            trackedSkeletons.Remove(iSkeleton);
                            skeletonToSend.State = SkeletonState.Removed;
                            sendPacket = true;
                            //Send(socket, "#RemoveUser|" + iSkeleton);
                        }
                        else
                        {
                            // Otherwise do nothing with this skeleton until it becomes FULL
                            sendPacket = false;
                        }
                    }

                    // Else if we have a FULL skeleton then ADD or UPDATE it!
                    else if (!trackedSkeletons.Contains(iSkeleton))
                    {
                        trackedSkeletons.Add(iSkeleton);
                        skeletonToSend.Position = new Vector(data.Position.X, data.Position.Y, data.Position.Z);
                        skeletonToSend.State = SkeletonState.New;
                        sendPacket = true;
                        //Send(socket, "#AddUser|" + iSkeleton);
                    }
                    else
                    {
                        //string jointText = "#User|" + iSkeleton;
                        foreach (Joint joint in data.Joints)
                        {
                            //jointText += getLineFromJoint(joint);
                            KinectJointID jointType = (KinectJointID)Enum.Parse(typeof(KinectJointID), joint.ID.ToString(), true);
                            Vector position = new Vector(joint.Position.X, joint.Position.Y, joint.Position.Z);
                            skeletonToSend.GetJoint(jointType).Position = position;
                        }
                        skeletonToSend.Position = new Vector(data.Position.X, data.Position.Y, data.Position.Z);
                        skeletonToSend.State = SkeletonState.Updated;
                        sendPacket = true;

                        //Send(socket, jointText);
                    }

                    if (sendPacket)
                    {
                        // Serialise skeleton and send it through the socket
                        //TEST SERIALISTATION
                       // byte[] skeletonBytes = ObjectToByteArray(skeletonToSend);
                        //var testSkeleton = ByteArrayToObject(testArray) as SkeletonData;
                     //   Console.WriteLine("Skeleton size: {0}", skeletonBytes.Length);

                        SendBytes(socket, ObjectToByteArray(skeletonToSend));
                        KinectJoint tempJoint = skeletonToSend.GetJoint(KinectJointID.HipCenter);

                        Console.WriteLine("User: {0}, State: {1}, HipCenter X: {2}, Y: {3}, Z: {4}",
                            skeletonToSend.UserIndex, skeletonToSend.State.ToString(),
                            tempJoint.Position.X,
                            tempJoint.Position.Y,
                            tempJoint.Position.Z);
                    }
                }
                // If we were tracking skeleton and its state has gone bad then remove it..
                // May cause removal to occur twice??
                /*
                else if (trackedSkeletons.Contains(iSkeleton))
                {
                    var skeletonToSend = new KinectViaTcp.SkeletonData(iSkeleton);

                    trackedSkeletons.Remove(iSkeleton);
                    skeletonToSend.State = SkeletonState.Removed;

                    //Send(socket, "#RemoveUser|" + iSkeleton);

                    // Serialise skeleton and send it through the socket
                    //TEST SERIALISTATION
                    byte[] skeletonBytes = ObjectToByteArray(skeletonToSend);
                    //var testSkeleton = ByteArrayToObject(testArray) as SkeletonData;
                    Console.WriteLine("Skeleton size: {0}", skeletonBytes.Length);


                    SendBytes(socket, ObjectToByteArray(skeletonBytes));
                }*/

                iSkeleton++;
            } // for each skeleton

        }

        private static void SendBytes(Socket client, byte[] byteData)
        {
            try
            {
                if (socket.Connected)
                {
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
            PlanarImage Image = e.ImageFrame.Image;

            var imageSkeleton = GeneratePlayerOnly(Image.Bits);
            if(imageSkeleton != null)
            {

            //Console.WriteLine("Image bytes. {0}", convertedDepthFrame.Length);
                byte[] skeletonBytes = ObjectToByteArray(imageSkeleton);
                //var testSkeleton = ByteArrayToObject(testArray) as SkeletonData;
               // Console.WriteLine("Skeleton size: {0}", skeletonBytes.Length);
                SendBytes(socket, skeletonBytes);

            }
        }

        // Converts a 16-bit grayscale depth frame which includes player indexes into a 32-bit frame
        // that displays different players in different colors
        private static SkeletonData GeneratePlayerOnly(byte[] depthFrame16)
        {
            byte[] userFrame32 = new byte[320 * 240 * 4];
            bool hasPlayer = false;

            for (int i16 = 0, i32 = 0; i16 < depthFrame16.Length && i32 < depthFrame32.Length; i16 += 2, i32 += 4)
            {
                int player = depthFrame16[i16] & 0x07;
                int realDepth = (depthFrame16[i16 + 1] << 5) | (depthFrame16[i16] >> 3);
                // transform 13-bit depth information into an 8-bit intensity appropriate
                // for display (we disregard information in most significant bit)
                byte intensity = (byte)(255 - (255 * realDepth / 0x0fff));

                //userFrame32[i16] = 255;

                depthFrame32[i32 + RED_IDX] = 255;
                depthFrame32[i32 + GREEN_IDX] = 255;
                depthFrame32[i32 + BLUE_IDX] = 255;

                // choose different display colors based on player
                if (player > 0)
                {
                    hasPlayer = true;
                    depthFrame32[i32 + RED_IDX] = 0;
                    depthFrame32[i32 + GREEN_IDX] = 0;
                    depthFrame32[i32 + BLUE_IDX] = 0;
                }

            }
            if (hasPlayer)
            {
                var imageSkeleton = new SkeletonData(1);
                imageSkeleton.State = SkeletonState.ImageOnly;
                imageSkeleton.UserImage = depthFrame32;
                return imageSkeleton;
            }
            return null;
        }

        // Serialise SkeletonData

        // Convert an object to a byte array
        private static byte[] ObjectToByteArray(Object obj)
        {
            if (obj == null)
                return null;
            BinaryFormatter bf = new BinaryFormatter();
            MemoryStream ms = new MemoryStream();
            bf.Serialize(ms, obj);
            return ms.ToArray();
        }

        // Convert a byte array to an Object
        private static Object ByteArrayToObject(byte[] arrBytes)
        {
            MemoryStream memStream = new MemoryStream();
            BinaryFormatter binForm = new BinaryFormatter();
            memStream.Write(arrBytes, 0, arrBytes.Length);
            memStream.Seek(0, SeekOrigin.Begin);
            Object obj = (Object)binForm.Deserialize(memStream);
            return obj;
        }


    }

}
