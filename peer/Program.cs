using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using socketSrv;
using System.Diagnostics;
using System.Net.Sockets;
using System.Net;
using System.IO;
using System.Messaging;

namespace ServerExperiment
{
    class Program
    {
        Random RNG = new Random();
        Int32 serverPort, clientPort;
        static TcpClient centralServer = new TcpClient();
		static List<FileInfo> myFiles;
		static string fileDir;
		static string consoleCmd;
		static MessageQueue clientMessageQueue = new MessageQueue();
		static MessageQueue serverMessageQueue = new MessageQueue();
		
		
        static void Main(string[] args)
        {
			
			//create the list of local files
			
            //create the server instance for this client
            Server s = new Server();
            
			//connect to central server and get P2P server info
            peerInstance clientInstance = new peerInstance();            
			clientInstance = connectToCentralServer(args[0], s);
			
            //Debug.WriteLine(s.myIPAddress + ", " + s.myPort);
            //Debug.WriteLine(clientInstance.peerIP + ", " + clientInstance.peerPort);
			fileDir = args[1];
			refreshFileList(fileDir);
			
            //make connection
            if (s.myIPAddress.Address != clientInstance.peerIP.Address)
            {
                Console.WriteLine("My server address is: " + s.myIPAddress + ":" + s.myPort);
                Console.WriteLine("My PEER server address is: " + clientInstance.peerIP + ":" + clientInstance.peerPort);

				//create the client instance
                Client c = new Client();
                c.setServer(clientInstance);
                c.connectToServer();
            }
            else
            {
				//first machine don't need a client instance
                Console.WriteLine("First machine.");
                Console.WriteLine("My address is: " + s.myIPAddress + ":" + s.myPort);
            }
			
			while (true)
			{
				//check for input
				checkForInput();
				
			}


        }
		
		public static void checkForInput()
		{
			ConsoleKeyInfo cki = new ConsoleKeyInfo();
			char ch;
			
			if (Console.KeyAvailable) 
			{
				cki = Console.ReadKey(false);

				if (cki.Key == ConsoleKey.Backspace) 
				{
					Console.Write (" ");
					Console.Write("\b");
					consoleCmd = consoleCmd.Substring (0, consoleCmd.Length - 1);

				} 
				else 
				{
					ch = cki.KeyChar;
					
					switch (ch) 
					{
					case '\n':
						//parse the command and execute the command
						executeConsoleCommand(consoleCmd);
						consoleCmd = "";
						break;

					default:
						consoleCmd += ch.ToString ();
						break;
					}
				}

			}
		}
		public static void executeConsoleCommand(string cmd)
		{
			char[] charSeparators = new char[] {' '};
			
			cmd = cmd.TrimEnd();
			cmd = cmd.TrimStart();
			
			string [] cmdParts = cmd.Split(charSeparators, 2, StringSplitOptions.RemoveEmptyEntries); //split on spaces
			
			printMsg(cmdParts[0].ToUpper().Trim());
			switch (cmdParts[0].ToUpper().Trim())
			{
			case "QUIT":
				//tell clients, server, and central server that this client is leaving.
				Environment.Exit(0);
				break;
				
			case "GET":
				Console.WriteLine("got a get for file: " + cmdParts[1] + "\n");
				break;
				
			case "PUT":
				Console.WriteLine("got a put for file: " + cmdParts[1] + "\n");
				break;
				
			case "LIST":
				printFiles();
				break;
				
			case "REFRESH":
				refreshFileList(fileDir);
				break;
			
			default:
				Console.Write("\nUnrecognized command.  Check syntax and re-enter\n");
				break;
			}
		}
		
		public static void printMsg(string msg)
		{
			Console.WriteLine("Executing command: " + msg);
		}
		
		
		public static void refreshFileList(string fDir)
		{
			myFiles = new List<FileInfo> ();
			
			try
			{
				var txtFiles = Directory.EnumerateFiles(fileDir);
	
				foreach (string currentFile in txtFiles)
				{
					myFiles.Add(new FileInfo(currentFile));
				}
			}
			catch (Exception e)
			{
				Console.WriteLine(e.Message);
			}
			printFiles();
			
		}
		
		public static void printFiles()
		{
			Console.WriteLine("\nLocal Files are:");
			
			int fc = myFiles.Count;
			for (int i=0; i<fc; i++) 
			{
				Console.WriteLine (myFiles [i].Name);
			}
			Console.Write("\n");
		}
		
        public static peerInstance connectToCentralServer(string centralServerName, Server s1)
        {
            try
            {
                Program.centralServer = new TcpClient(centralServerName, 4000);
            }
            catch (SocketException SE)
            {
                Console.WriteLine(SE.ErrorCode.ToString());
                Console.WriteLine(SE.Message);
                Environment.Exit(0);
            }

            NetworkStream clientStream = centralServer.GetStream();

            peerInstance peer = new peerInstance();
            peer.peerIP = s1.myIPAddress;
            peer.peerPort = s1.myPort;

            byte[] addressBytes = peer.peerIP.GetAddressBytes();
            byte[] portBytes = BitConverter.GetBytes(peer.peerPort);

            int clientMsgStreamLength = (int)(addressBytes.Length + portBytes.Length + sizeof(Int32));


            //copy to byte array
            byte[] buffer = new byte[4096];  //add 4 bytes for the message length at the front

            byte[] intBytes = BitConverter.GetBytes(clientMsgStreamLength);

            System.Buffer.BlockCopy(intBytes, 0, buffer, 0, 4);  //prepends length to buffer
            System.Buffer.BlockCopy(addressBytes, 0, buffer, 4, addressBytes.Length);
            System.Buffer.BlockCopy(portBytes, 0, buffer, 4 + addressBytes.Length, portBytes.Length);

            clientStream.Write(buffer, 0, clientMsgStreamLength);
            clientStream.Flush();

            int bytesRead, nextMsgBytesRead;
            bytesRead = -99;
            bytesRead = clientStream.Read(buffer, 0, 4096);

            byte[] message = new byte[4092];
            byte[] messageLength = new byte[4];
            int messageBytes = 0;

            if (bytesRead > 3)
            {
                //strip off first 4 bytes and get the message length
                System.Buffer.BlockCopy(buffer, 0, messageLength, 0, sizeof(Int32));

                //if (BitConverter.IsLittleEndian)
                //    Array.Reverse(messageLength);  //convert from big endian to little endian

                messageBytes = BitConverter.ToInt32(messageLength, 0);
            }

            while (bytesRead != messageBytes)
            {
                nextMsgBytesRead = clientStream.Read(buffer, bytesRead, 4096 - bytesRead);
                bytesRead += nextMsgBytesRead;

                //bugbug - need a watchdog timer for timeouts
                //bugbug - need to handle the case of more data than expected from the network
            }
            byte[] inBuffer = new byte[messageBytes];
            System.Buffer.BlockCopy(buffer, 4, inBuffer, 0, messageBytes - 4);

            addressBytes = new byte[4];
            portBytes = new byte[sizeof(Int32)];
            System.Buffer.BlockCopy(buffer, 4, addressBytes, 0, 4);
            System.Buffer.BlockCopy(buffer, 8, portBytes, 0, 4);

            IPAddress messageIP = new IPAddress(addressBytes);
            Int32 port = BitConverter.ToInt32(portBytes, 0);

            peerInstance myServer = new peerInstance();
            myServer.peerIP = messageIP;
            myServer.peerPort = port;

            return myServer;
        }
    }
}
