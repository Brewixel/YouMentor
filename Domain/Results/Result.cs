using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Domain.Results;

public class Result()
{
	public required bool IsSuccess { get; init; }
	public ErrorInfo? ErrorInfo { get; init; }

	public static Result Success()
		=> new() { IsSuccess = true };

	public static Result Validation(string message)
		=> new()
		{
			IsSuccess = false,
			ErrorInfo = new ErrorInfo(ErrorType.Validation, message)
		};

	public static Result NotFound(string message)
		=> new()
		{
			IsSuccess = false,
			ErrorInfo = new ErrorInfo(ErrorType.NotFound, message)
		};

	public static Result Conflict(string message)
		=> new()
		{
			IsSuccess = false,
			ErrorInfo = new ErrorInfo(ErrorType.Conflict, message)
		};

	public static Result Failure(string message)
		=> new()
		{
			IsSuccess = false,
			ErrorInfo = new ErrorInfo(ErrorType.Failure, message)
		};
}

public class Result<T> : Result
{
	public T? Value { get; init; }

	public static Result<T> Success(T value)
		=> new()
		{
			IsSuccess = true,
			Value = value
		};

	public static new Result<T> Validation(string message)
		=> new()
		{
			IsSuccess = false,
			ErrorInfo = new ErrorInfo(ErrorType.Validation, message)
		};

	public static new Result<T> NotFound(string message)
		=> new()
		{
			IsSuccess = false,
			ErrorInfo = new ErrorInfo(ErrorType.NotFound, message)
		};

	public static new Result<T> Conflict(string message)
		=> new()
		{
			IsSuccess = false,
			ErrorInfo = new ErrorInfo(ErrorType.Conflict, message)
		};

	public static new Result<T> Failure(string message)
		=> new()
		{
			IsSuccess = false,
			ErrorInfo = new ErrorInfo(ErrorType.Failure, message)
		};
}
