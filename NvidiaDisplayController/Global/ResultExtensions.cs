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
}