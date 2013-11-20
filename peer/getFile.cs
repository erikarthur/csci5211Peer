using System;
using System.Threading;
using socketSrv;
using System.Net;
using System.Net.Sockets;
using System.IO;

namespace peer
{
	public class getFile
	{
		public void getFileFromNet (object data)
		{
			socketSrv.commandMessage cmd = (socketSrv.commandMessage)data;
			IPEndPoint iep = new IPEndPoint(IPAddress.Any, cmd.port);
			TcpListener server = new TcpListener(iep);
			server.ExclusiveAddressUse = true;
			server.Start();
			TcpClient tcpFIleNetClient = server.AcceptTcpClient();
			
			IPAddress remoteMachine = IPAddress.Parse(tcpFIleNetClient.Client.RemoteEndPoint.ToString());
			
			Console.WriteLine("\n{0} Connected.  Starting file transfer\n", remoteMachine);
			
			byte [] buffer = new byte[1500];
			
			NetworkStream fileNetStream = tcpFIleNetClient.GetStream();
			int numBytes = fileNetStream.Read(buffer, 0, 1500);
			
			byte [] messageSizeBytes = new byte[4];
			byte [] cmdBytes = new byte[4];
			byte [] fileSizeBytes = new byte[4];
			byte [] fileNameSizeBytes = new byte[4];
			byte [] fileNameBytes = new byte[255];
			int messageSize, fileSize, fileNameSize, cmdNum;
			string fileName;
			
			System.Buffer.BlockCopy(buffer, 0, messageSizeBytes, 0, 4);
			System.Buffer.BlockCopy(buffer, 4, cmdBytes, 0, 4);
			System.Buffer.BlockCopy(buffer, 8, fileSizeBytes, 0, 4);	
			System.Buffer.BlockCopy(buffer, 12, fileNameSizeBytes, 0, 4);
			
			messageSize = BitConverter.ToInt32(messageSizeBytes,0);
			fileSize = BitConverter.ToInt32(fileSizeBytes, 0);
			fileNameSize = BitConverter.ToInt32(fileNameBytes,0);
			cmdNum = BitConverter.ToInt32(cmdBytes,0);
			if (cmdNum != cmd.command)
			{
				//lucy we have a problem
				//bugbug
			}
				
			System.Buffer.BlockCopy(buffer, 16, fileNameBytes, 0, fileNameSize);
			
			fileName = fileNameBytes.ToString();
			int bytesLeft = (16 + fileNameSize + fileSize);
			
			if (numBytes == bytesLeft)  //may need messageSize here
			{
				//got it all
				//open fileStream and write the bytes
				FileStream fs = new FileStream(fileName, FileMode.CreateNew);
				for (int i=bytesLeft;i<buffer.Length;i++)
				{
					fs.WriteByte(buffer[i]);	
				}
				fs.Close();
			}
			
			Console.WriteLine("\nFile transfer complete. {0} bytes written\n", numBytes);
			
			//bugbug need to cover the larger than 1500 bytes case.
			fileNetStream.Close();
			tcpFIleNetClient.Close();
		}
	}
}

