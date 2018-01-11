using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Company2.Models;

namespace Company2.Data
{
    public class DbInitalizer
    {
        public static void Initialize(EmployeeContext context)
        {
            context.Database.EnsureCreated();

            if (context.Employees.Any())
            {
                return;
            }

            var employee = new Employee[]
            {
                new Employee {LastName="Washington", FirstName="George"},
                new Employee {LastName="Adams", FirstName="John"}
            };

            foreach (Employee s in employee)
            {
                context.Employees.Add(s);
            }
            context.SaveChanges();

        }
    }
}
