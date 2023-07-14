using LibraryClass;
using Microsoft.AspNetCore.SignalR.Client;
// See https://aka.ms/new-console-template for more information
Console.WriteLine("Hello, World!");


System.Console.Write("User? ");
string user = System.Console.ReadLine();

ClassNeccessary myObject = new ClassNeccessary();

var connectionMessage = new HubConnectionBuilder()
    .WithUrl("http://localhost:5044/chathubmessage")
    .Build();

var connectionInterest = new HubConnectionBuilder()
    .WithUrl("http://localhost:5044/chathubinterest")
    .Build();

var connectionToDo = new HubConnectionBuilder()
    .WithUrl("http://localhost:5044/chathubtodo")
    .Build();


connectionMessage.On<string,string>("ReceiveMessage", (user,message) =>
{
    System.Console.Out.WriteLine($"{user}: {message}");
});


connectionInterest.On<Interest>("ReceiveEdit",inte =>
{
    Console.Clear();
    lock(myObject.locker)
    {
        foreach(var i in myObject.GetInterests())
        {
            System.Console.Out.WriteLine($"{i.Id},{i.Name}");
        }
    }
    System.Console.Out.Write("> ");
});

connectionToDo.On<ToDo>("ReceiveEdit",t =>
{
    Console.Clear();
    lock(myObject.locker)
    {
        foreach(var i in myObject.GetToDos())
        {
            System.Console.Out.WriteLine($"{i.Id},{i.Name},{i.DueDate},{i.IsCompleted}");
            System.Console.WriteLine($"{i.Answer}");
            System.Console.WriteLine("  ");
        }
    }
    System.Console.Out.Write("> ");
});



connectionMessage.StartAsync().Wait();
connectionInterest.StartAsync().Wait();
connectionToDo.StartAsync().Wait();

System.Console.Out.WriteLine("Guide: use numbers to do below tasks. you can also give tasks to be added to database.");
System.Console.Out.WriteLine("1 : Show Tasks");
System.Console.Out.WriteLine("2 : Show Interests");
System.Console.Out.WriteLine("3 : Remove Interest by Id");
System.Console.Out.WriteLine("4 : Remove Task by Id");
System.Console.Out.Write("> ");

while(true)
{


    string msg = System.Console.ReadLine();

    try
    {
        if(msg[0]=='1')
        {
                foreach(var i in myObject.GetToDos())
                {
                    System.Console.Out.WriteLine($"{i.Id},{i.Name},{i.Answer},{i.DueDate},{i.IsCompleted}");
                }
            connectionToDo.InvokeAsync("SendEdit",new ToDo()).Wait();
        }

        else if(msg[0]=='2')
        {
                foreach(var i in myObject.GetInterests())
                {
                    System.Console.Out.WriteLine($"{i.Id},{i.Name}");
                }
            connectionInterest.InvokeAsync("SendEdit",new Interest()).Wait();
        }

        else if(msg[0]=='3')
        {
            System.Console.Out.Write("Interest Id to remove:");
            var msgg=System.Console.ReadLine();
            try
            {
                myObject.RemoveInterest(Convert.ToInt32(msgg));
                connectionInterest.InvokeAsync("SendEdit",new Interest()).Wait();
            }
            catch
            {

            }
        }

        else if(msg[0]=='4')
        {
            System.Console.Out.Write("Task Id to remove:");
            var msgg=System.Console.ReadLine();
            try
            {
                myObject.RemoveToDo(Convert.ToInt32(msgg));
                connectionInterest.InvokeAsync("SendEdit",new ToDo()).Wait();
            }
            catch
            {

            }
        }

        else
        {
            connectionMessage.InvokeAsync("SendMessage",user,msg).Wait();
            var result = await myObject.UseChatGPT(msg);
            myObject.SaveToDo(msg,result.Replace("\n","").Replace(",",":"),DateTime.Now);
            connectionMessage.InvokeAsync("SendMessage","ChatGPT",result).Wait();
        }
    }
    catch
    {
        
    }
}