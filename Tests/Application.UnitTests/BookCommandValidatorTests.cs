using Application.Sessions;
using Application.Sessions.Validators;
using FluentValidation.TestHelper;

namespace Application.UnitTests;

public class BookCommandValidatorTests
{
	private readonly BookCommandValidator _validator = new();

	[Fact]
	public async Task Should_HaveError_When_SessionIdIsEmpty()
	{
		// Arrange
		var command = new Book.Command()
		{
			SessionId = Guid.Empty
		};

		// Act
		var result = await _validator.TestValidateAsync(command);

		// Assert
		result.ShouldHaveValidationErrorFor(x => x.SessionId);
	}

	[Fact]
	public async Task Should_NotHaveError_When_CommandIsValid()
	{
		// Arrange
		var command = new Book.Command()
		{
			SessionId = Guid.NewGuid()
		};

		// Act
		var result = await _validator.TestValidateAsync(command);

		// Assert
		result.ShouldNotHaveAnyValidationErrors();
	}
}
