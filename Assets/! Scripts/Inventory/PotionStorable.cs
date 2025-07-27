using UnityEngine;

public class PotionStorable : StorableObject
{
    public Potion potion;
    public Sprite potionIcon;

    private void Start()
    {
        if (potion == null) potion = GetComponent<Potion>();
    }

    public StoredPotionData GetPotionData()
    {
        return potion.GetSaveData();
    }

    public void LoadPotionData(StoredPotionData saved)
    {
        potion.LoadFromData(saved);
    }
}

public interface IStorableData
{
}


