using System;
using FluentResults;

namespace NvidiaDisplayController.Global;

public static class ResultExtensions
{
    public static Result IfSuccess(this Result r, Action action)
    {
        if (r.IsSuccess)
            action();
        return r;
    }

    public static Result<T> IfSuccess<T>(this Result<T> r, Action<T> action)
    {
        if (r.IsSuccess)
            action(r.Value);
        return r;
    }
    
    public static Result<T1> IfFail<T, T1>(this Result<T> r, Func<T1> action)
    {
        if (r.IsFailed)
            return action();
        return Result.Ok();
    }

    public static Result<T1> MapIfSuccess<T, T1>(this Result<T> r, Func<T, T1> action)
    {
        return r.IsSuccess ? r.Map(action) : Result.Fail<T1>("");
    }

    public static Result<T> Do<T>(this Result<T> r, Action<T> f)
    {
        f(r.Value);
        return r;
    }
}