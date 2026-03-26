using SV22T1020761.DataLayers.SQLServer.HR;
using SV22T1020761.Models.HR;
using SV22T1020761.Models.Common;
using System.Threading.Tasks;
using System.Collections.Generic;

namespace SV22T1020761.BusinessLayers
{
    /// <summary>
    /// Service cung c?p d? li?u cho domain HR (Employees)
    /// </summary>
    public static class HRDataService
    {
        private static EmployeeRepository _employeeRepo;

        static HRDataService()
        {
            _employeeRepo = new EmployeeRepository(Configuration.ConnectionString);
        }

        public static PagedResult<Employee> ListEmployees(PaginationSearchInput input)
            => _employeeRepo.ListAsync(input).GetAwaiter().GetResult();

        public static Employee? GetEmployee(int id)
            => _employeeRepo.GetAsync(id).GetAwaiter().GetResult();

        public static void AddEmployee(Employee employee)
        {
            _employeeRepo.AddAsync(employee).GetAwaiter().GetResult();
        }

        public static void UpdateEmployee(Employee employee)
        {
            _employeeRepo.UpdateAsync(employee).GetAwaiter().GetResult();
        }

        public static void DeleteEmployee(int employeeId)
        {
            _employeeRepo.DeleteAsync(employeeId).GetAwaiter().GetResult();
        }

        // Async wrappers
        public static Task<Employee?> GetEmployeeAsync(int id) => _employeeRepo.GetAsync(id);

        public static Task<bool> ValidateEmployeeEmailAsync(string email, int id = 0) => _employeeRepo.ValidateEmailAsync(email, id);

        public static Task<int> AddEmployeeAsync(Employee employee) => _employeeRepo.AddAsync(employee);

        public static Task<bool> UpdateEmployeeAsync(Employee employee) => _employeeRepo.UpdateAsync(employee);
    }
}
