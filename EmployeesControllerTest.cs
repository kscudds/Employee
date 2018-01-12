using Company2.Controllers;
using Company2.Data;
using Company2.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Xunit;

namespace Company2.Test
{
    public class EmployeesControllerTest
    {
        private readonly EmployeeContext _context;
        private readonly IServiceProvider serviceProvider;

        public EmployeesControllerTest()
        {
            var services = new ServiceCollection();
            services.AddEntityFrameworkSqlServer().AddDbContext<EmployeeContext>(options =>
                options.UseSqlServer("Server=(localdb)\\mssqllocaldb;Database=EmployeeTest;Trusted_Connection=True;MultipleActiveResultSets=true"));

            serviceProvider = services.BuildServiceProvider();

        }
        private void CreateTestData(EmployeeContext dbContext)
        {
            dbContext.Database.EnsureCreated();

            if (!dbContext.Employees.Any())
            {
                var employee = new Employee[]
                {
                    new Employee {LastName="Washington", FirstName="George"},
                    new Employee {LastName="Adams", FirstName="John"},
                    new Employee {LastName="Jefferson", FirstName="Thomas"},
                    new Employee {LastName="Madison", FirstName="James"},
                    new Employee {LastName="Monroe", FirstName="James"}
                };
                dbContext.Employees.AddRange(employee);
                dbContext.SaveChanges();
            }

        }
        [Fact]
        public async Task ShouldReturnViewWithListOfEmployeesAsync()
        {
            //Arrange
            var dbContext = serviceProvider.GetRequiredService<EmployeeContext>();
            CreateTestData(dbContext);
            var controller = new EmployeesController(dbContext);

            //Act
            var result = await controller.Index();

            //Assert
            var view = Assert.IsType<ViewResult>(result);
            var model = Assert.IsAssignableFrom<IEnumerable<Employee>>(view.ViewData.Model);
            Assert.Equal(5, model.Count());

        }
        [Fact]
        public async void ShouldReturnBadRequest()
        {
            //Arrange
            var dbContext = serviceProvider.GetRequiredService<EmployeeContext>();
            CreateTestData(dbContext);
            var controller = new EmployeesController(dbContext);

            //Act
            var actionRequest = await controller.Details(null);

            //Assert
            Assert.IsType<BadRequestResult>(actionRequest);

        }
        [Fact]
        public async void ShouldReturnNotFound()
        {
            //Arrange
            var dbContext = serviceProvider.GetRequiredService<EmployeeContext>();
            CreateTestData(dbContext);
            var controller = new EmployeesController(dbContext);

            //Act
            var actionRequest = await controller.Details(6);

            //Assert
            Assert.IsType<NotFoundResult>(actionRequest);

        }
        [Fact]
        public async void ShouldReturnCorrectEmployee()
        {
            //Arrange
            var dbContext = serviceProvider.GetRequiredService<EmployeeContext>();
            CreateTestData(dbContext);
            var controller = new EmployeesController(dbContext);

            //Act
            var result = await controller.Details(1);

            //Assert
            var view = Assert.IsType<ViewResult>(result);
            var model = Assert.IsType<Employee>(view.ViewData.Model);
            Assert.Equal(1, model.ID);

        }

        //Create Tests
        [Fact]
        public async Task ShouldReturnAListWithCreatedEmployee()
        {
            //Arrange
            var dbContext = serviceProvider.GetRequiredService<EmployeeContext>();
            CreateTestData(dbContext);
            var controller = new EmployeesController(dbContext);
            Employee e = new Employee { LastName = "Adams", FirstName = "JohnQuincy" };

            //Act
            var result = await controller.Create(e);

            //Assert
            var actionResult = Assert.IsType<RedirectToActionResult>(result);
            var view = Assert.IsType<ViewResult>(await controller.Index());
            var model = Assert.IsAssignableFrom<IEnumerable<Employee>>(view.ViewData.Model);

            Assert.Equal(6, model.Count());

        }
        [Fact]
        public async Task ShouldReturnViewFromInvalidModelState()
        {
            //Arrange
            var dbContext = serviceProvider.GetRequiredService<EmployeeContext>();
            CreateTestData(dbContext);
            var controller = new EmployeesController(dbContext);
            controller.ModelState.AddModelError("error", "error");

            //Act
            var result = await controller.Create(null);

            //Assert
            Assert.IsType<ViewResult>(result);

        }
        //UPDATES
        [Fact]
        public async void ShouldReturnNotFoundWhenUpdatingAnIdNotInDB()
        {
            //Arrange
            var dbContext = serviceProvider.GetRequiredService<EmployeeContext>();
            CreateTestData(dbContext);
            var controller = new EmployeesController(dbContext);

            //Act
            var editResult = await controller.Edit(100);

            //Assert
            Assert.IsType<NotFoundResult>(editResult);
        }
        [Fact]
        public async Task ShouldReturnUpdatedEmployee()
        {
            //Arrange
            var dbContext = serviceProvider.GetRequiredService<EmployeeContext>();
            CreateTestData(dbContext);
            var controller = new EmployeesController(dbContext);

            var detailResult = await controller.Details(5);
            var detailView = Assert.IsType<ViewResult>(detailResult);
            var model = Assert.IsType<Employee>(detailView.ViewData.Model);
            model.LastName = "A";

            //Act
            var editResult = await controller.Edit(5);
            var editView = Assert.IsType<ViewResult>(editResult);
            var editModel = Assert.IsType<Employee>(editView.ViewData.Model);

            //Assert
            Assert.Equal("A", editModel.LastName);
        }
        //DELETE
        [Fact]
        public async void ShouldReturnViewOfItemToDelete()
        {
            //Arrange
            var dbContext = serviceProvider.GetRequiredService<EmployeeContext>();
            CreateTestData(dbContext);
            var controller = new EmployeesController(dbContext);

            //Act
            var deleteResult = await controller.Delete(6);

            //Assert
            Assert.IsType<ViewResult>(deleteResult);
        }
        [Fact]
        public async void ShouldReturnNotFoundWhenDeletingAnIdNotInDB()
        {
            //Arrange
            var dbContext = serviceProvider.GetRequiredService<EmployeeContext>();
            CreateTestData(dbContext);
            var controller = new EmployeesController(dbContext);

            //Act
            var deleteResult = await controller.Delete(100);

            //Assert
            Assert.IsType<NotFoundResult>(deleteResult);
        }
        [Fact]
        public async void ShouldDeleteConfirmedItem()
        {
            //Arrange
            var dbContext = serviceProvider.GetRequiredService<EmployeeContext>();
            CreateTestData(dbContext);
            var controller = new EmployeesController(dbContext);

            //Act
            var deleteResult = await controller.DeleteConfirmed(6);

            //Assert
            Assert.IsType<RedirectToActionResult>(deleteResult);

            var indexResult = await controller.Index();

            var indexView = Assert.IsType<ViewResult>(indexResult);
            var model = Assert.IsAssignableFrom<IEnumerable<Employee>>(indexView.ViewData.Model);
            Assert.Equal(5, model.Count());

        }

    }
}