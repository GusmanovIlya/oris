namespace StudentApi.Models;

public sealed class Student
{
    public long Id { get; set; }                 
    public string Name { get; set; } = "";    
    public string Surname { get; set; } = "";    
    public int Age { get; set; }                
    public string Nationality { get; set; } = ""; 
    public string Profession { get; set; } = ""; 
}
