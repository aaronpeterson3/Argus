namespace Argus.Infrastructure.Common
{
    public class NotFoundException : Exception
    {
        public NotFoundException(string message) : base(message)
        {
        }

        public NotFoundException(string message, Exception innerException) 
            : base(message, innerException)
        {
        }

        public NotFoundException(string entityName, object id)
            : base($"Entity '{entityName}' with ID {id} was not found.")
        {
        }
    }
}