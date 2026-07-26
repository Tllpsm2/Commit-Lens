namespace CommitLens.Application.Reports.GetActivityHeatMap;

public sealed class HeatMapGrid
{
    private readonly int[,] _cells = new int[7, 24];

    public int this[DayOfWeek day, int hour]
    {
        get
        {
            if (hour < 0 || hour > 23)
                throw new ArgumentOutOfRangeException(nameof(hour));
            return _cells[(int)day, hour];
        }
    }

    public int Total { get; private set; }
    public int Max { get; private set; }

    public void Increment(DayOfWeek day, int hour)
    {
        if (hour < 0 || hour > 23)
            throw new ArgumentOutOfRangeException(nameof(hour));

        _cells[(int)day, hour]++;
        Total++;
        if (_cells[(int)day, hour] > Max)
            Max = _cells[(int)day, hour];
    }

    public void Merge(HeatMapGrid other)
    {
        for (var d = 0; d < 7; d++)
        for (var h = 0; h < 24; h++)
        {
            var value = other._cells[d, h];
            if (value == 0)
                continue;

            _cells[d, h] += value;
            Total += value;
            if (_cells[d, h] > Max)
                Max = _cells[d, h];
        }
    }

    public (DayOfWeek Day, int Hour, int Count) Peak()
    {
        var maxDay = 0;
        var maxHour = 0;
        var maxValue = 0;

        for (var d = 0; d < 7; d++)
        for (var h = 0; h < 24; h++)
        {
            if (_cells[d, h] > maxValue)
            {
                maxValue = _cells[d, h];
                maxDay = d;
                maxHour = h;
            }
        }

        return ((DayOfWeek)maxDay, maxHour, maxValue);
    }
}

public record ActivityHeatMapResponse(
    HeatMapPeriod Period,
    string PeriodLabel,
    DateTimeOffset From,
    DateTimeOffset To,
    int TotalCommits,
    HeatMapGrid Aggregate,
    IReadOnlyList<DailyCommitCount> DailyCounts,
    IReadOnlyList<RepositoryHeatMapDto> Repositories
);

public record RepositoryHeatMapDto(
    string RepositoryName,
    int CommitCount,
    HeatMapGrid HeatMap,
    IReadOnlyList<DailyCommitCount> DailyCounts
);

public record DailyCommitCount(DateOnly Date, int Count);