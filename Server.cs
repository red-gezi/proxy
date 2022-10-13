using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;


internal class Server
{
    public static Dictionary<int, TcpClient> dic = new Dictionary<int, TcpClient>();
    public static NetworkStream clientStream = null;
    static int serverPort = int.Parse(File.ReadAllLines("config.ini")[1]);
    static Random rnd = new Random();

    public static void Init()
    {
        Task.Run(() =>
        {
            TcpListener tl = new TcpListener(766);//开一个对方可以连接的端口，今天这棒子机器连他只能1433，其他连不上，他连别人只能80 8080 21     
            tl.Start();
            while (true)
            {
                TcpClient tc1 = tl.AcceptTcpClient();
                NetworkStream ns = tc1.GetStream();
                byte[] bt = new byte[4];
                int count = ns.Read(bt, 0, bt.Length);
                if (count == 2 && bt[0] == 0x6f && bt[1] == 0x6b)
                {
                    clientStream = ns;
                    Console.WriteLine("客户端已登录");
                }
                else
                {
                    Console.WriteLine("客户端传输数据");
                    int biaoji = BitConverter.ToInt32(bt, 0);
                    TcpClient tc2 = null;
                    if (dic.ContainsKey(biaoji))
                    {
                        dic.TryGetValue(biaoji, out tc2);
                        dic.Remove(biaoji);
                        tc1.SendTimeout = 300000;
                        tc1.ReceiveTimeout = 300000;
                        tc2.SendTimeout = 300000;
                        tc2.ReceiveTimeout = 300000;
                        object obj1 = (object)(new TcpClient[] { tc1, tc2 });
                        object obj2 = (object)(new TcpClient[] { tc2, tc1 });
                        ThreadPool.QueueUserWorkItem(new WaitCallback(transfer), obj1);
                        ThreadPool.QueueUserWorkItem(new WaitCallback(transfer), obj2);
                    }
                }
                static void transfer(object obj)
                {
                    TcpClient tc1 = ((TcpClient[])obj)[0];
                    TcpClient tc2 = ((TcpClient[])obj)[1];
                    NetworkStream ns1 = tc1.GetStream();
                    NetworkStream ns2 = tc2.GetStream();
                    while (true)
                    {
                        try
                        {
                            byte[] bt = new byte[10240];
                            int count = ns1.Read(bt, 0, bt.Length);
                            ns2.Write(bt, 0, count);
                            Console.WriteLine("进行转发" + count);
                        }
                        catch
                        {
                            ns1.Dispose();
                            ns2.Dispose();
                            tc1.Close();
                            tc2.Close();
                            break;
                        }
                    }
                }
            }
        });
        Task.Run(() =>
        {
            //网页端口
            TcpListener tl = new TcpListener(serverPort); //开一个随意端口让自己的mstsc连。     
            tl.Start();
            while (true)
            {
                try
                {
                    TcpClient tc = tl.AcceptTcpClient();
                    Console.WriteLine("接收到网页");
                    int tag = rnd.Next(1000000000, 2000000000);
                    dic.Add(tag, tc);
                    byte[] bt = BitConverter.GetBytes(tag);
                    clientStream.Write(bt, 0, bt.Length);
                    Console.WriteLine("向客户端写入数据" + bt.Length);
                }
                catch (Exception)
                {
                }
            }
        });
    }
}