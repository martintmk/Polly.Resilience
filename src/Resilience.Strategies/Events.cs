﻿namespace Resilience.Strategies;

public delegate ValueTask EventsDelegate<T, TContext>(Outcome<T> outcome, TContext context);

public class Events<TContext>
{
    internal Dictionary<Type, List<object>> Callbacks { get; } = new();

    public Events<TContext> Add<T>(Action<Outcome<T>, TContext> callback)
    {
        return Add<T>((o, c) =>
        {
            callback(o, c);
            return default;
        });
    }

    public Events<TContext> Add(Action<TContext> callback) => Add((_, args) => callback(args));

    public Events<TContext> Add(Func<TContext, ValueTask> callback) => Add((_, args) => callback(args));

    public Events<TContext> Add(EventsDelegate<object, TContext> callback) => Add<object>(callback);

    public Events<TContext> Add(Action<Outcome<object>, TContext> callback)
    {
        return Add<object>((o, c) =>
        {
            callback(o, c);
            return default;
        });
    }

    public Events<TContext> Add<T>(EventsDelegate<T, TContext> callback)
    {
        var type = typeof(T);

        if (Callbacks.TryGetValue(type, out var list))
        {
            list.Add(callback);
        }
        else
        {
            Callbacks.Add(type, new List<object> { callback });
        }

        return this;
    }
}

