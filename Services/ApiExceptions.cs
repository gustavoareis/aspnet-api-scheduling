namespace aspnet_api_scheduling.Services;

public sealed class ResourceNotFoundException(string message) : Exception(message);

public sealed class BusinessConflictException(string message) : Exception(message);

public sealed class BusinessRuleException(string message) : Exception(message);

public sealed class BootstrapUnavailableException()
    : Exception("O provisionamento inicial não está configurado.");

public sealed class InvalidProvisioningTokenException()
    : Exception("Chave de provisionamento inválida.");

public sealed class IdentityOperationException(IEnumerable<string> errors)
    : Exception(string.Join(" ", errors));
