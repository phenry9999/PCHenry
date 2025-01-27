namespace MyClasses;

public class Supervisor : Person
{
  public Supervisor()
  {
    Employees = new();
  }

  public List<Employee> Employees { get; set; }
}
