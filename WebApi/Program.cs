// See https://aka.ms/new-console-template for more information
using LibraryClass;
using System.Threading.Tasks;

Console.WriteLine("Hello, World!");
using SyncCollectionMaker DbCollection = new SyncCollectionMaker();

ClassNeccessary myobject=new ClassNeccessary();

DateTime now = new DateTime(DateTime.Now.Year,DateTime.Now.Month,DateTime.Now.Day,DateTime.Now.Hour,DateTime.Now.Minute,DateTime.Now.Second);


var builder = WebApplication.CreateBuilder(args);
builder.Services.AddSwaggerGen();
builder.Services.AddEndpointsApiExplorer();

var app = builder.Build();
app.UseSwagger();
app.UseSwaggerUI();


app.MapPost("Create Interest",(string name) =>
{
    int LastId=DbCollection.Interests.ToList().Count;
    if(!DbCollection.Interests.ToList().Any(i=>i.Name==name))
    {
        DbCollection.Interests.Add(new Interest()
        {
            Id=LastId+1,
            Name=name
        });
        DbCollection.SaveChanges();
    }
});

app.MapGet("Read Interests", ()=> DbCollection.Interests.ToList().OrderBy(l=>l.Id));

app.MapPost("Update Interest",(int id,string newName)=>
{
    var updateInterest=DbCollection.Interests.Find(id);
    if(updateInterest!=null)
    {
        updateInterest.Name=newName;
        DbCollection.SaveChanges();
    }
});

app.MapPost("Delete Interest",(int id)=>
{
    var deleteInterest=DbCollection.Interests.Find(id);
    if(deleteInterest!=null)
    {
        DbCollection.Interests.Remove(deleteInterest);
        DbCollection.SaveChanges();
        for(int i=id;i<=DbCollection.Interests.ToList().Count;i++)
        {
            var deleteInterest2=DbCollection.Interests.Find(i+1);
            var name = deleteInterest2.Name;
            DbCollection.Interests.Remove(deleteInterest2);
            DbCollection.SaveChanges();
            DbCollection.Add(new Interest()
            {
                Id=i,
                Name=name
            });
            DbCollection.SaveChanges();
        }
    }
});



app.MapPost("Create ToDo",async(string todoName) => 
{
    if(!DbCollection.ToDos.ToList().Any(l=>l.Name==todoName))
    {
        var result =await myobject.UseChatGPT($"I want to {todoName}.Give me the instructions.");
        int LastId=DbCollection.ToDos.ToList().Count;
        DbCollection.ToDos.Add(new ToDo()
        {
            Id=LastId+1,
            Name=todoName,
            HelpMode="Normal",
            Answer=result.Replace("\n",""),
            DueDate=DateTime.Now,
            IsCompleted=false
        });
        DbCollection.SaveChanges();
    }
});

app.MapGet("Read ToDos",() => DbCollection.ToDos.ToList().OrderBy(l=>l.Id));

app.MapPost("Update ToDo",(int id,bool newIsCompleted)=>
{
    var updateToDo = DbCollection.ToDos.Find(id);
    if(updateToDo!=null)
    {
        updateToDo.DueDate=DateTime.Now;
        updateToDo.IsCompleted=newIsCompleted;
        DbCollection.SaveChanges();
    }
});

app.MapPost("Delete ToDo",(int id)=>
{
    var deleteToDo = DbCollection.ToDos.Find(id);
    if(deleteToDo!=null)
    {
        DbCollection.ToDos.Remove(deleteToDo);
        DbCollection.SaveChanges();
        for(int i=id;i<=DbCollection.ToDos.ToList().Count;i++)
        {
            var deleteToDo2=DbCollection.ToDos.Find(i+1);
            var name=deleteToDo2.Name;
            var helpMode=deleteToDo2.HelpMode;
            var answer=deleteToDo2.Answer;
            var dueDate=deleteToDo2.DueDate;
            var isCompleted=deleteToDo2.IsCompleted;
            DbCollection.ToDos.Remove(deleteToDo2);
            DbCollection.SaveChanges();
            DbCollection.ToDos.Add(new ToDo()
            {
                Id=i,
                Name=name,
                HelpMode=helpMode,
                Answer=answer,
                DueDate=dueDate,
                IsCompleted=isCompleted
            });
            DbCollection.SaveChanges();
        }
    }
});

app.Run();

