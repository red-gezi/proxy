using System.Net;
using System.Net.Sockets;
using System.Text;
using WebSocketSharp;
using WebSocketSharp.Server;
internal class Server
{
    static Socket clinetSocket;
    public static void Init()
    {
        int port = int.Parse(File.ReadAllLines("config.ini")[1]);
        IPEndPoint ipe = new IPEndPoint(IPAddress.Any, port);
        Socket sSocket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
        sSocket.Bind(ipe);
        sSocket.Listen(100);
        Console.WriteLine("监听已经打开，请等待");

        Task.Run(() =>
        {
            while (true)
            {
                Socket newSocket = sSocket.Accept();
                Console.WriteLine("接收到链接");
                byte[] recByte = new byte[4096];
                int bytes = newSocket.Receive(recByte);
                Console.WriteLine(" from {0}", newSocket.RemoteEndPoint?.ToString());
                string recStr = Encoding.ASCII.GetString(recByte, 0, bytes);
                if (recStr == "C")
                {
                    Console.WriteLine("确认为穿透客户端");
                    clinetSocket = newSocket;
                    Console.WriteLine("客户端1:{0}", recStr);
                }
                else
                {
                    Console.WriteLine("确认为访问者,进行转发");
                    clinetSocket?.Send(recByte, recByte.Length, 0);
                    _ = Task.Run(() =>
                      {
                          while (true)
                          {
                              if (!newSocket.Connected)
                              {
                                  newSocket.Dispose();
                                  newSocket = null;
                                  break;
                              }
                              byte[] result = new byte[1024];
                              int num = clinetSocket.Receive(result, result.Length, SocketFlags.None);
                              if (num == 0) break;//接受空包关闭连接
                              Console.WriteLine("转发" + num);
                              if (!newSocket.Connected)
                              {
                                  newSocket.Dispose();
                                  newSocket = null;
                                  break;
                              }
                              try
                              {
                                  newSocket.Send(result, num, SocketFlags.None);
                              }
                              catch (Exception e)
                              {
                                  Console.WriteLine(e.Message); 
                              }
                          }
                      });
                }
            }
        });
    }
}