using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Text;

namespace Application.Tests;

public class FakeLogger<T> : ILogger<T>
{
	public List<string> Messages { get; } = new();

	public void Log<TState>(LogLevel logLevel, EventId eventId, TState state, Exception? exception, Func<TState, Exception?, string> formatter)
	{
		Messages.Add(formatter(state, exception));
	}

	public bool IsEnabled(LogLevel logLevel) => true;
	public IDisposable? BeginScope<TState>(TState state) where TState : notnull => null;
}
