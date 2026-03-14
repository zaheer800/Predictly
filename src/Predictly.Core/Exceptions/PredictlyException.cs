namespace Predictly.Core.Exceptions;

public class PredictlyException(string message) : Exception(message);

public class NotFoundException(string entity, object id)
    : PredictlyException($"{entity} '{id}' was not found.");

public class PredictionLockedException(Guid matchId)
    : PredictlyException($"Match '{matchId}' has started. Predictions are locked.");

public class InvalidOperationException(string message) : PredictlyException(message);
