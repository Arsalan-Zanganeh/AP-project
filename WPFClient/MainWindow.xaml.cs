using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using LibraryClass;
using Microsoft.AspNetCore.SignalR.Client;

namespace WPFClient
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        HubConnection connection;
        HubConnection connectionInterest;
        HubConnection connectionToDo;
        ClassNeccessary myObject = new ClassNeccessary();

        public MainWindow()
        {
            InitializeComponent();

            connection = new HubConnectionBuilder()
                .WithUrl("http://localhost:5044/chathubmessage")
                .WithAutomaticReconnect()
                .Build();

            connectionInterest = new HubConnectionBuilder()
                .WithUrl("http://localhost:5044/chathubinterest")
                .WithAutomaticReconnect()
                .Build();

            connectionToDo = new HubConnectionBuilder()
                .WithUrl("http://localhost:5044/chathubtodo")
                .WithAutomaticReconnect()
                .Build();


            connection.Reconnecting += (sender)=>
            {
                this.Dispatcher.InvokeAsync(()=>
                {
                    var newMessage = "Attempting to reconnect...";
                    messages.Items.Add(newMessage);
                });

                return Task.CompletedTask;
            };

            connection.Reconnected += (sender)=>
            {
                this.Dispatcher.InvokeAsync(()=>
                {
                    var newMessage = "Reconnected to the server";
                    messages.Items.Clear();
                    messages.Items.Add(newMessage);
                });

                return Task.CompletedTask;
            };

            connection.Closed += (sender)=>
            {
                this.Dispatcher.InvokeAsync(()=>
                {
                    var newMessage = "Connection Closed";
                    messages.Items.Add(newMessage);
                    openConnection.IsEnabled = true;
                    SendTask.IsEnabled = false;
                });

                return Task.CompletedTask;
            };

        }

        private async void openConnection_Click(object sender,RoutedEventArgs e)
        {

            connectionInterest.On<Interest>("ReceiveEdit",t=>
            {
                this.Dispatcher.InvokeAsync(()=>
                {
                    messages.Items.Clear();
                    lock(myObject.locker)
                    {
                        foreach(var i in myObject.GetInterests())
                        {
                            messages.Items.Add($"{i.Id},{i.Name}");
                        }
                    }
                });
            });

            connectionToDo.On<ToDo>("ReceiveEdit",t=>
            {
                this.Dispatcher.InvokeAsync(()=>
                {
                    messages.Items.Clear();
                    lock(myObject.locker)
                    {
                        foreach(var i in myObject.GetToDos())
                        {
                            messages.Items.Add($"{i.Id},{i.Name},{i.Answer},{i.DueDate},{i.IsCompleted}");
                        }
                    }
                });
            });

            connection.On<string,string>("ReceiveMessage",(user,msg)=>
            {
                this.Dispatcher.InvokeAsync(()=>
                {

                });
            });

            try
            {
                await connection.StartAsync();
                await connectionInterest.StartAsync();
                await connectionToDo.StartAsync();
                messages.Items.Add("Connection Started");
                openConnection.IsEnabled = false;
                SendTask.IsEnabled=true;
                SendBreakTask.IsEnabled=true;
                SendGetInspirationTask.IsEnabled=true;
                GetToDos.IsEnabled=true;
                GetInterests.IsEnabled=true;
            }
            catch (Exception ex)
            {
                messages.Items.Add(ex.Message);
            }

        }

        private async void SendTask_Click(object sender,RoutedEventArgs e)
        {
            try
            {
                await connection.InvokeAsync("SendMessage","WPF Client",TaskInput.Text);
                var result = await myObject.DoTask(TaskInput.Text,TaskInputAddMonth.Text,TaskInputAddDay.Text,TaskInputAddHour.Text);
                myObject.SaveToDo(TaskInput.Text,result.Replace("\n","").Replace(","," "),DateTime.Now);
                await connectionToDo.InvokeAsync("SendEdit",new ToDo());
                TaskInput.Clear();
                TaskInputAddMonth.Clear();
                TaskInputAddDay.Clear();
                TaskInputAddHour.Clear();
                await connection.InvokeAsync("SendMessage","ChatGPT",result);
            }
            catch(Exception ex)
            {
                messages.Items.Add(ex.Message);
            }
        }

        private async void SendBreakTask_Click(object sender,RoutedEventArgs e)
        {
            try
            {
                await connection.InvokeAsync("SendMessage","WPF Client",BreakTaskInput.Text);
                var result = await myObject.BreakTask(BreakTaskInput.Text,BreakTaskInputInto.Text);
                foreach(var i in result)
                {
                    await connection.InvokeAsync("SendMessage","ChatGPT",i);
                    myObject.SaveToDo(i.Split(",")[0],i.Split(",")[1],DateTime.Now);
                }
                await connectionToDo.InvokeAsync("SendEdit",new ToDo());
                BreakTaskInput.Clear();
                BreakTaskInputInto.Clear();
            }
            catch(Exception ex)
            {
                messages.Items.Add(ex.Message);
            }
        }
        private async void SendGetInspirationTask_Click(object sender,RoutedEventArgs e)
        {
            try
            {
                await connection.InvokeAsync("SendMessage","WPF Client",$"I want to {GetInspirationForInput.Text}. give me inspiration.");
                var result = await myObject.InspireForTask(GetInspirationForInput.Text,GetInspirationByInput.Text);
                myObject.SaveToDo(GetInspirationForInput.Text,result.Replace("\n","").Replace(","," "),DateTime.Now);
                await connectionToDo.InvokeAsync("SendEdit",new ToDo());
                GetInspirationForInput.Clear();
                GetInspirationByInput.Clear();
                await connection.InvokeAsync("SendMessage","ChatGPT",result);
                TaskInput.Clear();
            }
            catch(Exception ex)
            {
                messages.Items.Add(ex.Message);
            }            
        }
        private async void GetToDos_Click(object sender,RoutedEventArgs e)
        {
            messages.Items.Clear();
            foreach(var i in myObject.GetToDos())
            {
                messages.Items.Add($"{i.Id},{i.Name},{i.Answer},{i.DueDate},{i.IsCompleted}");
            }
            await connectionToDo.InvokeAsync("SendEdit",new ToDo());
        }
        private void GetInterests_Click(object sender,RoutedEventArgs e)
        {
            messages.Items.Clear();
            foreach(var i in myObject.GetInterests())
            {
                messages.Items.Add($"{i.Id},{i.Name}");
            }
            connectionInterest.InvokeAsync("SendEdit",new Interest());
        }        
    }
}
