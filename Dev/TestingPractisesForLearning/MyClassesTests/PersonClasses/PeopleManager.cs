namespace MyClasses;

public class PeopleManager
{
  #region GetPerson Method
  public Person? GetPerson(int id)
  {
    // Simulate looking up a person by their ID
    // and not finding them
    return null;
  }
  #endregion

  #region GetPeople Method
  public List<Person> GetPeople()
  {
    return new()
    {
      new () { FirstName = "Paul", LastName = "Sheriff", Age = 59 },
      new () { FirstName = "John", LastName = "Kuhn", Age = 55 },
      new () { FirstName = "Steve", LastName = "Nystrom", Age = 65 }
    };
  }
  #endregion

  #region GetSupervisors Method
  public List<Person> GetSupervisors()
  {
    return new()
    {
      new Supervisor() { FirstName = "Paul", LastName = "Sheriff", Age = 59 },
      new Supervisor() { FirstName = "Michael", LastName = "Krasowski", Age = 65 }
    };
  }
  #endregion

  #region GetEmployees Method
  public List<Person> GetEmployees()
  {
    return new()
    {
      new Employee() { FirstName = "Paul", LastName = "Sheriff", Age = 59 },
      new Employee() { FirstName = "John", LastName = "Kuhn", Age = 55 },
      new Employee() { FirstName = "Steve", LastName = "Nystrom", Age = 48 }
    };
  }
  #endregion

  #region GetSupervisorsAndEmployees Method
  public List<Person> GetSupervisorsAndEmployees()
  {
    List<Person> people = new();

    people.AddRange(GetEmployees());
    people.AddRange(GetSupervisors());

    return people;
  }
  #endregion
}
