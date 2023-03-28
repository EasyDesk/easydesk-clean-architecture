namespace EasyDesk.Commons;

public static class TaskUtils
{
    public static async Task Then(this Task task, Action taskContinuation)
    {
        await task;
        taskContinuation();
    }

    public static async Task Then(this Task task, AsyncAction taskContinuation)
    {
        await task;
        await taskContinuation();
    }

    public static async Task Then<T>(this Task<T> task, Action<T> taskContinuation)
    {
        taskContinuation(await task);
    }

    public static async Task Then<T>(this Task<T> task, AsyncAction<T> taskContinuation)
    {
        await taskContinuation(await task);
    }

    public static async Task<B> Map<A, B>(this Task<A> task, Func<A, B> mapper)
    {
        return mapper(await task);
    }

    public static async Task<T> Map<T>(this Task task, Func<T> mapper)
    {
        await task;
        return mapper();
    }

    public static async Task<B> FlatMap<A, B>(this Task<A> task, AsyncFunc<A, B> taskContinuation)
    {
        return await taskContinuation(await task);
    }

    public static async void FireAndForget(this Task task)
    {
        await task;
    }

    public static async void FireAndForget<TException>(this Task task, Action<TException> exceptionHandler)
        where TException : Exception
    {
        try
        {
            await task;
        }
        catch (TException ex)
        {
            exceptionHandler(ex);
        }
    }
}
