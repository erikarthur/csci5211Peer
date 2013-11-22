using System;
using System.Net;

namespace socketSrv
{
   
    [Serializable()]
    public class commandMessage
	{
		private IPAddress _peerIP;
		private string _peerHostname;
		private Int32 _port;
        private Int32 _command;
        private string _fileName;
        private string _fileDir;
        private IPAddress _putIP;


        public string fileName
        {
            get { return _fileName; }
            set { _fileName = value; }
        }

        public string fileDir
        {
            get { return _fileDir; }
            set { _fileDir = value; }
        }

        public IPAddress putIP
        {
            get { return _putIP; }
            set { _putIP = value; }
        }

		public IPAddress peerIP {
			get { return _peerIP; }
			set {_peerIP = value; }
		}

        public string peerHostname
        {
            get { return _peerHostname; }
            set { _peerHostname = value; }
		}
		
		public Int32 port {
			get {return _port; }
			set {_port = value; }
		}

        public Int32 command
        {
            get { return _command; }
            set { _command = value; }
        }
	}
}

