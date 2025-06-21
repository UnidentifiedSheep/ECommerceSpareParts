using Core.Exceptions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Core.HttpCommunication.Exception
{
	internal class ServiceNotFoundException : NotFoundException
	{
		internal ServiceNotFoundException(object key) : base("Service not found", key)
		{
		}
	}
}
