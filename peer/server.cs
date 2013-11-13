using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Timers;

namespace socketSrv
{
    class Server
    {
        private TcpListener tcpListener;
        private Thread listenThread;
        public Int32 myPort;
        public IPAddress myIPAddress;
        public Random RNG = new Random();
        public List<networkList> clientList = new List<networkList>();
       
        public Server()
        {
            this.myPort = 4001 + RNG.Next(3000);

            this.tcpListener = new TcpListener(IPAddress.Any, this.myPort);
            string hostname = Dns.GetHostName();

            //bug bug  need to figure out the array of addresses and pass correctly
            this.myIPAddress = Dns.Resolve(hostname).AddressList[0];  //0 for ubunto and most machines


            this.listenThread = new Thread(new ThreadStart(ListenForClients));
            this.listenThread.Start();
            Console.WriteLine(this.myIPAddress + ", " + this.myPort);
            
        }

        private void HandleClientComm(object client)
        {
            TcpClient tcpClient = (TcpClient)client;
            NetworkStream clientStream = tcpClient.GetStream();
            ASCIIEncoding encoder = new ASCIIEncoding();

            byte[] buffer = new byte[4096];
            byte[] messageLenth = new byte[4];
            int bytesRead, numMessageBytes, nextMsgBytesRead;

            numMessageBytes = 0;
            nextMsgBytesRead = 0;
            bool foundClient = false;
            while (true)
            {

                bytesRead = 0;

                
                //blocks until a client sends a message
                bytesRead = clientStream.Read(buffer, 0, 4096);
                Console.WriteLine("just read data from the client below");
                Console.WriteLine(tcpClient.Client.RemoteEndPoint);


                //check to see if client in in clientList
                for (int j = 0; j < clientList.Count();j++ )
                {
                    if ((clientList[j].machineIP == IPAddress.Parse(((IPEndPoint)tcpClient.Client.RemoteEndPoint).Address.ToString()))  &&
                        ( clientList[j].machinePort == Int32.Parse(((IPEndPoint)tcpClient.Client.RemoteEndPoint).Port.ToString())))
                    {
                        //found it
                        foundClient = true;
                    }
                }
                if (!foundClient)
                {
                    clientList.Add(new networkList());
                    clientList[clientList.Count() - 1].machineIP = IPAddress.Parse(((IPEndPoint)tcpClient.Client.RemoteEndPoint).Address.ToString());
                    clientList[clientList.Count() - 1].machinePort = Int32.Parse(((IPEndPoint)tcpClient.Client.RemoteEndPoint).Port.ToString());
                    clientList[clientList.Count() - 1].machineName = Dns.Resolve(clientList[clientList.Count() - 1].machineIP.ToString()).ToString();
					Console.WriteLine("------");
                    Console.WriteLine("Added peer connection");
                    Console.WriteLine(clientList[clientList.Count() - 1].machineIP + ", " + clientList[clientList.Count() - 1].machinePort);
                }

                if (bytesRead > 3)
                {
                    //strip off first 4 bytes and get the message length
                    System.Buffer.BlockCopy(buffer, 0, messageLenth, 0, sizeof(Int32));

                    //if (BitConverter.IsLittleEndian)
                    //    Array.Reverse(messageLength);  //convert from big endian to little endian

                    numMessageBytes = BitConverter.ToInt32(messageLenth, 0);
                }

                while (bytesRead != numMessageBytes)
                {
                    nextMsgBytesRead = clientStream.Read(buffer, bytesRead, 4096 - bytesRead);
                    bytesRead += nextMsgBytesRead;

                    //bugbug - need a watchdog timer for timeouts
                    //bugbug - need to handle the case of more data than expected from the network
                }
               
                int messageID = BitConverter.ToInt32(buffer, 4);

                if (messageID == 1)
                {
                    //request for peer to join network
                    messageID = 1;   //add client
                    int clientMsgStreamLength = (int)(2 * sizeof(Int32));
                    
                    byte[] intBytes = BitConverter.GetBytes(clientMsgStreamLength);

                    byte[] messageBytes = BitConverter.GetBytes(messageID);

                    System.Buffer.BlockCopy(intBytes, 0, buffer, 0, 4);  //prepends length to buffer
                    System.Buffer.BlockCopy(messageBytes, 0, buffer, 4, messageBytes.Length);

                    clientStream.Write(buffer, 0, clientMsgStreamLength);
                    clientStream.Flush();
                }
                messageID = 0;

            }

            //tcpClient.Close();
        }

        void timer_Elapsed(object sender, ElapsedEventArgs e)
        {
            throw new NotImplementedException();
        }

        private void ListenForClients()
        {
            this.tcpListener.Start();

            while (true)
            {
                if (tcpListener.Pending())
                {
                    //blocks until a client has connected to the server
                    TcpClient client = this.tcpListener.AcceptTcpClient();

                    //create a thread to handle communication 
                    //with connected client
                    Thread clientThread = new Thread(new ParameterizedThreadStart(HandleClientComm));
                    clientThread.Start(client);
                }
            }
        }
    }
}
