using Application.Behaviors;
using Domain.Results;
using FluentAssertions;
using FluentValidation;
using FluentValidation.Results;
using MediatR;
using Moq;

namespace Application.UnitTests;

public class ValidationBehaviorTests
{
	[Fact]
	public async Task Handle_Should_ReturnSuccessAndCallNext_When_NoValidationErrors()
	{
		// Arrange
		var firstValidator = SetupValidator();
		var secondValidator = SetupValidator();
		var behavior = new ValidationBehavior<IRequest<Result>, Result>([firstValidator.Object, secondValidator.Object]);

		var command = new TestCommand();

		var nextCalled = false;
		RequestHandlerDelegate<Result> next = async (ct) =>
		{
			nextCalled = true;
			return Result.Success();
		};

		// Act
		var result = await behavior.Handle(command, next, CancellationToken.None);

		// Assert
		result.IsSuccess.Should().BeTrue(result.ErrorInfo?.Message);
		nextCalled.Should().BeTrue();
	}

	[Fact]
	public async Task Handle_Should_ReturnValidationAndDontCallNext_On_GetValidationErrors()
	{
		// Arrange
		var firstValidatorErrors = new List<string> { "Email is incorrect", "Bad GUID" };
		var secondValidatorErrors = new List<string> { "Damn input data" };
		var firstValidator = SetupValidator(firstValidatorErrors);
		var secondValidator = SetupValidator(secondValidatorErrors);
		var behavior = new ValidationBehavior<IRequest<Result>, Result>([firstValidator.Object, secondValidator.Object]);

		var command = new TestCommand();

		var nextCalled = false;
		RequestHandlerDelegate<Result> next = async (ct) =>
		{
			nextCalled = true;
			return Result.Success();
		};

		// Act
		var result = await behavior.Handle(command, next, CancellationToken.None);

		// Assert
		result.IsSuccess.Should().BeFalse();
		var expectedErrorMessage = string.Join(Environment.NewLine, firstValidatorErrors.Union(secondValidatorErrors));
		result.ErrorInfo!.Type.Should().Be(ErrorType.Validation);
		result.ErrorInfo!.Message.Should().Be(expectedErrorMessage);
		nextCalled.Should().BeFalse();
	}

	private Mock<IValidator<IRequest<Result>>> SetupValidator(params IEnumerable<string> errors)
	{
		var result = new ValidationResult
		{
			Errors = errors.Select(error => new ValidationFailure { ErrorMessage = error })
				.ToList()
		};

		var validator = new Mock<IValidator<IRequest<Result>>>();
		validator
			.Setup(x => x.ValidateAsync(It.IsAny<IRequest<Result>>(), It.IsAny<CancellationToken>()))
			.ReturnsAsync(result);
		return validator;
	}

	record TestCommand : IRequest<Result>;
}
