using Application.Sessions;
using Application.Sessions.Validators;
using FluentValidation.TestHelper;

namespace Application.UnitTests;

public class BookCommandValidatorTests
{
	private readonly BookCommandValidator _validator;

	public BookCommandValidatorTests()
	{
		_validator = new BookCommandValidator();
	}

	[Fact]
	public async Task Should_HaveError_When_SessionIdIsEmpty()
	{
		// Arrange
		var command = new Book.Command()
		{
			SessionId = Guid.Empty,
			StudentId = Guid.NewGuid()
		};

		// Act
		var result = await _validator.TestValidateAsync(command);

		// Assert
		result.ShouldHaveValidationErrorFor(x => x.SessionId);
	}

	[Fact]
	public async Task Should_HaveError_When_StudentIdIsEmpty()
	{
		// Arrange
		var command = new Book.Command()
		{
			SessionId = Guid.NewGuid(),
			StudentId = Guid.Empty
		};

		// Act
		var result = await _validator.TestValidateAsync(command);

		// Assert
		result.ShouldHaveValidationErrorFor(x => x.StudentId);
	}

	[Fact]
	public async Task Should_NotHaveError_When_CommandIsValid()
	{
		// Arrange
		var command = new Book.Command()
		{
			SessionId = Guid.NewGuid(),
			StudentId = Guid.NewGuid(),
		};

		// Act
		var result = await _validator.TestValidateAsync(command);

		// Assert
		result.ShouldNotHaveAnyValidationErrors();
	}
}
