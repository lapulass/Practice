using System;
using System.Collections.Generic;
using System.Threading;
using System.Net.Sockets;
using System.Runtime.InteropServices;

namespace TestServer {
    class CASLock {
        int runned = 0;
        bool Is_Runnig()
        {
            return 1 == Interlocked.CompareExchange(ref runned, 1, 0);
        }
        public void Lock()
        {
            while (Is_Runnig()) { Thread.Sleep(1); }
        }
        public void UnLock()
        {
            Interlocked.CompareExchange(ref runned, 0, 1);
        }
    }

    partial class Server {
        private int client_count = 0;
        private const int MAX_USER = 200;
        private CASLock cas_lock = new CASLock();
        private SocketPool socketpool;
        private Socket accept_socket;
        private SortedDictionary<int, Client> Client_list = new SortedDictionary<int, Client>();

        public void InitailizeServer()
        {
            RegisterHandler();

            ThreadPool.SetMaxThreads(8, 8);
            socketpool = new SocketPool(MAX_USER);
            SocketAsyncEventArgs socketarg;
            byte[] buffer;
            for (var i = 0; i < MAX_USER; ++i) {
                socketarg = new SocketAsyncEventArgs();
                socketarg.Completed += new EventHandler<SocketAsyncEventArgs>(IOCP);
                buffer = new byte[1024];
                socketarg.SetBuffer(buffer, 0, buffer.Length);
                socketarg.UserToken = new AsyncUserToken();
                socketpool.Push(socketarg);
            }
        }

        public void DestroyServer()
        {
            accept_socket.Close();
        }

        public void RunServer()
        {
            accept_socket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
            System.Net.IPEndPoint endposint = new System.Net.IPEndPoint(System.Net.IPAddress.Any, 11200);
            accept_socket.Bind(endposint);
            accept_socket.Listen(5);

            accept_socket.Blocking = false;

            StartAccept(null);
        }

        private void StartAccept(SocketAsyncEventArgs server_info)
        {
            if (null == server_info) {
                server_info = new SocketAsyncEventArgs();
                server_info.Completed += new EventHandler<SocketAsyncEventArgs>(AcceptCompleted);
            } else server_info.AcceptSocket = null;

            try {
                bool result = accept_socket.AcceptAsync(server_info);
                if (!result) {
                    //error process
                    StartAccept(null);
                }
            } catch (InvalidOperationException iop) {
                Console.WriteLine("receive iop" + iop.Message);

            } catch (SocketException se) {
                Console.WriteLine("receive se" + se.Message);
            } catch (NullReferenceException nre) {
                Console.WriteLine(nre.Message);
            } catch {
                Console.WriteLine("???");
            }
         }

        private void InitializeClient(ref SocketAsyncEventArgs client_info)
        {
            int new_id = 0;
            int check = 0;
            cas_lock.Lock();
            foreach (var value in Client_list) {
                if (check == value.Key) {
                    check++;
                    cas_lock.UnLock();
                    continue;
                }
            }
            cas_lock.UnLock();
            new_id = check;

            Client client = new Client();

            client.client_id = (short)new_id;
            client.socket = client_info;
            client.x_pos = 0;
            client.y_pos = 0;

            cas_lock.Lock();
            Client_list.Add(new_id, client);
            cas_lock.UnLock();

            ((AsyncUserToken)client_info.UserToken).client_id = new_id;

            //sc_packet_player_put 
            sc_packet_player_put my_put;
            my_put.id = (ushort)new_id;
            my_put.size = (byte)Marshal.SizeOf(typeof(sc_packet_player_put));
            my_put.type = PLAYER_PUT;

            byte[] buf = StructureToByte(my_put);
            SendProcess(client, buf);

            retry_0:
            cas_lock.Lock();
            try {
                foreach (var other_info in Client_list) {
                    if (other_info.Value.socket.AcceptSocket.Connected == false) {
                        cas_lock.UnLock();
                        CloseClientSocket(other_info.Value.socket);
                        goto retry_0;
                    }
                    cas_lock.UnLock();
                    short id = (short)other_info.Key;
                    if (id == new_id) continue;

                    Client other_client = other_info.Value;
                    cas_lock.UnLock();
                    sc_packet_player_put player_put;
                    player_put.id = (ushort)new_id;
                    player_put.size = (byte)Marshal.SizeOf(typeof(sc_packet_player_put));
                    player_put.type = PLAYER_PUT;

                    buf = StructureToByte(player_put);
                    SendProcess(other_client, buf);
                }
            } catch {
                goto retry_0;
            }

        }

        private void AcceptCompleted(object sender, SocketAsyncEventArgs server_info)
        {

            Interlocked.Increment(ref client_count);
            Console.WriteLine(client_count);

            var ServerSocket = (Socket)sender;
            SocketAsyncEventArgs client_info = socketpool.Pop();
            client_info.AcceptSocket = server_info.AcceptSocket;
            Socket socket = null;
            try {
                socket = ((AsyncUserToken)client_info.UserToken).client_socket = server_info.AcceptSocket;
            } catch {
                StartAccept(server_info);
                return;
            }
            InitializeClient(ref client_info);

            bool result = false;
            try {
                result = socket.ReceiveAsync(client_info);

                if (false == result) {
                    socket.Close();
                    socket = null;
                    socketpool.Push(server_info);
                }
            } catch (InvalidOperationException iop) {
                Console.WriteLine("receive iop" + iop.Message);
  
            } catch (SocketException se) {
                Console.WriteLine("receive se" + se.Message);
                CloseClientSocket(client_info);
            }
            catch(NullReferenceException nre) {
                Console.WriteLine(nre.Message);
            } catch {
                Console.WriteLine("???");
            }

            StartAccept(server_info);

        }

        private void ReceivePacket(SocketAsyncEventArgs client_info)
        {
            if (client_info.BytesTransferred <= 0) {
               
                CloseClientSocket(client_info);
                return;
            }
            if(client_info.SocketError != SocketError.Success) {
                // kind of socket error process
            }
            PacketProcess(client_info.Buffer);
            Socket client_socket = ((AsyncUserToken)client_info.UserToken).client_socket;
            try {
                bool result = client_socket.ReceiveAsync(client_info);
                if (!result) CloseClientSocket(client_info);
            } catch (InvalidOperationException iop) {
                Console.WriteLine("receive iop" + iop.Message);

            } catch (SocketException se) {
                Console.WriteLine("receive se" + se.Message);
            } catch (NullReferenceException nre) {
                Console.WriteLine(nre.Message);
            } catch {
                Console.WriteLine("???");
            }
        }

        private void PacketProcess(byte[] packet)
        {
            ProcessPacket value;
            if(!process_handler.TryGetValue((packet[1]), out value)) {
                Console.WriteLine("Unknown Packet Type");
            }
            value(packet);
        }

        private void SendProcess(Client client_info, byte[] packet)
        {
            SocketAsyncEventArgs socketarg = new SocketAsyncEventArgs();
            socketarg.Completed += new EventHandler<SocketAsyncEventArgs>(IOCP);

            socketarg.UserToken = client_info.socket.UserToken;
            int id = client_info.client_id;
            Socket Socket = client_info.socket.AcceptSocket;
            Socket.SendBufferSize = 0;
            socketarg.SetBuffer(packet, 0, packet.Length);

            try {
                Socket.SendAsync(socketarg);
            } catch (InvalidOperationException iop) {
                Console.WriteLine("send iop" + iop.Message);

            } catch (SocketException se) {
                Console.WriteLine("send se" + se.Message);
                CloseClientSocket(client_info.socket);
            } catch (NullReferenceException nre) {
                Console.WriteLine(nre.Message);
            }
        }

        private void SendPacket(SocketAsyncEventArgs client_info)
        {
            if(client_info.SocketError != SocketError.Success) {
                //error process;
                CloseClientSocket(client_info);
            }
            int id = ((AsyncUserToken)client_info.UserToken).client_id;
            //Console.WriteLine(id + "Player Send Complete");
        }

        private void IOCP(object sender, SocketAsyncEventArgs arg)
        {
            switch (arg.LastOperation) {
                case SocketAsyncOperation.Receive:
                    ReceivePacket(arg);
                    break;
                case SocketAsyncOperation.Send:
                    SendPacket(arg);
                    break;
                default: throw new AggregateException("UnDefine Completed Action");
            }
        }

        private void CloseClientSocket(SocketAsyncEventArgs client_info)
        {
            Socket Client_socket = ((AsyncUserToken)client_info.UserToken).client_socket;
            int id = ((AsyncUserToken)client_info.UserToken).client_id;
            Client_socket.Close();

            Interlocked.Decrement(ref client_count );

            Console.WriteLine(client_count);

            cas_lock.Lock();
            Client_list.Remove(id);
            socketpool.Push(client_info);
            cas_lock.UnLock();
        }
    }
}
