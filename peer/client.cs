using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;
using System.Timers;

namespace socketSrv
{
    class Client
    {
        TcpClient tcpClient = new TcpClient(AddressFamily.InterNetwork);
        peerInstance myServer = new peerInstance();
        NetworkStream clientStream;
        Timer timer = new Timer();

        public void setServer(peerInstance p)
        {
            myServer.peerIP = p.peerIP;
            myServer.peerPort = p.peerPort;
        }
        public void connectToServer()
        {
            IPHostEntry serverIP = Dns.GetHostEntry(myServer.peerIP.ToString());
            tcpClient = new TcpClient(serverIP.HostName, myServer.peerPort);

            clientStream = tcpClient.GetStream();

            //Int32 messageID = 1;   //add client
            //int clientMsgStreamLength = (int)(4 * sizeof(Int32));
            //byte[] buffer = new byte[4096];

            //byte[] intBytes = BitConverter.GetBytes(clientMsgStreamLength);
            //byte[] addressBytes = ServerExperiment.Program.myAddress.GetAddressBytes();
            //byte[] portBytes = BitConverter.GetBytes(ServerExperiment.Program.myPort);
            //byte[] messageBytes = BitConverter.GetBytes(messageID);

            //System.Buffer.BlockCopy(intBytes, 0, buffer, 0, intBytes.Length);  //prepends length to buffer
            //System.Buffer.BlockCopy(addressBytes, 0, buffer, 4, addressBytes.Length);
            //System.Buffer.BlockCopy(portBytes, 0, buffer, 8, portBytes.Length);
            //System.Buffer.BlockCopy(messageBytes, 0, buffer, 12, messageBytes.Length);

            //clientStream.Write(buffer, 0, clientMsgStreamLength);
            //clientStream.Flush();

            ////wait for ACK from server
            //int bytesRead = clientStream.Read(buffer, 0, 4096);

            //byte[] message = new byte[4092];
            //byte[] messageLength = new byte[4];
            //int numMessageBytes = 0;
            //int nextMsgBytesRead = 0;

            //if (bytesRead > 3)
            //{
            //    //strip off first 4 bytes and get the message length
            //    System.Buffer.BlockCopy(buffer, 0, messageLength, 0, sizeof(Int32));
            //    numMessageBytes = BitConverter.ToInt32(messageLength, 0);
            //}

            //while (bytesRead < numMessageBytes)
            //{
            //    nextMsgBytesRead = clientStream.Read(buffer, bytesRead, 4096 - bytesRead);
            //    bytesRead += nextMsgBytesRead;

            //    //bugbug - need a watchdog timer for timeouts
            //    //bugbug - need to handle the case of more data than expected from the network
            //}


            //byte[] ackMessage = new byte[4];
            //int ackNum = -99;
            //System.Buffer.BlockCopy(buffer, 4, ackMessage, 0, 4);
            //ackNum = BitConverter.ToInt32(ackMessage, 0);
        }

        public void SendCmd (socketSrv.commandMessage cmd)
        {
            switch( cmd.command)
            {
                case 2:    //get file
                    Console.WriteLine("got a get");
                    break;

                case 3:     //put file
                    Console.WriteLine("got a put");
                    break;

            }

        }

        void timer_Elapsed(object sender, ElapsedEventArgs e)
        {
            Console.WriteLine("Sent data");
            Int32 messageID = 0;
            int clientMsgStreamLength = (int)(2 * sizeof(Int32));
            byte[] buffer = new byte[4096];

            byte[] intBytes = BitConverter.GetBytes(clientMsgStreamLength);

            byte[] messageBytes = BitConverter.GetBytes(messageID);

            System.Buffer.BlockCopy(intBytes, 0, buffer, 0, 4);  //prepends length to buffer
            System.Buffer.BlockCopy(messageBytes, 0, buffer, 4, messageBytes.Length);

            clientStream.Write(buffer, 0, clientMsgStreamLength);
            clientStream.Flush();
        }
    }

}
