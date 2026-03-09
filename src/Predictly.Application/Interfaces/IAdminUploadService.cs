using Predictly.Application.DTOs.Admin;

namespace Predictly.Application.Interfaces;

public interface IAdminUploadService
{
    Task<UploadSummaryResponse> ProcessUploadAsync(Stream fileStream, int uploadedBy, CancellationToken ct = default);
}
