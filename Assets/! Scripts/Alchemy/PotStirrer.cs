using UnityEngine;

public class PotStirrer : MonoBehaviour
{
    public bool isVelocity = false;
    public bool isSelected = false;

    public bool isStirring = false;

    public void Update()
    {
        if (isVelocity && isSelected) isStirring = true;
        else isStirring = false;
    }

    public void SetVelocity(bool state)
    {
        isVelocity = state;
    }

    public void SetSelected(bool state)
    {
        isSelected = state;
    }
}
