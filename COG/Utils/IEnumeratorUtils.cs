using System;
using System.Collections;
using System.Collections.Generic;
using System.Threading.Tasks;
using TMPro;
using UnityEngine;

namespace COG.Utils
{
    public static class IEnumeratorUtils
    {
        static public IEnumerator WaitAsCoroutine(this Task task)
        {
            while (!task.IsCompleted) yield return null;
            yield break;
        }

        static public IEnumerable<T> Delimit<T>(this IEnumerable<T> enumerable, T delimiter)
        {
            bool isFirst = true;
            foreach (T item in enumerable)
            {
                if (!isFirst) yield return delimiter;
                yield return item;
                isFirst = false;
            }
        }

        private static IEnumerator AnimateCoroutine(HideAndSeekDeathPopup __instance)
        {
            HideAndSeekDeathPopup andSeekDeathPopup = __instance;
            DateTime startTime = DateTime.UtcNow;
            bool doNeedReduceSpeed = true;
            bool doNeedIncrease = true;
            while (true)
            {
                AnimatorStateInfo animatorStateInfo = andSeekDeathPopup.animator.GetCurrentAnimatorStateInfo(0);
                if (animatorStateInfo.IsName("Show"))
                {
                    if (doNeedReduceSpeed && DateTime.UtcNow.Subtract(startTime).TotalSeconds >= 0.5)
                    {
                        __instance.animator.speed *= 0.15f;
                        doNeedReduceSpeed = false;
                    }
                    if (doNeedIncrease && DateTime.UtcNow.Subtract(startTime).TotalSeconds >= 6.8f)
                    {
                        __instance.animator.speed *= 6.66f;
                        doNeedIncrease = false;
                    }
                    yield return null;
                }
                else
                    break;
            }
            UnityEngine.Object.Destroy(andSeekDeathPopup.gameObject);
        }

        static public IEnumerator Action(Action action)
        {
            action.Invoke();
            yield break;
        }
        private static IEnumerator RunTaskAsCoroutine(Task task)
        {
            while (!task.IsCompleted)
            {
                yield return null;
            }
        }
    }
}
