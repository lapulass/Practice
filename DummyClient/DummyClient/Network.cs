using System;
using System.Net;
using System.Net.Sockets;
using System.Runtime.InteropServices;
using System.Threading;

namespace DummyClient {

    class AsyncUserToken {
        Player player;
        public int plater_id { get; set; }
        public AsyncUserToken() :this(null) { }
        public AsyncUserToken(Player p) { player = p; }
        public Player SafePlayer
        {
            get { return player; }
            set { player = value; }
        }
        
    }

    class Network {

        const int MaxUser = 100;
        public Socket ConnectSocket;
        SocketAsyncEventArgs RecvArg;
        public ushort id { get; set; }

        private bool isFirst = false;

        public Network()
        {

        }

        public void InitialzeNetWork()
        {
            ConnectSocket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
            IPEndPoint endppoint = new IPEndPoint(IPAddress.Loopback, 11200);

            SocketAsyncEventArgs ConnectArg = new SocketAsyncEventArgs();
            ConnectArg.Completed += new EventHandler<SocketAsyncEventArgs>(ConnectCompleted);
            ConnectArg.RemoteEndPoint = endppoint;

            try {
                bool result =  ConnectSocket.ConnectAsync(ConnectArg);
                if(!result) {
                    Console.WriteLine("?????????????????");
                }
            } catch (InvalidOperationException iop) {
                Console.WriteLine("receive iop" + iop.Message);

            } catch (SocketException se) {
                Console.WriteLine("receive se" + se.Message);
            } catch (NullReferenceException nre) {
                Console.WriteLine(nre.Message);
            } 
            ConnectSocket.SendBufferSize = 0;



            RecvArg = new SocketAsyncEventArgs();
            RecvArg.Completed += new EventHandler<SocketAsyncEventArgs>(RecvComplete);
            byte[] buf = new byte[1024];
            RecvArg.SetBuffer(buf, 0, buf.Length);
        }   

        private void ConnectCompleted(object sender, SocketAsyncEventArgs socketarg)
        {
            Interlocked.Increment(ref Program.x);
            Socket sock = sender as Socket;
            if (sock.Connected == false) sock.ConnectAsync(socketarg);
            else {
                try {
                    sock.ReceiveAsync(RecvArg);
                } catch {

                }
            }
        }

        public void SendPacket(int type)
        {
            byte[] buf = null;
            switch (type) {
                case PLAYER_PUT:
                    break;
                case PLAYER_POP:
                    break;
                case PLAYER_MOVE:
                    cs_pacekt_player_move player_move;
                    player_move.type = PLAYER_MOVE;
                    player_move.size = (byte)Marshal.SizeOf(typeof(cs_pacekt_player_move));
                    player_move.id = (ushort)id;
                    Random rand = new Random();
                    rand.Next(0, 4);
                    player_move.dir = (ushort)rand.Next(0, 4);
                    buf = StructureToByte(player_move);
                    break;
                default: Console.WriteLine("Unknown Packet Type");
                    break;
            }

            SocketAsyncEventArgs SendArg = new SocketAsyncEventArgs();
            SendArg.Completed += new EventHandler<SocketAsyncEventArgs>(SendCompleted);
            SendArg.SetBuffer(buf, 0, buf.Length);
            ConnectSocket.SendAsync(SendArg);
        }

        private void SendCompleted(object sender, SocketAsyncEventArgs socketarg)
        {

        }

        public void ProcessPacket(byte[] packet)
        {
            switch (packet[1]) {
                case PLAYER_PUT: {
                        sc_packet_player_put player_put = ByteToStruct<sc_packet_player_put>(packet);
                        if (!isFirst) {
                            id = player_put.id;
                            Console.WriteLine(id + "put");
                        }
                        if (id == player_put.id) break;
                        else {
                            // other player process
                        }
                    }
                    break;
                default:
                    Console.WriteLine("Unknown Packet Type");
                    break;
            }
        }

        private void RecvComplete(object sender, SocketAsyncEventArgs arg)
        {
            ProcessPacket(arg.Buffer);
            Socket sock = sender as Socket;
            //sock.ReceiveAsync(arg);
        }


        // Player Protocol
        internal const int PLAYER_PUT = 0;
        internal const int PLAYER_POP = 1;
        internal const int PLAYER_MOVE = 2;
        //const int PLAYER_CHATTING = 3;

        // define packet
        struct sc_packet_player_put {
            public byte size;
            public byte type;
            public ushort id;
        }

        struct sc_packet_player_pop {
            public byte size;
            public byte type;
            public short id;
        }

        struct sc_packet_player_move {
            public byte size;
            public byte type;
            public short id;
            public float x;
            public float y;
        }

        struct cs_pacekt_player_move {
            public byte size;
            public byte type;
            public ushort dir;
            public ushort id;
        }

        public static T ByteToStruct<T>(byte[] buffer) where T : struct
        {
            int size = Marshal.SizeOf(typeof(T));

            if (size > buffer.Length) {
                throw new Exception();
            }

            IntPtr ptr = Marshal.AllocHGlobal(size);
            Marshal.Copy(buffer, 0, ptr, size);
            T obj = (T)Marshal.PtrToStructure(ptr, typeof(T));
            Marshal.FreeHGlobal(ptr);
            return obj;
        }

        public static byte[] StructureToByte(object obj)
        {
            int datasize = Marshal.SizeOf(obj);//((PACKET_DATA)obj).TotalBytes; // 구조체에 할당된 메모리의 크기를 구한다.
            IntPtr buff = Marshal.AllocHGlobal(datasize); // 비관리 메모리 영역에 구조체 크기만큼의 메모리를 할당한다.
            Marshal.StructureToPtr(obj, buff, false); // 할당된 구조체 객체의 주소를 구한다.
            byte[] data = new byte[datasize]; // 구조체가 복사될 배열
            Marshal.Copy(buff, data, 0, datasize); // 구조체 객체를 배열에 복사
            Marshal.FreeHGlobal(buff); // 비관리 메모리 영역에 할당했던 메모리를 해제함
            return data; // 배열을 리턴
        }
    }
}
