using System;
using System.Collections.Generic;
using System.Net.Sockets;

namespace DummyClient {

    class Vector2D {

        public float x_pos { get; set; }
        public float y_pos { get; set; }

        public Vector2D()
        {
            x_pos = y_pos = 0;
        }
        public Vector2D(float x, float y)
        {
            x_pos = x; y_pos = y;
        }
        public void Add(float x, float y)
        {
            x_pos += x; y_pos += y;
        }
        public void Add(Vector2D vec2d)
        {
            x_pos += vec2d.x_pos;
            y_pos += vec2d.y_pos;
        }
    }

    class Player {
        Vector2D Position;
        public Network tcpconnect;
        
        public int GetId() { return tcpconnect.id; }

        public Player()
        {

        }
        public Player(Vector2D pos)
        {
            Position = pos;
            tcpconnect = new Network();
            tcpconnect.InitialzeNetWork();
     
        }

        public void Move(Vector2D vec2d)
        {
            Position.Add(vec2d);
        }
        public void Move()
        {
            tcpconnect.SendPacket(DummyClient.Network.PLAYER_MOVE);
        }
    }
}
