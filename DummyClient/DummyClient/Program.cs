using System;
using System.Threading;
using System.Collections.Generic;

namespace DummyClient {

    static class Program {

        static List<Player> Player_list = new List<Player>();

        static int count = 0;
        static public int x ;

        static private void Update()
        {
            for(var i =0; i < 50; ++i) {
                Player player = new Player(new Vector2D());
                Player_list.Add(player);
            }
            Thread.Sleep(5000);

            foreach(var v in Player_list) {
                if(v.tcpconnect.ConnectSocket.Connected == false) {
                    count++;
                }
            }

            Console.WriteLine("Unconnected = " + count);
            Console.WriteLine("Connected = " + x);


            while (true) {
                foreach (var value in Player_list) {
                    Random rand = new Random();
                    value.Move();
                }
            }
        }
        static void Main(string[] args)
        {
            Update();
        }
    }
}
