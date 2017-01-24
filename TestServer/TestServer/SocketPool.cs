using System;
using System.Collections.Generic;
using System.Threading;
using System.Net.Sockets;

namespace TestServer {

    class AsyncUserToken {
        Socket socket;
        int id;
        public AsyncUserToken() : this(null) { }
        public AsyncUserToken(Socket s) { socket = s; id = -1; }
        public Socket client_socket
        {
            get { return socket; }
            set { socket = value; }
        }

        public int client_id
        {
            get { return id; }
            set { id = value; }
        }
    }

    class SocketPool {
        Stack<SocketAsyncEventArgs> pool;
        CASLock caslock = new CASLock();

        public SocketPool(int capacity)
        {
            pool = new Stack<SocketAsyncEventArgs>();
        }

        public void Push(SocketAsyncEventArgs arg)
        {
            if (arg == null) throw new ArgumentNullException("SocketPool Null Exception");
            else {
                caslock.Lock();
                pool.Push(arg);
                caslock.UnLock();
            }
        }

        public SocketAsyncEventArgs Pop()
        {
            caslock.Lock();
            SocketAsyncEventArgs temparg = pool.Pop();
            caslock.UnLock();
            return temparg;
        }
    }
}
