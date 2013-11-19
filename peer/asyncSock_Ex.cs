using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Sockets;
using System.Text;

namespace peer
{
    class asyncSock_Ex : SocketAsyncEventArgs
    {
        public int fileSize;
        public int bytesReceived;
        public string filename;
    }
}
