using System;
using System.Threading.Tasks;
using UnityEngine;

public static class ErrorHandler
{
    public static async Task SafeExecuteAsync(Func<Task> action, string contextMessage = null)
    {
        try
        {
            await action();
        }
        catch (Exception e)
        {
            string errorMessage = string.IsNullOrEmpty(contextMessage) 
                ? e.Message 
                : $"{contextMessage}: {e.Message}";
            Debug.LogError(errorMessage);
        }
    }

    public static async Task<T> SafeExecuteAsync<T>(Func<Task<T>> action, string contextMessage = null, T defaultValue = default)
    {
        try
        {
            return await action();
        }
        catch (Exception e)
        {
            string errorMessage = string.IsNullOrEmpty(contextMessage) 
                ? e.Message 
                : $"{contextMessage}: {e.Message}";
            Debug.LogError(errorMessage);
            return defaultValue;
        }
    }
}