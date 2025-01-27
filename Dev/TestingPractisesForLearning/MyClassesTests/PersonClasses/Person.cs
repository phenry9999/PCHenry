namespace MyClasses;

public class Person
{
  public Person()
  {
    FirstName = string.Empty;
    LastName = string.Empty;
  }

  public string FirstName { get; set; }
  public string LastName { get; set; }
  public int Age { get; set; }
}
