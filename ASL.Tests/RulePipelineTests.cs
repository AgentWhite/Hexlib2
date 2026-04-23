using ASL.Rules;

namespace ASL.Tests;

public class RulePipelineTests
{
    // --- Test helpers ---

    private class TestContext
    {
        public List<string> AppliedOrder { get; } = new();
    }

    private class RecordingRule : IRule<TestContext>
    {
        public string Id { get; }
        public string Name { get; }
        public string? Description => null;
        public RulePriority Priority { get; }

        public RecordingRule(string id, RulePriority priority = RulePriority.Core)
        {
            Id = id;
            Name = id;
            Priority = priority;
        }

        public void Apply(TestContext context) => context.AppliedOrder.Add(Id);
    }

    // --- Register / GetActiveRules / Rules ---

    [Fact]
    public void Register_AddsRuleToActiveRules()
    {
        var pipeline = new RulePipeline<TestContext>();
        var rule = new RecordingRule("rule1");

        pipeline.Register(rule);

        Assert.Single(pipeline.GetActiveRules());
        Assert.Single(pipeline.Rules);
    }

    [Fact]
    public void Register_MultipleRules_AllAppearInActiveRules()
    {
        var pipeline = new RulePipeline<TestContext>();
        pipeline.Register(new RecordingRule("a"));
        pipeline.Register(new RecordingRule("b"));
        pipeline.Register(new RecordingRule("c"));

        Assert.Equal(3, pipeline.GetActiveRules().Count());
    }

    // --- Unregister ---

    [Fact]
    public void Unregister_ById_RemovesRule()
    {
        var pipeline = new RulePipeline<TestContext>();
        pipeline.Register(new RecordingRule("target"));
        pipeline.Register(new RecordingRule("other"));

        pipeline.Unregister("target");

        var active = pipeline.GetActiveRules().ToList();
        Assert.Single(active);
        Assert.Equal("other", active[0].Id);
    }

    [Fact]
    public void Unregister_UnknownId_DoesNotThrow()
    {
        var pipeline = new RulePipeline<TestContext>();
        pipeline.Register(new RecordingRule("rule1"));

        var ex = Record.Exception(() => pipeline.Unregister("nonexistent"));

        Assert.Null(ex);
        Assert.Single(pipeline.GetActiveRules());
    }

    [Fact]
    public void Unregister_EmptyPipeline_DoesNotThrow()
    {
        var pipeline = new RulePipeline<TestContext>();
        var ex = Record.Exception(() => pipeline.Unregister("anything"));
        Assert.Null(ex);
    }

    // --- Execute ---

    [Fact]
    public void Execute_EmptyPipeline_DoesNotThrow()
    {
        var pipeline = new RulePipeline<TestContext>();
        var ctx = new TestContext();

        var ex = Record.Exception(() => pipeline.Execute(ctx));

        Assert.Null(ex);
        Assert.Empty(ctx.AppliedOrder);
    }

    [Fact]
    public void Execute_SingleRule_AppliesRule()
    {
        var pipeline = new RulePipeline<TestContext>();
        pipeline.Register(new RecordingRule("only"));
        var ctx = new TestContext();

        pipeline.Execute(ctx);

        Assert.Single(ctx.AppliedOrder);
        Assert.Equal("only", ctx.AppliedOrder[0]);
    }

    [Fact]
    public void Execute_MultipleRules_AllAreApplied()
    {
        var pipeline = new RulePipeline<TestContext>();
        pipeline.Register(new RecordingRule("a"));
        pipeline.Register(new RecordingRule("b"));
        pipeline.Register(new RecordingRule("c"));
        var ctx = new TestContext();

        pipeline.Execute(ctx);

        Assert.Equal(3, ctx.AppliedOrder.Count);
        Assert.Contains("a", ctx.AppliedOrder);
        Assert.Contains("b", ctx.AppliedOrder);
        Assert.Contains("c", ctx.AppliedOrder);
    }

    [Fact]
    public void Execute_CoreRulesRunBeforeSSR()
    {
        var pipeline = new RulePipeline<TestContext>();
        pipeline.Register(new RecordingRule("ssr1", RulePriority.SSR));
        pipeline.Register(new RecordingRule("core1", RulePriority.Core));
        pipeline.Register(new RecordingRule("ssr2", RulePriority.SSR));
        pipeline.Register(new RecordingRule("core2", RulePriority.Core));
        var ctx = new TestContext();

        pipeline.Execute(ctx);

        var coreIndices = ctx.AppliedOrder
            .Select((id, i) => (id, i))
            .Where(x => x.id.StartsWith("core"))
            .Select(x => x.i)
            .ToList();

        var ssrIndices = ctx.AppliedOrder
            .Select((id, i) => (id, i))
            .Where(x => x.id.StartsWith("ssr"))
            .Select(x => x.i)
            .ToList();

        Assert.True(coreIndices.Max() < ssrIndices.Min(),
            "All Core rules must execute before any SSR rule.");
    }

    [Fact]
    public void Execute_AfterUnregister_DoesNotApplyRemovedRule()
    {
        var pipeline = new RulePipeline<TestContext>();
        pipeline.Register(new RecordingRule("keep"));
        pipeline.Register(new RecordingRule("remove"));
        pipeline.Unregister("remove");
        var ctx = new TestContext();

        pipeline.Execute(ctx);

        Assert.Single(ctx.AppliedOrder);
        Assert.Equal("keep", ctx.AppliedOrder[0]);
    }

    [Fact]
    public void Execute_CanBeCalledMultipleTimes()
    {
        var pipeline = new RulePipeline<TestContext>();
        pipeline.Register(new RecordingRule("rule"));
        var ctx = new TestContext();

        pipeline.Execute(ctx);
        pipeline.Execute(ctx);

        Assert.Equal(2, ctx.AppliedOrder.Count);
    }
}
