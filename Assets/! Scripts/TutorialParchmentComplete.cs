using UnityEngine;

public class TutorialParchmentComplete : MonoBehaviour
{
    public event System.Action OnTaskCompleted;

    public void TaskComplete()
    {
        OnTaskCompleted?.Invoke();
    }
}
