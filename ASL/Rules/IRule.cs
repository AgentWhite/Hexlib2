namespace ASL.Rules
{
    /// <summary>
    /// Defines the base interface for a rule within the ASL system.
    /// </summary>
    public interface IRule<TContext>
    {
        /// <summary>
        /// Gets the unique identifier for the rule.
        /// </summary>
        string Id { get; }

        /// <summary>
        /// Gets the name of the rule.
        /// </summary>
        string Name { get; }
        
        /// <summary>
        /// An optional description of a rule.
        /// </summary>
        string? Description { get; }

        /// <summary>
        /// Gets the execution priority of the rule within a pipeline.
        /// </summary>
        RulePriority Priority { get; }

        /// <summary>
        /// Applies the rule against a given context.
        /// </summary>
        /// <param name="context">The context data to evaluate.</param>
        /// <returns>True if the rule condition is met; otherwise, false.</returns>
        void Apply(TContext context);
    }
}
