using SV22T1020761.DataLayers.SQLServer.HR;
using SV22T1020761.Models.HR;
using SV22T1020761.Models.Common;

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
            // Implementation for adding an employee
            throw new NotImplementedException();
        }

        public static void UpdateEmployee(Employee employee)
        {
            // Implementation for updating an employee
            throw new NotImplementedException();
        }

        public static void DeleteEmployee(int employeeId)
        {
            // Implementation for deleting an employee
            throw new NotImplementedException();
        }
    }
}
