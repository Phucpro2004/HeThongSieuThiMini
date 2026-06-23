using System;

namespace MiniShop.Exceptions
{
    public class BadRequestException : AppException
    {
        public BadRequestException(string message) : base(message)
        {
        }
    }
}
