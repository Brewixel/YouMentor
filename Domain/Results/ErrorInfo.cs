using System;
using System.Collections.Generic;
using System.Text;

namespace Domain.Results;

public record ErrorInfo(ErrorType Type, string Message);
