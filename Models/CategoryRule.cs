namespace SpendWise.Models;

/// <summary>Which transaction column a rule looks at.</summary>
public enum RuleField
{
    Counterparty,
    Operation,
    Description
}

/// <summary>How the rule's pattern is compared with the column (always case-insensitive).</summary>
public enum RuleMatch
{
    Contains,
    Equals
}

/// <summary>
/// "If <see cref="Field"/> <see cref="Match"/> <see cref="Pattern"/>, the transaction belongs to <see cref="Category"/>."
/// Lower <see cref="Priority"/> wins when several rules match.
/// </summary>
public sealed class CategoryRule
{
    public Guid Id { get; set; }
    public RuleField Field { get; set; }
    public RuleMatch Match { get; set; }
    public string Pattern { get; set; } = string.Empty;
    public int Priority { get; set; } = 100;
    public Guid CategoryId { get; set; }
    public Category Category { get; set; } = null!;
}
