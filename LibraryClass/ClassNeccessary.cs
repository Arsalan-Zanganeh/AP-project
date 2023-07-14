using System;
using System.Globalization;
using OpenAI_API;
using OpenAI_API.Completions;


namespace LibraryClass;



public class ClassNeccessary
{
    OpenAIAPI openai = new OpenAIAPI(@"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");
    CompletionRequest completionRequest = new CompletionRequest();
    public object locker = new Object();
    public async Task<string> UseChatGPT(string query)
    {
        string outputResult = "";
        completionRequest.Prompt = query;
        completionRequest.Model = OpenAI_API.Models.Model.DavinciText;
        completionRequest.MaxTokens = 1024;

        var completions = await openai.Completions.CreateCompletionAsync(completionRequest);

        foreach (var completion in completions.Completions)
        {
            outputResult += completion.Text;
        }

        return outputResult;
    }

    public async Task<List<string>> BreakTask(string mytask,string minutes)
    {
        try
        {
            var wanted = new List<string>{};
            var minute=Convert.ToInt32(minutes);
            var result = await UseChatGPT($"break my task into {minute} minutes subtasks.use a json template that has name and description foreach task.my task is {mytask}");
            var splittedResult = result.Split("\n");
            string onAir="";
            foreach(var i in splittedResult)
            {
                if((!i.Contains('{')) & (!i.Contains('}')) & (!i.Contains('[')) & (!i.Contains(']')))
                {
                    if(i.Split(":")[0].Contains('m'))
                    {
                        onAir+=i.Split(":")[1].Replace("\"", "");
                    }
                    else if(i.Split(":")[0].Contains('d'))
                    {
                        onAir+=i.Split(":")[1].Replace("\"", "");
                        wanted.Add($"{onAir}");
                        onAir="";
                    }
                }
            }
            return wanted;
        }
        catch
        {
            var wanted = new List<string>{};
            var result = await UseChatGPT($"break my task into several subtasks. use a json template that has name and description foreach task.my task is {mytask}");
            var splittedResult = result.Split("\n");
            string onAir="";
            foreach(var i in splittedResult)
            {
                if((!i.Contains('{')) & (!i.Contains('}')) & (!i.Contains('[')) & (!i.Contains(']')))
                            {
                                if(i.Split(":")[0].Contains('m'))
                                {
                                    onAir+=i.Split(":")[1].Replace("\"", "");
                                }
                                else if(i.Split(":")[0].Contains('d'))
                                {
                                    onAir+=i.Split(":")[1].Replace("\"", "");
                                    wanted.Add($"{onAir}");
                                    onAir="";
                                }
                            }
            }
            return wanted;                
        }
    }


    public async Task<string> InspireForTask(string task,string myInterest)
    {
        if(myInterest!=null & myInterest!="")
        {
            return await UseChatGPT($"I want to do my task but i am tired. please give me motivation by {myInterest} to do my task. my task is {task}");
        }
        else
        {
            var wanted=GetInterests();
            Random rnd=new Random();
            int index=rnd.Next(0,wanted.Count);
            var wantedInterest=wanted.ElementAt(index).Name;
            return await UseChatGPT($"I want to do my task but i am tired. please give me motivation by {wantedInterest} to do my task. my task is {task}");
        }
    }

    public DateTime GetTime(string addMonth,string addDay,string addHour)
    {
        DateTime now=new DateTime(DateTime.Now.Year,DateTime.Now.Month,DateTime.Now.Day,DateTime.Now.Hour,DateTime.Now.Minute,DateTime.Now.Second);
        try
        {
            now.AddMonths(Convert.ToInt32(addMonth));
            try
            {
                now.AddDays(Convert.ToDouble(addDay));
                try
                {
                    now.AddHours(Convert.ToDouble(addHour));
                }
                catch
                {
                    
                }
            }
            catch
            {
                try
                {
                    now.AddHours(Convert.ToDouble(addHour));
                }
                catch
                {
                    
                }
            }
        }
        catch
        {
            try
            {
                now.AddDays(Convert.ToDouble(addDay));
                try
                {
                    now.AddHours(Convert.ToDouble(addHour));
                }
                catch
                {

                }
            }
            catch
            {
                try
                {
                    now.AddHours(Convert.ToDouble(addHour));
                }
                catch
                {

                }
            }
        }
        return now;
    }

    public async Task<string> DoTask(string myTask,string addMonth,string addDay,string addHour)
    {
        DateTime now=GetTime(addMonth,addDay,addHour);
        var result = await UseChatGPT($"I want to {myTask}. give me the instructions as simple as possible.");
        return result;
    }

    string InterestsFilePath=@"..\LibraryClass\Interests.txt";
    string ToDosFilePath=@"..\LibraryClass\ToDos.txt";

    public List<Interest> GetInterests()
    {
        var wanted=new List<Interest> {};
        var result=File.ReadAllLines(InterestsFilePath)
        .Select(l=>
        {
            var toks=l.Split(",");
            try
            {
                return (id:Convert.ToInt32(toks[0]),name:toks[1]);
            }
            catch
            {
                return (id:0,name:"");
            }
        });
        foreach(var tuple in result)
        {
            if(tuple.id!=0)
            {
                wanted.Add(new Interest()
                {
                    Id=tuple.id,
                    Name=tuple.name
                });
            }
        }
        return wanted;
    }


    public void SaveInterest(string name)
    {
        var wanted=GetInterests();
        int LastId=wanted.Count;
        bool contains=wanted.Any(x=>x.Name.ToLower()==name.ToLower());
        if(!contains)
        {
            wanted.Add(new Interest()
            {
                Id=LastId+1,
                Name=name
            });
            var wanted2=new List<string>{};
            foreach(var i in wanted)
            {
                wanted2.Add($"{i.Id},{i.Name}");
            }
            File.WriteAllLines(InterestsFilePath,wanted2);
        }
    }


    public void EditInterest(int id,string newName)
    {
        var interests=GetInterests();
        var mylist=new List<string>{};
        bool contains=interests.Any(x=>x.Id==id);
        if(contains)
        {
            foreach(var i in interests)
            {
                if(i.Id!=id)
                {
                    mylist.Add($"{i.Id},{i.Name}");
                }
                else
                {
                    mylist.Add($"{i.Id},{newName}");
                }
            }
            File.WriteAllLines(InterestsFilePath,mylist);
        }
    }

    public void RemoveInterest(int id)
    {
        var interests=GetInterests();
        var mylist=new List<string>{};
        bool contains=interests.Any(x=>x.Id==id);
        int j=1;
        if(contains)
        {
            foreach(var i in interests)
            {
                if(i.Id!=id)
                {
                    mylist.Add($"{j},{i.Name}");
                    j++;
                }
            }
            File.WriteAllLines(InterestsFilePath,mylist);
        }
    }


    public List<ToDo> GetToDos()
    {
        var wanted=new List<ToDo> {};
        var result = File.ReadAllLines(ToDosFilePath)
        .Select(l=>
        {
            var toks=l.Split(",");
            try
            {
                return (id:Convert.ToInt32(toks[0]),name:toks[1],answer:toks[2],duedate:DateTime.ParseExact(toks[3],"yyyy-MM-dd HH:mm:ss",CultureInfo.InvariantCulture),iscompleted:Convert.ToBoolean(toks[4]));
            }
            catch
            {
                return (id:0,name:"",answer:"",duedate:DateTime.Now,iscompleted:false);
            }
        });
        foreach(var tuple in result)
        {
            if(tuple.id!=0)
            {
                wanted.Add(new ToDo()
                {
                    Id=tuple.id,
                    Name=tuple.name,
                    Answer=tuple.answer,
                    DueDate=tuple.duedate,
                    IsCompleted=tuple.iscompleted
                });
            }
        }
        return wanted;
    }


    public void SaveToDo(string name,string answer,DateTime duedate)
    {
        var result=GetToDos();
        bool contain=result.Any(x=>x.Name.ToLower()==name.ToLower());
        if(!contain)
        {
            var wanted=new List<string>{};
            foreach(var i in result)
            {
                wanted.Add($"{i.Id},{i.Name},{i.Answer},{i.DueDate.ToString("yyyy-MM-dd HH:mm:ss")},{i.IsCompleted}");
            }
            wanted.Add($"{result.Count+1},{name},{answer},{duedate.ToString("yyyy-MM-dd HH:mm:ss")},{false}");
            File.WriteAllLines(ToDosFilePath,wanted);
        }
    }


    public void EditToDo(int id,bool newIsCompleted,DateTime newDueDate)
    {
        var result = GetToDos();
        var mylist = new List<string>{};
        bool contains=result.Any(x=>x.Id==id);
        if(contains)
        {
            foreach(var i in result)
            {
                if(i.Id!=id)
                {
                    mylist.Add($"{i.Id},{i.Name},{i.Answer},{i.DueDate.ToString("yyyy-MM-dd HH:mm:ss")},{i.IsCompleted}");
                }
                else
                {
                    mylist.Add($"{i.Id},{i.Name},{i.Answer},{newDueDate.ToString("yyyy-MM-dd HH:mm:ss")},{newIsCompleted.ToString()}");
                }
            }
            File.WriteAllLines(ToDosFilePath,mylist);
        }
    }


    public void RemoveToDo(int id)
    {
        var result = GetToDos();
        var mylist = new List<string>{};
        bool contains=result.Any(x=>x.Id==id);
        int j=1;
        if(contains)
        {
            foreach(var i in result)
            {
                if(i.Id!=id)
                {
                    mylist.Add($"{j},{i.Name},{i.Answer},{i.DueDate.ToString("yyyy-MM-dd HH:mm:ss")},{i.IsCompleted}");
                    j++;
                }
            }
            File.WriteAllLines(ToDosFilePath,mylist);
        }
    }



}

public class ToDo
{
    public int Id {get; set;}
    public string Name {get; set;}
    public string Answer {get; set;}
    public DateTime DueDate {get; set;}
    public bool IsCompleted {get; set;}

}

public class Interest
{
    public int Id {get; set;}
    public string Name {get; set;}
}