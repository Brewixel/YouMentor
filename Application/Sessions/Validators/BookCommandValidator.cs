using FluentValidation;
using System;
using System.Collections.Generic;
using System.Text;

namespace Application.Sessions.Validators;

public class BookCommandValidator : AbstractValidator<Book.Command>
{
	public BookCommandValidator()
	{
		RuleFor(x => x.SessionId)
			.NotEmpty();

		RuleFor(x => x.StudentId)
			.NotEmpty();
	}
}
