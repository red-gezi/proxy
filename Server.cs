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
                string recStr = Encoding.ASCII.GetString(recByte, 0, bytes);
                if (recStr == "C")
                {
                    Console.ForegroundColor = ConsoleColor.Blue;
                    Console.WriteLine($"确认为'穿透客户端'"+ newSocket.RemoteEndPoint);
                    Console.ForegroundColor = ConsoleColor.White;
                    clinetSocket = newSocket;
                }
                else
                {
                    Console.ForegroundColor = ConsoleColor.Red;
                    Console.WriteLine("确认为'浏览器'" + newSocket.RemoteEndPoint);
                    Console.ForegroundColor = ConsoleColor.White;
                    if (!clinetSocket.Connected)
                    {
                        continue;
                    }
                    Console.ForegroundColor = ConsoleColor.Yellow;
                    Console.WriteLine($"{newSocket.RemoteEndPoint}---->{clinetSocket.RemoteEndPoint}({clinetSocket.LocalEndPoint}):----发送给客户端长度:{recByte.Length}");
                    Console.ForegroundColor = ConsoleColor.White;
                    clinetSocket?.Send(recByte, recByte.Length, 0);
                    _ = Task.Run(() =>
                      {
                          while (true)
                          {
                              try
                              {
                                  byte[] result = new byte[1024];
                                  int num = clinetSocket.Receive(result, result.Length, SocketFlags.None);
                                  if (num == 0) break;//接受空包关闭连接
                                  Console.WriteLine($"{clinetSocket.RemoteEndPoint}({clinetSocket.LocalEndPoint})---->{newSocket.RemoteEndPoint}:----返回给网页长度:{num}");
                                  newSocket.Send(result, num, SocketFlags.None);
                              }
                              catch (Exception e)
                              {
                                  Console.WriteLine(e.Message);
                                  newSocket.Dispose();
                                  newSocket = null;
                                  break;
                              }
                          }
                      });
                }
            }
        });
    }
}