using System;
using System.Collections;
using Reactor.Utilities;
using UnityEngine;

namespace COG.Utils;

public static class TaskUtils
{
    public static void RunTask(Action action)
    {
        Coroutines.Start(Run());
        return;

        IEnumerator Run()
        {
            yield return new WaitForSeconds(0);
            action();
        }
    }

    public static void RunTaskAfter(float seconds, Action action)
    {
        Coroutines.Start(Run());
        return;
        
        IEnumerator Run()
        {
            yield return new WaitForSeconds(seconds);
            action();
        }
    }

    public static void RunTaskAfter(uint frames, Action action)
    {
        Coroutines.Start(Run());
        return;

        IEnumerator Run()
        {
            for (uint i = 0; i < frames; i++)
                yield return null;
            action();
        }
    }
}