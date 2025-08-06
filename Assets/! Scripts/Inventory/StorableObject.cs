using UnityEngine;


public class StorableObject : MonoBehaviour
{
    public StorableData data;

    private float lockoutDuration = 1f;
    private float spawnTime;
    private bool hasBeenStored = false;

    public string itemID => data != null ? data.itemID : "";

    private void Start()
    {
        spawnTime = Time.time;
    }
    public void SetFreshSpawn(float duration = 2f)
    {
        spawnTime = Time.time;
        lockoutDuration = duration;
    }

    public bool IsInLockout()
    {
        return Time.time - spawnTime < lockoutDuration;
    }

    public bool HasBeenStored()
    {
        return hasBeenStored;
    }

    public void MarkAsStored()
    {
        hasBeenStored = true;
    }
}

