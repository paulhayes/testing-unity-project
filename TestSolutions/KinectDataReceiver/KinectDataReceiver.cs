using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Net;
using System.Net.Sockets;
using System.Threading;
using System.IO;
using System.Runtime.Serialization.Formatters.Binary;

namespace KinectViaTcp
{
    // A delegate type for hooking up change notifications.
    public delegate void EventHandler(object sender, SkeletonData e);

    

    public class KinectDataReceiver
    {
        public List<SkeletonData> trackedSkeletons = new List<SkeletonData>();
        public List<byte> transmissionBuffer = new List<byte>();

        // An event that clients can use to be notified whenever there is new Kinect Data
        public event EventHandler UpdatedSkeletonDataEvent;
        //public event EventHandler NewUserEvent;
        //public event EventHandler UserLostEvent;

        Socket socket;
        static int buffersize = 1024;
        static byte[] buffer = new byte[buffersize];
        int PORT = 12345;
        IPAddress localhostIP;

        static ManualResetEvent connectDone = new ManualResetEvent(false);
        static ManualResetEvent receiveDone = new ManualResetEvent(false);

        public class StateObject
        {
            // Client socket.
            public Socket workSocket = null;
            // Size of receive buffer.
            public const int BufferSize = 1024;
            // Receive buffer.
            public byte[] buffer = new byte[BufferSize];
            // Received data string.
            public StringBuilder sb = new StringBuilder();
        }


        // Use this for initialization
        public KinectDataReceiver(string ipAsString)
        {
            localhostIP = IPAddress.Parse(ipAsString);
            //while (true)
            //{
                socket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
                //socket.SetSocketOption(SocketOptionLevel.Socket, SocketOptionName.ReuseAddress, true);

                //print("Establishing Connection to "+localhostIP.ToString());

                EndPoint remoteEP = new IPEndPoint(localhostIP, PORT);

                // Connect to remote machine
                Connect(remoteEP, socket);

                // Connected, begin receiving
                Receive(socket);
            //}
        }

        public void Connect(EndPoint remoteEP, Socket client)
        {
            client.BeginConnect(remoteEP,
                new AsyncCallback(ConnectCallback), client);

            connectDone.WaitOne();
        }

        private static void ConnectCallback(IAsyncResult ar)
        {
            try
            {
                // Retrieve the socket from the state object.
                Socket client = (Socket)ar.AsyncState;

                // Complete the connection.
                client.EndConnect(ar);

                Console.WriteLine("Socket connected to {0}",
                    client.RemoteEndPoint.ToString());

                // Signal that the connection has been made.
                connectDone.Set();
            }
            catch (Exception e)
            {
                Console.WriteLine(e.ToString());
            }
        }

        private void Receive(Socket client)
        {
            try
            {
                // Create the state object.
                StateObject state = new StateObject();
                state.workSocket = client;

                // Begin receiving the data from the remote device.
                client.BeginReceive(state.buffer, 0, StateObject.BufferSize, 0,
                    new AsyncCallback(ReceiveCallback), state);
            }
            catch (Exception e)
            {
                Console.WriteLine(e.ToString());
            }
        }




        private void ReceiveCallback(IAsyncResult ar)
        {
            try
            {
                // Retrieve the state object and the client socket 
                // from the asynchronous state object.
                StateObject state = (StateObject)ar.AsyncState;
                Socket client = state.workSocket;
                // Read data from the remote device.
                int bytesRead = client.EndReceive(ar);
                if (bytesRead > 0)
                {
                    for (int i = 0; i < bytesRead; i++)
                    {
                        transmissionBuffer.Add(state.buffer[i]);
                    }
 
                    //we need to read again if this is true
                    if (bytesRead == state.buffer.Length)
                    {
                        client.BeginReceive(state.buffer, 0, StateObject.BufferSize, 0, new AsyncCallback(ReceiveCallback), state);
                    }
                    else
                    {
                        // Remove header 
                       // transmissionBuffer.RemoveRange(0, 27);
                      //  transmissionBuffer.RemoveAt(1917);

                        ProcessBytes(transmissionBuffer.ToArray());
                        transmissionBuffer.Clear();

                        // Signal that all bytes have been received.
                        receiveDone.Set();

                        Receive(client);
                    }


                    /*
                    // There might be more data, so store the data received so far.
                    state.sb.Append(Encoding.ASCII.GetString(state.buffer, 0, bytesRead));

                    string receivedData = state.sb.ToString();
                    string[] statements = receivedData.Split(new char[] { '#' }, StringSplitOptions.RemoveEmptyEntries);

                    for (int i = 0; i < statements.Length - 1; i++)
                    {
                        ProcessStatement(statements[i]);
                    }
                    var lastIndex = statements.Length - 1;
                    state.sb.Length = 0;
                    state.sb.Append("#" + statements[lastIndex]);




                    //  Get the rest of the data.
                    client.BeginReceive(state.buffer, 0, StateObject.BufferSize, 0,
                        new AsyncCallback(ReceiveCallback), state);
                     * 
                     * */

                }
                else
                {
                    // All the data has arrived; put it in response.
                   /* if (state.sb.Length > 1)
                    {
                        //string receivedData = state.sb.ToString();
                        //ProcessReceivedData(receivedData);
                    }
                    */

                    // Remove header 
                 //   transmissionBuffer.RemoveRange(0, 27);
                    
                    // Process!!!
                    ProcessBytes(transmissionBuffer.ToArray());
                    transmissionBuffer.Clear();

                    // Signal that all bytes have been received.
                    receiveDone.Set();

                    // Begin receiving again??...
                    Receive(client);

                }
            }
            catch (Exception e)
            {
                Console.WriteLine(e.ToString());
            }
        }

        void ProcessBytes(byte[] receivedBytes)
        {
            Console.WriteLine("Received {0} bytes from server.", receivedBytes.Length);
            if (receivedBytes.Length == 309144)
            {
                var skeleton = ByteArrayToObject(receivedBytes) as SkeletonData;
                //          Console.WriteLine("User: {0}, State: {1}, HipCenter X: {2}", skeleton.UserIndex, skeleton.State.ToString(), skeleton.GetJoint(KinectJointID.HipCenter).Position.X);

                OnUpdatedSkeletonData(skeleton);
            }
        }

        private static Object ByteArrayToObject(byte[] arrBytes)
        {
            MemoryStream memStream = new MemoryStream();
            BinaryFormatter binForm = new BinaryFormatter();
            memStream.Write(arrBytes, 0, arrBytes.Length);
            memStream.Seek(0, SeekOrigin.Begin);
            Object obj = (Object)binForm.Deserialize(memStream);
            return obj;
        }

        /*
        void ProcessStatement(string message)
        {
            // Add new user
            if (message.StartsWith("Add"))
            {
                string[] parts = message.Split('|');
                var skeleton = new SkeletonData(int.Parse(parts[1]));
                skeleton.State = SkeletonState.New;
                trackedSkeletons.Add(skeleton);
                OnUpdatedSkeletonData(skeleton);
            }
            // Remove user
            else if (message.StartsWith("Remove"))
            {
                string[] parts = message.Split('|');
                var skeleton = new SkeletonData(int.Parse(parts[1]));
                skeleton.State = SkeletonState.Removed;
                trackedSkeletons.Remove(skeleton);
                OnUpdatedSkeletonData(skeleton);
            }
            else
            {
                ProcessUserSkeleton(message);
            }

        }


        void ProcessUserSkeleton(string message)
        {
            // Splits into joint data
            string[] joints = message.Split('|');
            int userId;
            int.TryParse(joints[1], out userId);
            SkeletonData skeleton = GetExistingSkeleton(userId);

            if (skeleton == null)
            {
                return;
            }
            skeleton.State = SkeletonState.Updated;
            for (int i = 2; i < joints.Length; i++)
            {
                string[] subStrings = joints[i].Split(' ');
                if (subStrings.Length == 4)
                {
                    KinectJointID jointType = (KinectJointID)Enum.Parse(typeof(KinectJointID), subStrings[0], true);
                    skeleton.GetJoint(jointType).Position = new Vector(float.Parse(subStrings[1]), float.Parse(subStrings[2]), float.Parse(subStrings[3]));

                }

            }
            OnUpdatedSkeletonData(skeleton);
        }

        private SkeletonData GetExistingSkeleton(int userId)
        {
            foreach (SkeletonData skeleton in trackedSkeletons)
            {
                if (skeleton.UserIndex == userId)
                {
                    return skeleton;
                }
            }
            return null;
        }*/

        // Invoke the NewDataEvent; called whenever list changes
        protected virtual void OnUpdatedSkeletonData(SkeletonData e)
        {
            if (UpdatedSkeletonDataEvent != null)
                UpdatedSkeletonDataEvent(this, e);
        }

        // Invoke the NewDataEvent; called whenever list changes
        /*    protected virtual void OnNewUser(SkeletonData e)
            {
                if (NewUserEvent != null)
                    NewUserEvent(this, e);
            }

            // Invoke the NewDataEvent; called whenever list changes
            protected virtual void OnUserLost(SkeletonData e)
            {
                if (UserLostEvent != null)
                    UserLostEvent(this, e);
            }
         */
    }
}
