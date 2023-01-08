﻿using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.ObjectPool;
using Polly;

namespace Resilience.Strategies.Bench;

internal static partial class Helper
{
    private static readonly ObjectPool<Context> _contextPool = ObjectPool.Create<Context>();

    public static async ValueTask ExecuteAsync(this object obj, ResilienceTechnology technology)
    {
        switch (technology)
        {
            case ResilienceTechnology.Polly:
                var pollyContext = _contextPool.Get();

                try
                {
                    await ((IAsyncPolicy<int>)obj).ExecuteAsync(static (_, _) => Task.FromResult(999), pollyContext, System.Threading.CancellationToken.None).ConfigureAwait(false);
                }
                finally
                {
                    _contextPool.Return(pollyContext);
                }

                return;
            case ResilienceTechnology.ResiliencePrototype:
                var context = ResilienceContext.Get();
                try
                {
                    await ((IResilienceStrategy)obj).ExecuteAsync(static (_, _) => new ValueTask<int>(999), context, 0).ConfigureAwait(false);
                }
                finally
                {
                    ResilienceContext.Return(context);
                }

                return;
        }

        throw new NotSupportedException();
    }

    private static IResilienceStrategy CreateStrategy(Action<IResilienceStrategyBuilder> configure)
    {
        var builder = new ResilienceStrategyBuilder();
        configure(builder);
        return builder.Create();
    }
}
