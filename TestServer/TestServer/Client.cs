using System;
using System.Net.Sockets;
namespace TestServer {
    class Client {
        public float x_pos { get; set; }
        public float y_pos { get; set; }

        public int client_id { get; set; }

        public SocketAsyncEventArgs socket { get; set; }

        public Client()
        {
            x_pos = 0;
            y_pos = 1;
            
        }

        public void Move(float x, float y)
        {
            x_pos += x;
            y_pos += y;
        }
    }
}
