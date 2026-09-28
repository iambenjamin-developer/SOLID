namespace SolidShop.Api.Common.Exceptions;

// Translated to HTTP status codes by GlobalExceptionHandler.
// Services throw these instead of returning IActionResult (they don't know about HTTP).

public class NotFoundException(string message) : Exception(message);        // 404

public class ConflictException(string message) : Exception(message);        // 409

public class BusinessRuleException(string message) : Exception(message);    // 400
