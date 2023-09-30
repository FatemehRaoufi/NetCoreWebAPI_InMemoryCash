using NetCoreWebAPI_InMemoryCash.Models;

namespace NetCoreWebAPI_InMemoryCash.Model
{
    public class DataRepositoryEmployee : IDataRepositoryEmployee
    {

        private readonly List<Employee> _employees;

        public DataRepositoryEmployee()
        {
            _employees = new List<Employee>(){
                new Employee()
                {
                    Id = 101,
                    Name = "George Harrison",
                    PhoneNo= "11111111111",
                    EmailId = "george@gmail.com"
                } ,
              new Employee()
                {
                    Id = 102,
                    Name = "Amanda Smith",
                    PhoneNo= "22222222222",
                    EmailId = "amanda@gmail.com"
                } ,
              new Employee()
                {
                    Id = 103,
                    Name = "Amanda Smith",
                    PhoneNo= "22222222222",
                    EmailId = "amanda@gmail.com"
                }
            };
        }
        public Employee Add(Employee employee)
        {
            int newId = _employees.Max(x => x.Id) + 1;
            employee.Id = newId;
            _employees.Add(employee);
            return employee;
        }

        public bool Delete(int id)
        {
            var existing = _employees.Find(x => x.Id == id);
            if (existing != null)
            {
                _employees.Remove(existing);
                return true;
            }
            return false;
        }
        public List<Employee> GetAll()
        {
            return _employees;
        }

        public Employee GetById(int id)
        {
            return _employees.Where(x => x.Id == id).FirstOrDefault();
        }
    }
