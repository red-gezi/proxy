using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;


internal class Client
{
    public static NetworkStream clientStream = null;
    static string host = File.ReadAllLines("config.ini")[0];
    static int targetPort = int.Parse(File.ReadAllLines("config.ini")[2]);
    public static void Init()
    {
        try
        {

            TcpClient tc = new TcpClient(host, 766);
            clientStream = tc.GetStream();
            byte[] bt = Encoding.Default.GetBytes("ok");//这里发送一个连接提示  
            clientStream.Write(bt, 0, bt.Length);
            Console.WriteLine("链接服务端");
            while (true)
            {
                byte[] tag = new byte[4];
                Console.WriteLine("等待数据流");
                clientStream.Read(tag, 0, tag.Length);
                Console.WriteLine("接收到数据流目标编号"+ tag.Length);
                TcpClient tc1 = new TcpClient(host, 766);
                TcpClient tc2 = new TcpClient("127.0.0.1", targetPort);
                tc1.SendTimeout = 300000;
                tc1.ReceiveTimeout = 300000;
                tc2.SendTimeout = 300000;
                tc2.ReceiveTimeout = 300000;
                tc1.GetStream().Write(tag, 0, tag.Length);
                object obj1 = (object)(new TcpClient[] { tc1, tc2 });
                object obj2 = (object)(new TcpClient[] { tc2, tc1 });
                ThreadPool.QueueUserWorkItem(new WaitCallback(transfer), obj1);
                ThreadPool.QueueUserWorkItem(new WaitCallback(transfer), obj2);
            }
        }
        catch { }
      
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
                    Console.WriteLine("进行转发"+ count);
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
}