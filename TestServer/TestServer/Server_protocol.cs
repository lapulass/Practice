using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Runtime.InteropServices;

namespace TestServer {
    partial class Server {
        delegate void ProcessPacket(byte[] packet);
        Dictionary<int, ProcessPacket> process_handler = new Dictionary<int, ProcessPacket>();


        private void RegisterHandler()
        {
            AddHandler(PLAYER_PUT, ProcessPlayerPut);
            AddHandler(PLAYER_POP, ProcessPlayerPop);
            AddHandler(PLAYER_MOVE, ProcessPlayerMove);
        }

        private void AddHandler(int number, ProcessPacket name)
        {
            process_handler.Add(number, name);
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

        // process packet function
        #region

        void ProcessPlayerPut(byte[] packet)
        {
            sc_packet_player_put player_put = 
            ByteToStruct<sc_packet_player_put>(packet);

  

            //Console.WriteLine(player_put.id + "Player Enter");
        }

        void ProcessPlayerPop(byte[] packet)
        {

        }

        void ProcessPlayerMove(byte[] packet)
        {

            Console.WriteLine("move packet");
        }

        #endregion

        // define protocol
        #region

        // Player Protocol
        const int PLAYER_PUT = 0;
        const int PLAYER_POP = 1;
        const int PLAYER_MOVE = 2;
        //const int PLAYER_CHATTING = 3;

        #endregion

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

    }
}
