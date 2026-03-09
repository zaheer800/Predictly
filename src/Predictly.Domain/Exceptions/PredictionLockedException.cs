namespace Predictly.Domain.Exceptions;

public sealed class PredictionLockedException : DomainException
{
    public PredictionLockedException(int matchId)
        : base($"Predictions are locked for match {matchId}. The match has already started.")
    {
    }
}
