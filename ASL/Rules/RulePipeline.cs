namespace ASL.Rules;

/// <summary>
/// Default implementation of <see cref="IRulePipeline{TContext}"/> that executes rules in priority order.
/// </summary>
/// <typeparam name="TContext">The type of context the rules will be applied to.</typeparam>
public class RulePipeline<TContext> : IRulePipeline<TContext>
{
    private readonly List<IRule<TContext>> _rules = new();

    /// <inheritdoc />
    public IEnumerable<IRule<TContext>> Rules => _rules.AsReadOnly();

    /// <inheritdoc />
    public void Register(IRule<TContext> rule) => _rules.Add(rule);

    /// <inheritdoc />
    public void Unregister(string ruleId) =>
        _rules.RemoveAll(r => r.Id == ruleId);

    /// <inheritdoc />
    public void Execute(TContext context)
    {
        foreach (var rule in _rules.OrderBy(r => (int)r.Priority))
            rule.Apply(context);
    }

    /// <inheritdoc />
    public IEnumerable<IRule<TContext>> GetActiveRules() => _rules.AsReadOnly();
}
