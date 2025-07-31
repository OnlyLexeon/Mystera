using System;
using System.Collections.Generic;
using UnityEngine;

public class SpellsManager : MonoBehaviour
{
    public static SpellsManager instance;

    [Header("All spells List")]
    public List<SpellList> spellList = new List<SpellList>();

    public List<SpellObject> equippedSpells = new List<SpellObject>();
    public float maxMana = 100;
    public float manaRegenRate = 1f;
    public float manaDrainPerPoint = 0.1f;
    public int maxSpellSlots = 4;
    public bool isCasting = false;

    [Header("Player Current Data (For Debug Only)")]
    public float currentMana = 0;

    [Header("Private Data (For Debug Only)")]
    public float timePassed = 0.0f;

    private void Awake()
    {
        if (instance != null && instance != this)
        {
            Destroy(gameObject);
            return;
        }
        instance = this;
        DontDestroyOnLoad(gameObject);

        currentMana = maxMana;
    }

    private void Update()
    {
        if (!isCasting)
        {
            if (currentMana < maxMana)
            {
                timePassed += Time.deltaTime;
                if (timePassed > manaRegenRate)
                {
                    int manaRegen = (int)(timePassed / manaRegenRate);
                    timePassed -= manaRegen;
                    currentMana += manaRegen;
                    if (currentMana > maxMana)
                        currentMana = maxMana;
                }
            }
        }
    }

    public void ResetManaRegenTimer()
    {
        timePassed = 0.0f;
    }
}

[Serializable]
public class SpellList
{
    public SpellObject spellObj;
    public bool learned = false;
}
