using Predictly.Application.DTOs.Admin;

namespace Predictly.Application.Interfaces;

public interface IExcelParser
{
    ParsedUpload Parse(Stream stream);
}
