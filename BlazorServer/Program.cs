using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Web;
using BlazorServer.Data;
using Microsoft.AspNetCore.SignalR;
using Microsoft.AspNetCore.ResponseCompression;
using LibraryClass;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddRazorPages();
builder.Services.AddServerSideBlazor();
builder.Services.AddSingleton<WeatherForecastService>();
builder.Services.AddResponseCompression(opts =>
{
    opts.MimeTypes = ResponseCompressionDefaults.MimeTypes.Concat(
        new[] { "application/octet-stream"}
    );
});



var app = builder.Build();

// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Error");
    // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
    app.UseHsts();
}

app.UseHttpsRedirection();

app.UseStaticFiles();

app.UseRouting();

app.MapBlazorHub();
app.MapHub<ChatHub<string>>("/chathubmessage");
app.MapHub<ChatHub<Interest>>("/chathubinterest");
app.MapHub<ChatHub<ToDo>>("/chathubtodo");
app.MapFallbackToPage("/_Host");

app.Run();


public class ChatHub<T>:Hub
{
    public Task SendMessage(T user,T message)
    {
        return Clients.All.SendAsync("ReceiveMessage",user,message);
    }
    public Task SendEdit(T type)
    {
        return Clients.All.SendAsync("ReceiveEdit",type);
    }
}
