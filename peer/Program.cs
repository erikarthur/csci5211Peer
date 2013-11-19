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
using peer;

namespace ServerExperiment
{
    class Program
    {
        static void Main(string[] args)
        {
            PeerToPeer p2p = new PeerToPeer();
            p2p.connectCentralServer(args);
            p2p.runP2PNetwork();
        }
    }	

}
