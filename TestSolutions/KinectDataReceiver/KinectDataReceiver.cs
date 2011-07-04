using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Net;
using System.Net.Sockets;
using System.Threading;

namespace KinectViaTcp
{
    // A delegate type for hooking up change notifications.
    public delegate void EventHandler(object sender, SkeletonData e);



    public class KinectDataReceiver
    {
        public List<SkeletonData> trackedSkeletons = new List<SkeletonData>();

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

            socket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
            //socket.SetSocketOption(SocketOptionLevel.Socket, SocketOptionName.ReuseAddress, true);

            //print("Establishing Connection to "+localhostIP.ToString());

            EndPoint remoteEP = new IPEndPoint(localhostIP, PORT);

            // Connect to remote machine
            Connect(remoteEP, socket);

            // Connected, begin receiving
            Receive(socket);
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
                }
                else
                {
                    // All the data has arrived; put it in response.
                    if (state.sb.Length > 1)
                    {
                        //string receivedData = state.sb.ToString();
                        //ProcessReceivedData(receivedData);
                    }
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
        }

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
