using System.Collections;
using UnityEngine;
using UnityEngine.Events;

public class DelayedEvent : MonoBehaviour
{
    [Tooltip("The event to invoke after the delay.")]
    public UnityEvent onDelayedEvent;
    private Coroutine delayCoroutine;

    public void DoDelayedEvent(float delay)
    {
        delayCoroutine = StartCoroutine(DelayedEventCoroutine(delay));
    }

    private IEnumerator DelayedEventCoroutine(float delay)
    {
        yield return new WaitForSeconds(delay);
        onDelayedEvent?.Invoke();
        delayCoroutine = null;
    }

    public void CancelDelay()
    {
        if (delayCoroutine != null)
        {
            StopCoroutine(delayCoroutine);
            delayCoroutine = null;
        }
    }
}
