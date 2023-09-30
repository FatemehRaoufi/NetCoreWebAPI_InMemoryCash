using System.Collections.Generic;

namespace NetCoreWebAPI_InMemoryCash.Model
{
    public interface IDataRepositoryEmployee
    {
        List<Employee> AddEmployee(Employee employee);
        List<Employee> GetEmployees();
        Employee PutEmployee(Employee employee);
        Employee GetEmployeeById(string id);
    }
}
