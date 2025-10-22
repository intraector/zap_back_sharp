using System;

namespace App.Common.Exceptions
{
	public class NotFoundException : HttpException
	{
		public NotFoundException(string message) : base(404, message) { }
	}
}
