using System.Net;
using System.Net.Sockets;
using System.Text;

internal class Client
{
    static Socket clientSocket;
    static Socket targetSocket;
    public static void Init()
    {
        string host = File.ReadAllLines("config.ini")[0];
        int serverPort = int.Parse(File.ReadAllLines("config.ini")[1]);
        int targetPort = int.Parse(File.ReadAllLines("config.ini")[2]);
        clientSocket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
        targetSocket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
        //设置端口可复用
        clientSocket.SetSocketOption(SocketOptionLevel.Socket, SocketOptionName.ReuseAddress, true);
        //连接服务端
        clientSocket.Connect(host, serverPort);
        targetSocket.Connect(new IPEndPoint(IPAddress.Parse(host), targetPort));

        clientSocket.Send(Encoding.ASCII.GetBytes("C"));
        new Porxy("127.0.0.1", targetPort).Run();
        Console.ReadLine();
    }
    public class Porxy
    {
        int TargetPort { get; set; }
        string TargetIp { get; set; }
        public Porxy(string TargetIp, int TargetPort)
        {

            this.TargetIp = TargetIp;
            this.TargetPort = TargetPort;
        }

        public void Run()
        {
            //监听客户端连接
            Task.Run(async () =>
            {
                while (true)
                {
                    byte[] result = new byte[1024];
                    int num = clientSocket.Receive(result, result.Length, SocketFlags.None);
                    if (num == 0) break;//接受空包关闭连接
                    Console.ForegroundColor = ConsoleColor.DarkYellow;
                    Console.WriteLine($"{clientSocket.RemoteEndPoint}---->{targetSocket.RemoteEndPoint}({clientSocket.LocalEndPoint}):----接收长度:{num}");
                    Console.ForegroundColor = ConsoleColor.White;
                    targetSocket.Send(result, num, SocketFlags.None);
                    _ = Task.Run(() =>
                    {
                        while (true)
                        {
                            byte[] result = new byte[1024];
                            try
                            {
                                int num = targetSocket.Receive(result, result.Length, SocketFlags.None);
                                if (num == 0) break; //接受空包关闭连接
                                Console.WriteLine($"{targetSocket.RemoteEndPoint}---->{clientSocket.RemoteEndPoint}({clientSocket.LocalEndPoint}):----转发长度:{num}");
                                clientSocket.Send(result, num, SocketFlags.None);
                            }
                            catch (Exception)
                            {
                            }
                          
                        }
                    });
                }
            });
        }

    }
}