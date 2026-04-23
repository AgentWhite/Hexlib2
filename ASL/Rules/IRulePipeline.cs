namespace ASL.Rules
{
    /// <summary>
    /// Defines a pipeline that executes a sequence of rules against a specific context.
    /// </summary>
    /// <typeparam name="TContext">The type of context the rules will be applied to.</typeparam>
    public interface IRulePipeline<TContext>
    {
        /// <summary>
        /// Gets the collection of rules in the pipeline.
        /// </summary>
        IEnumerable<IRule<TContext>> Rules { get; }

        /// <summary>
        /// Executes all rules in the pipeline against the provided context.
        /// </summary>
        /// <param name="context">The context to process.</param>
        void Execute(TContext context);

        /// <summary>
        /// Adds a rule to the pipeline.
        /// </summary>
        /// <param name="rule">The rule to add.</param>
        void Register(IRule<TContext> rule);
        /// <summary>
        /// Unregisters a rule from the pipeline. 
        /// </summary>
        /// <param name="ruleId">The string id of the rule.</param>
        void Unregister(string ruleId);

        /// <summary>
        /// Returns the rules currently active in the pipeline.
        /// </summary>
        IEnumerable<IRule<TContext>> GetActiveRules();
    }
}
