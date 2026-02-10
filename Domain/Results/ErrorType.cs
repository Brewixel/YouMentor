using System;
using System.Collections.Generic;
using System.Text;

namespace Domain.Results;

public enum ErrorType
{
	Validation,
	NotFound,
	Conflict,
	Failure,
}
