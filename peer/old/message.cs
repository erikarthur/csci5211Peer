using System;
using System.Net;

namespace socketSrv22
{
	public class cmdMessage22
	{
		private IPAddress _peerIP;
		private string _peerName;
		private Int32 _port;
		private Int32 _command;
		private string _commandString;
		
		public IPAddress peerIP {
			get { return _peerIP; }
			set {_peerIP = value; }
		}
		
		public string peerName {
			get { return _peerName; }
			set { _peerName = value; }
		}
		
		public Int32 port {
			get {return _port; }
			set {_port = value; }
		}
		
		public Int32 command {
			get { return _command; }
			set { _command = value; }
		}
		
		public string commandString {
			get { return commandString; }
			set { commandString = value; }			
		}
		
		
	}
}

