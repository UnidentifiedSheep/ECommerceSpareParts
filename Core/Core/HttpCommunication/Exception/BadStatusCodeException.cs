using Core.Exceptions;
using Microsoft.AspNetCore.Server.IIS;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Core.HttpCommunication.Exception
{
	internal class BadStatusCodeException : BadRequestException
	{
		internal BadStatusCodeException(string statusCode) : base("Bad Status CODE!", statusCode)
		{
		}
	}
}
