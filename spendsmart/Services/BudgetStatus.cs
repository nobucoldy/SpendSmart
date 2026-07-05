namespace spendsmart.Services;

public sealed class BudgetStatus
{
    private const decimal NearLimitThresholdPercentage = 80m;

    public int BudgetId { get; init; }

    public int? CategoryId { get; init; }

    public string CategoryName { get; init; } = string.Empty;

    public decimal LimitAmount { get; init; }

    public decimal SpentAmount { get; init; }

    public decimal RemainingAmount => LimitAmount - SpentAmount;

    public decimal PercentageUsed { get; init; }

    public bool IsOverLimit => SpentAmount > LimitAmount;

    public bool IsNearLimit => !IsOverLimit && PercentageUsed >= NearLimitThresholdPercentage;

    public string StatusColor => IsOverLimit ? "#EF5350" : IsNearLimit ? "#FFA726" : "#26A69A";

    public string StatusText => IsOverLimit
        ? "Đã vượt hạn mức!"
        : IsNearLimit
            ? "Sắp đạt hạn mức"
            : "Trong hạn mức";
}
