using System;
using System.Collections.Generic;

namespace App.Common.Exceptions
{
	public class ValidationException : HttpException
	{
		public IDictionary<string, string[]> Errors { get; }

		public ValidationException(IDictionary<string, string[]> errors, string? message = null)
			: base(400, message ?? "One or more validation errors occurred.")
		{
			Errors = errors;
		}
	}
}
