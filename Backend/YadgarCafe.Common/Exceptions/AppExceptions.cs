namespace YadgarCafe.Common.Exceptions
{
    /// <summary>Base type for expected, client-facing failures. Mapped to HTTP codes by the API.</summary>
    public abstract class AppException : Exception
    {
        protected AppException(string message) : base(message) { }
    }

    /// <summary>400 - the request is malformed or violates an input rule.</summary>
    public class BadRequestException : AppException
    {
        public BadRequestException(string message) : base(message) { }
    }

    /// <summary>401 - missing or invalid credentials.</summary>
    public class UnauthorizedException : AppException
    {
        public UnauthorizedException(string message) : base(message) { }
    }

    /// <summary>403 - authenticated, but not allowed to touch this resource.</summary>
    public class ForbiddenException : AppException
    {
        public ForbiddenException(string message = "You do not have access to this resource.") : base(message) { }
    }

    /// <summary>404 - the resource does not exist.</summary>
    public class NotFoundException : AppException
    {
        public NotFoundException(string message) : base(message) { }

        public NotFoundException(string entity, object key) : base($"{entity} with ID '{key}' was not found.") { }
    }

    /// <summary>409 - the request conflicts with the current state (duplicates, insufficient stock, bad transition).</summary>
    public class ConflictException : AppException
    {
        public ConflictException(string message) : base(message) { }
    }
}
