﻿namespace Resilience;

public static partial class ResilienceStrategyExtensions
{
    public static async ValueTask ExecuteAsync<TState>(this IResilienceStrategy strategy, Func<TState, CancellationToken, ValueTask> execute, TState state, CancellationToken cancellationToken = default)
    {
        var context = ResilienceContext.Get(cancellationToken);
        context.IsVoid = true;

        try
        {
            await strategy.ExecuteAsync(
                static async (context, state) =>
                {
                    await state.execute(state.state, context.CancellationToken).ConfigureAwait(context.ContinueOnCapturedContext);
                    return VoidResult.Instance;
                },
                context,
                (execute, state));
        }
        finally
        {
            ResilienceContext.Return(context);
        }
    }

    public static async ValueTask ExecuteAsync<TState>(this IResilienceStrategy strategy, Func<ResilienceContext, TState, ValueTask> execute, ResilienceContext context, TState state)
    {
        context.IsVoid = true;

        await strategy.ExecuteAsync(
            static async (context, state) =>
            {
                await state.execute(context, state.state).ConfigureAwait(context.ContinueOnCapturedContext);
                return VoidResult.Instance;
            },
            context,
            (execute, state));
    }

    public static async ValueTask ExecuteAsync(this IResilienceStrategy strategy, Func<ResilienceContext, ValueTask> execute, ResilienceContext context)
    {
        context.IsVoid = true;

        await strategy.ExecuteAsync(
            static async (context, state) =>
            {
                await state(context).ConfigureAwait(context.ContinueOnCapturedContext);
                return VoidResult.Instance;
            },
            context,
            execute);
    }

    public static async ValueTask<T> ExecuteAsync<T>(this IResilienceStrategy strategy, Func<CancellationToken, ValueTask<T>> execute, CancellationToken cancellationToken = default)
    {
        var context = ResilienceContext.Get(cancellationToken);

        try
        {
            return await strategy.ExecuteAsync(static (context, state) => state(context.CancellationToken), context, execute).ConfigureAwait(context.ContinueOnCapturedContext);
        }
        finally
        {
            ResilienceContext.Return(context);
        }
    }

    public static ValueTask<T> ExecuteAsync<T>(this IResilienceStrategy strategy, Func<ResilienceContext, ValueTask<T>> execute, ResilienceContext context)
    {
        return strategy.ExecuteAsync(static (context, state) => state(context), context, execute);
    }

    public static async ValueTask<T> ExecuteAsync<T, TState>(this IResilienceStrategy strategy, Func<TState, CancellationToken, ValueTask<T>> execute, TState state, CancellationToken cancellationToken)
    {
        var context = ResilienceContext.Get(cancellationToken);

        try
        {
            return await strategy.ExecuteAsync(static (context, state) => state.execute(state.state, context.CancellationToken), context, (execute, state)).ConfigureAwait(context.ContinueOnCapturedContext);
        }
        finally
        {
            ResilienceContext.Return(context);
        }
    }
}
