using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LateStart : MonoBehaviour
{
    private static readonly List<LateCall> Calls = new();

    private class LateCall
    {
        public Action Action;
        public float Delay;
    }

    public static void AddLate(Action action, float delay = 0.1f)
    {
        Calls.Add(new LateCall
        {
            Action = action,
            Delay = delay
        });
    }

    private void Start()
    {
        foreach (LateCall call in Calls)
        {
            StartCoroutine(RunCall(call));
        }

        Calls.Clear();
    }

    private IEnumerator RunCall(LateCall call)
    {
        call.Delay = Mathf.Min(0.05f, call.Delay); // clamp
        yield return new WaitForSeconds(call.Delay);

        call.Action?.Invoke();
    }
}