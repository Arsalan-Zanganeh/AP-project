using System;
using Microsoft.EntityFrameworkCore;

public class ToDo
{
    public int Id {get; set;}
    public string Name {get; set;}
    public string HelpMode {get; set;}
    public string Answer {get; set;}
    public DateTime DueDate {get; set;}
    public bool IsCompleted {get; set;}
}

public class Interest
{
    public int Id {get; set;}
    public string Name {get; set;}
}

public class SyncCollectionMaker : DbContext
{
    public DbSet<ToDo> ToDos {get; set;}

    public DbSet<Interest> Interests {get; set;}

    protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder) 
    {
        optionsBuilder.UseSqlite(@"Data Source=data.db");
    }  

}