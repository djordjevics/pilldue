namespace Pilldue.Business;

/// <summary>A tracked medication and its stock / prescription settings.</summary>
public sealed class Medication
{
    public Guid Id { get; set; } = Guid.NewGuid();

    public string Name { get; set; } = string.Empty;

    /// <summary>Pills in one package (e.g. 12, 28, 30, 60).</summary>
    public int PackageSizePills { get; set; }

    /// <summary>Usual number of packages obtained each refill.</summary>
    public int PrescribedPackageCount { get; set; }

    /// <summary>Pills consumed on each dose day.</summary>
    public int DailyDosagePills { get; set; }

    /// <summary>
    /// Days between doses: <c>1</c> daily, <c>2</c> every other day, <c>7</c> weekly from
    /// <see cref="PrescriptionStartDate"/> (not a fixed weekday).
    /// </summary>
    public int DoseIntervalDays { get; set; } = 1;

    /// <summary>Pills currently on hand.</summary>
    public int CurrentStockPills { get; set; }

    /// <summary>
    /// Optional legacy field (ignored). Refill day-of-month is
    /// <see cref="PrescriptionStartDate"/>.Day.
    /// </summary>
    public int? RefillDayOfMonthOverride { get; set; }

    /// <summary>
    /// Start of current prescription validity. The day-of-month is this medication's refill day.
    /// </summary>
    public DateOnly PrescriptionStartDate { get; set; }

    /// <summary>Default prescription validity length in months (typically 6).</summary>
    public int PrescriptionDurationMonths { get; set; } = 6;
}
