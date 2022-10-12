// See https://aka.ms/new-console-template for more information
Console.WriteLine("启动程序，请输入1或2选择启动服务器或客户端");
string select = Console.ReadLine();
if (select =="1")
{
    Server.Init();
}
else if (select =="2")
{
    Client.Init();
}
Console.WriteLine("按回车停止");
Console.ReadLine();
