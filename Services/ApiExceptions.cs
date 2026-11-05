namespace aspnet_api_scheduling.Services;

public sealed class ResourceNotFoundException(string message) : Exception(message);

public sealed class BusinessConflictException(string message) : Exception(message);

public sealed class BusinessRuleException(string message) : Exception(message);
