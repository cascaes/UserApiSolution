using Xunit;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Collections.Generic;
using System.Threading.Tasks;
using FluentAssertions;
using System.Runtime.ConstrainedExecution;

public class UsersControllerTests
{
    private readonly UsersController _controller;
    private readonly UserContext _context;

    public UsersControllerTests()
    {
        // Configura um novo banco de dados em memória para cada teste
        var options = new DbContextOptionsBuilder<UserContext>()
            .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
            .Options;

        _context = new UserContext(options);
        _controller = new UsersController(_context);
    }

    [Fact(DisplayName = "DeveRetornarNotFound_QuandoUsuarioNaoExistir")]
    public async Task GetUser_ReturnsNotFound_WhenUserDoesNotExist()
    {
        // Act
        var result = await _controller.GetUser(1);
        // Assert
        var notFoundResult = Assert.IsType<NotFoundResult>(result.Result);
    }

    [Fact(DisplayName = "DeveCriarUsuario_ERetornarCreatedAtAction")]
    public async Task PostUser_CreatesUser_ReturnsCreatedAtAction()
    {
        // Arrange
        var newUser = new User { Id = 2, Name = "Jane Doe", Email = "jane@example.com", Phone = "987654321" };

        // Act
        var result = await _controller.PostUser(newUser);

        // Assert
        var createdAtActionResult = result.Result as CreatedAtActionResult;
        createdAtActionResult.Should().NotBeNull();
        createdAtActionResult.StatusCode.Should().Be(201);
        createdAtActionResult.Value.Should().Be(newUser);
    }

    [Fact(DisplayName = "RetornaBadRequest_QuandoIdsNaoCorrespondem")]
    public async Task PutUser_ReturnsBadRequest_WhenIdsDoNotMatch()
    {
        // Arrange
        var user = new User { Id = 1, Name = "John Doe", Email = "john@example.com", Phone = "123456789" };

        // Act
        var result = await _controller.PutUser(2, user); // IDs não correspondem

        // Assert
        Assert.IsType<BadRequestResult>(result);
    }

    [Fact(DisplayName = "RetornaNoContent_QuandoUsuarioDeletado")]
    public async Task DeleteUser_ReturnsNoContent_WhenUserDeleted()
    {
        // Arrange
        var user = new User { Id = 1, Name = "John Doe", Email = "john@example.com", Phone = "123456789" };
        _context.Users.Add(user);
        await _context.SaveChangesAsync();

        // Act
        var result = await _controller.DeleteUser(1);

        // Assert
        Assert.IsType<NoContentResult>(result);
    }

    [Fact(DisplayName = "RetornaBadRequest_QuandoIdForZero")]
    public async Task PostUser_ReturnsBadRequest_WhenIdIsZero()
    {
        // Arrange
        var user = new User { Id = 0, Name = "John Doe", Email = "john@example.com", Phone = "123456789" };

        // Act
        var result = await _controller.PostUser(user);

        // Assert
        var badRequestResult = result.Result as BadRequestObjectResult;
        badRequestResult.Should().NotBeNull();
        badRequestResult.StatusCode.Should().Be(400);
        badRequestResult.Value.Should().Be("User ID must not be zero.");
    }

    [Fact(DisplayName = "RetornaOkResult_ComListaDeUsuarios")]
    public async Task GetUsers_ReturnsOkResult_WithListOfUsers()
    {
      
     var users = new List<User>
    {
        new() { Id = 1, Name = "John Doe", Email = "john@example.com", Phone = "123456789" },
        new() { Id = 2, Name = "Jane Doe", Email = "jane@example.com", Phone = "987654321" }

    };
        _context.Users.AddRange(users);
        await _context.SaveChangesAsync();

        // Confirma que os usuários foram salvos
        var count = await _context.Users.CountAsync();
        Console.WriteLine($"Usuários salvos no banco: {count}");

        // Act
        ActionResult<IEnumerable<User>> result = null;

        try
        {
            result = await _controller.GetUsers();
            Console.WriteLine($"Tipo do resultado retornado: {result?.Result?.GetType().Name}");
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Erro ao chamar GetUsers(): {ex}");
        }

        // Assert
        result.Should().NotBeNull("O resultado não deveria ser nulo.");
        result.Result.Should().BeOfType<OkObjectResult>("Esperamos um OkObjectResult");

        var okResult = result.Result as OkObjectResult;
        okResult.StatusCode.Should().Be(200);
        okResult.Value.Should().BeEquivalentTo(users);
    }

}