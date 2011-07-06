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
        public List<byte> transmissionBuffer = new List<byte>();

        // An event that clients can use to be notified whenever there is new Kinect Data
        public event EventHandler UpdatedSkeletonDataEvent;

        Socket socket;
        int PORT = 12345;
        IPAddress localhostIP;

        static ManualResetEvent connectDone = new ManualResetEvent(false);
        static ManualResetEvent receiveDone = new ManualResetEvent(false);


        #region Constructor

        /// <summary>
        /// Connects to IP Address and raises an event when receiving SkeletonData
        /// </summary>
        /// <param name="ipAsString">IP Address to connect to</param>
        public KinectDataReceiver(string ipAsString)
        {
            localhostIP = IPAddress.Parse(ipAsString);
            socket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
            EndPoint remoteEP = new IPEndPoint(localhostIP, PORT);

            // Connect to remote machine
            Connect(remoteEP, socket);

            // Connected, begin receiving
            Receive(socket);

        }

        #endregion

        #region TCP Connection Methods

        /// <summary>
        /// Connect to a remote End Point
        /// </summary>
        /// <param name="remoteEP"></param>
        /// <param name="client"></param>
        public void Connect(EndPoint remoteEP, Socket client)
        {
            client.BeginConnect(remoteEP,
                new AsyncCallback(ConnectCallback), client);

            connectDone.WaitOne();
        }

        /// <summary>
        /// Callback fired after connection
        /// </summary>
        /// <param name="ar">Result from the connection to socket</param>
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

        /// <summary>
        /// Receive data from a socket
        /// </summary>
        /// <param name="client">Socket to receive from</param>
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



        /// <summary>
        /// Callback upon receipt of data from the socket
        /// </summary>
        /// <param name="ar"></param>
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
                        // SkeletonData byte length should WAS 1917 but getting packed with 
                        // and additional 28 bytes as part of the transmission
                        // Remove header 
                        // transmissionBuffer.RemoveRange(0, 27);
                        //  transmissionBuffer.RemoveAt(1917);  

                        // Process the complete bytes that we have received
                        ProcessBytes(transmissionBuffer.ToArray());
                        transmissionBuffer.Clear();

                        // Signal that all bytes have been received.
                        receiveDone.Set();

                        Receive(client);
                    }
                }
                else
                {
                    // Remove header 
                    //   transmissionBuffer.RemoveRange(0, 27);

                    // Again if here then we have received everything so process bytes
                    ProcessBytes(transmissionBuffer.ToArray());
                    transmissionBuffer.Clear();

                    // Signal that all bytes have been received.
                    receiveDone.Set();

                    // Begin receiving again
                    Receive(client);

                }
            }
            catch (Exception e)
            {
                Console.WriteLine(e.ToString());
            }
        }

        #endregion

        #region Data processing/conversion

        /// <summary>
        /// Processes the received data
        /// </summary>
        /// <param name="receivedBytes"></param>
        void ProcessBytes(byte[] receivedBytes)
        {
            Console.WriteLine("Received {0} bytes from server.", receivedBytes.Length);

            var skeleton = ByteArrayToObject(receivedBytes) as SkeletonData;
            OnUpdatedSkeletonData(skeleton);


            // Adding image to SkeletonData was increasing the byte length to 309144
            // Checking that we have received a full packet...
            //if (receivedBytes.Length == 309144)
            //{
            //    var skeleton = ByteArrayToObject(receivedBytes) as SkeletonData;
            //    //Console.WriteLine("User: {0}, State: {1}, HipCenter X: {2}", skeleton.UserIndex, skeleton.State.ToString(), skeleton.GetJoint(KinectJointID.HipCenter).Position.X);

            //    OnUpdatedSkeletonData(skeleton);
            //}
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

        #endregion

        #region Fire Event

        /// <summary>
        /// Invoke the event when SkeletonData has been processed from Kinect
        /// </summary>
        /// <param name="e">SkeletonData</param>
        protected virtual void OnUpdatedSkeletonData(SkeletonData e)
        {
            if (UpdatedSkeletonDataEvent != null)
                UpdatedSkeletonDataEvent(this, e);
        }

        #endregion
    }
}
