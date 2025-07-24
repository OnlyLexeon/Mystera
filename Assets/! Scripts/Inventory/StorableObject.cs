using UnityEngine;


public class StorableObject : MonoBehaviour
{
    public StorableData data;

    public float lockoutDuration = 2f;
    private float spawnTime;

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
}

[CreateAssetMenu(menuName = "Inventory/Storable Database Entry")]
public class StorableData : ScriptableObject
{
    public string itemID;
    public string displayName;
    public Sprite icon;
    public GameObject prefab;
}

