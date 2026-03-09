using ClosedXML.Excel;
using Predictly.Application.DTOs.Admin;
using Predictly.Application.Interfaces;

namespace Predictly.Infrastructure.Services;

/// <summary>
/// Parses the admin Excel upload (.xlsx) with two sheets:
///   - MatchSetup:   TournamentId | TeamHome | TeamAway | Venue | MatchStartTime
///   - MatchResults: MatchId | Winner | SelectionId1 | Numeric1 | Choice1 | SelectionId2 | Numeric2 | Choice2 | SelectionId3 | Numeric3 | Choice3
/// </summary>
public class ExcelParser : IExcelParser
{
    public ParsedUpload Parse(Stream stream)
    {
        using var workbook = new XLWorkbook(stream);

        var matchSetups   = ParseMatchSetupSheet(workbook, out var setupErrors);
        var matchResults  = ParseMatchResultsSheet(workbook, out var resultErrors);
        var allErrors     = setupErrors.Concat(resultErrors).ToList();

        return new ParsedUpload(matchSetups, matchResults, allErrors);
    }

    private static IReadOnlyList<ParsedMatchSetupRow> ParseMatchSetupSheet(
        XLWorkbook workbook, out List<string> errors)
    {
        errors = [];
        var rows = new List<ParsedMatchSetupRow>();

        if (!workbook.TryGetWorksheet("MatchSetup", out var sheet))
        {
            errors.Add("Sheet 'MatchSetup' not found.");
            return rows;
        }

        foreach (var row in sheet.RowsUsed().Skip(1))
        {
            var rowNum = row.RowNumber();
            try
            {
                var tournamentId    = row.Cell(1).GetValue<int>();
                var teamHome        = row.Cell(2).GetString().Trim();
                var teamAway        = row.Cell(3).GetString().Trim();
                var venue           = row.Cell(4).GetString().Trim();
                var matchStartRaw   = row.Cell(5).GetString().Trim();

                if (string.IsNullOrWhiteSpace(teamHome) || string.IsNullOrWhiteSpace(teamAway))
                {
                    errors.Add($"MatchSetup row {rowNum}: TeamHome and TeamAway are required.");
                    continue;
                }

                if (!DateTime.TryParse(matchStartRaw, out var matchStartTime))
                {
                    errors.Add($"MatchSetup row {rowNum}: Invalid MatchStartTime '{matchStartRaw}'. Expected yyyy-MM-dd HH:mm.");
                    continue;
                }

                rows.Add(new ParsedMatchSetupRow(
                    tournamentId,
                    teamHome,
                    teamAway,
                    string.IsNullOrWhiteSpace(venue) ? null : venue,
                    DateTime.SpecifyKind(matchStartTime, DateTimeKind.Utc)));
            }
            catch (Exception ex)
            {
                errors.Add($"MatchSetup row {rowNum}: Failed to parse — {ex.Message}");
            }
        }

        return rows;
    }

    private static IReadOnlyList<ParsedMatchResultRow> ParseMatchResultsSheet(
        XLWorkbook workbook, out List<string> errors)
    {
        errors = [];
        var rows = new List<ParsedMatchResultRow>();

        if (!workbook.TryGetWorksheet("MatchResults", out var sheet))
        {
            errors.Add("Sheet 'MatchResults' not found.");
            return rows;
        }

        foreach (var row in sheet.RowsUsed().Skip(1))
        {
            var rowNum = row.RowNumber();
            try
            {
                var matchId = row.Cell(1).GetValue<int>();
                var winner  = row.Cell(2).GetString().Trim();

                if (string.IsNullOrWhiteSpace(winner))
                {
                    errors.Add($"MatchResults row {rowNum}: Winner is required.");
                    continue;
                }

                var bonusResults = new List<ParsedBonusResult>();
                for (int slot = 0; slot < 3; slot++)
                {
                    var col         = 3 + slot * 3;
                    var selIdRaw    = row.Cell(col).GetString().Trim();
                    var numericRaw  = row.Cell(col + 1).GetString().Trim();
                    var choiceRaw   = row.Cell(col + 2).GetString().Trim();

                    if (string.IsNullOrWhiteSpace(selIdRaw)) continue;

                    if (!int.TryParse(selIdRaw, out var selectionId))
                    {
                        errors.Add($"MatchResults row {rowNum}: Invalid SelectionId '{selIdRaw}' in slot {slot + 1}.");
                        continue;
                    }

                    decimal? actualNumeric = null;
                    if (!string.IsNullOrWhiteSpace(numericRaw) &&
                        decimal.TryParse(numericRaw, out var parsed))
                        actualNumeric = parsed;

                    bonusResults.Add(new ParsedBonusResult(
                        selectionId,
                        actualNumeric,
                        string.IsNullOrWhiteSpace(choiceRaw) ? null : choiceRaw));
                }

                rows.Add(new ParsedMatchResultRow(matchId, winner, bonusResults));
            }
            catch (Exception ex)
            {
                errors.Add($"MatchResults row {rowNum}: Failed to parse — {ex.Message}");
            }
        }

        return rows;
    }
}
