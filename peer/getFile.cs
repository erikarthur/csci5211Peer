using System;
using System.Threading;
using socketSrv;
using System.Net;
using System.Net.Sockets;
using System.IO;
using System.Text;

namespace peer
{
	public class fileTransport
	{
		public void getFile (object data)
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
		
		public void sendFile (object data)
		{
			
			
			socketSrv.commandMessage cmd = (socketSrv.commandMessage)data;
			
			IPEndPoint iep = new IPEndPoint(cmd.peerIP, cmd.port);
			TcpClient tcpSendFile = new TcpClient(iep);
			NetworkStream netStream = tcpSendFile.GetStream();
			
			byte [] messageSizeBytes = new byte[4];
			byte [] addressBytes = new byte[4];
			byte [] portBytes = new byte[4];
			byte [] cmdBytes = new byte[4];
			byte [] fileSizeBytes = new byte[4];
			byte [] fileNameSizeBytes = new byte[4];
			byte [] fileNameBytes = new byte[255];
			byte[] buffer = new byte[1500];
			int messageSize, fileSize, fileNameSize, bufCnt;
			string fileName;
			
			FileInfo fi1 = new FileInfo(cmd.fileName);
			if (fi1.Exists)
			{
				fileSize = (int)fi1.Length;
				fileName = fi1.Name;
				fileNameSize = fileName.Length;
			}
			else
				return;
			
			bufCnt = 0;
			
			messageSizeBytes = BitConverter.GetBytes(0);
			System.Buffer.BlockCopy(buffer, 0, messageSizeBytes, 0, 4);
			bufCnt += 4;

			addressBytes = cmd.peerIP.GetAddressBytes();
			System.Buffer.BlockCopy(buffer, bufCnt, addressBytes, 0, 4);
			bufCnt += 4;

			portBytes = BitConverter.GetBytes(cmd.port);
			System.Buffer.BlockCopy(buffer, bufCnt, cmdBytes, 0, 4);
			bufCnt += 4;
			
			cmdBytes = BitConverter.GetBytes(cmd.command);
			System.Buffer.BlockCopy(buffer, bufCnt, cmdBytes, 0, 4);
			bufCnt += 4;
			
			fileSizeBytes = BitConverter.GetBytes(fileSize);
			System.Buffer.BlockCopy(buffer, bufCnt, fileSizeBytes, 0, 4);	
			bufCnt += 4;
				
			UTF8Encoding encoder = new UTF8Encoding();
			
			fileNameSize = encoder.GetByteCount(fileName);
				
			fileNameSizeBytes = BitConverter.GetBytes(fileNameSize);
			System.Buffer.BlockCopy(buffer, bufCnt, fileNameSizeBytes, 0, 4);
			bufCnt += 4;
			
			fileNameBytes = encoder.GetBytes(fileName);
			System.Buffer.BlockCopy(fileNameBytes, 0, buffer, bufCnt, fileNameSize);
			bufCnt += fileNameSize;
				
			using (FileStream fs = File.OpenRead(cmd.fileName))
	        {
	            int readCnt = fs.Read(buffer,bufCnt,buffer.Length-bufCnt);
	            while (readCnt > 0)
	            {
					bufCnt += readCnt;
					messageSizeBytes = BitConverter.GetBytes(bufCnt);
					System.Buffer.BlockCopy(messageSizeBytes, 0, buffer, 0, 4);
	                //ready to send  - have bufCnt bytes in buffer
					netStream.Write(buffer,0,bufCnt);
					bufCnt = 0;
					readCnt = fs.Read(buffer,bufCnt,buffer.Length);
	            }
	        }
			
			netStream.Close();
			tcpSendFile.Close();
			return;	
		}
	}
}

