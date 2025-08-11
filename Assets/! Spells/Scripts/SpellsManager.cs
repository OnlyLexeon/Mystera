using System;
using System.Collections.Generic;
using System.IO;
using UnityEngine;

public class SpellsManager : MonoBehaviour
{
    public static SpellsManager instance;

    [Header("All spells List")]
    public List<SpellList> spellList = new List<SpellList>();

    public List<SpellObject> equippedSpells = new List<SpellObject>();
    public float maxMana = 100;
    public float manaRegenRate = 1f;
    public float manaRegenAmount = 1f;
    public float manaDrainPerPoint = 0.1f;
    public int maxSpellSlots = 4;
    public bool isCasting = false;
    public bool manaRegen = false;
    public bool manaDrain = false;

    [Header("Player Current Data (For Debug Only)")]
    public float currentMana = 0;

    [Header("Private Data (For Debug Only)")]
    public float _timePassed = 0.0f;
    public int _currentSlotIndex = -1;

    public bool testingMode = false;

    private string savePath => Path.Combine(Application.persistentDataPath, "spellSate.json");

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

    private void Start()
    {
        if (!testingMode)
            LoadSpellState();
    }

    private void Update()
    {
        manaRegen = false;
        if (!isCasting)
        {
            if (currentMana < maxMana)
            {
                manaRegen = true;
            }
        }

        if(manaRegen)
        {
            _timePassed += Time.deltaTime;

            while (_timePassed >= manaRegenRate)
            {
                _timePassed -= manaRegenRate;
                currentMana += manaRegenAmount;

                if (currentMana > maxMana)
                {
                    currentMana = maxMana;
                    break;
                }
            }
        }
    }

    public void EquipSpell(int spellIndex)
    {
        if (_currentSlotIndex >= 0)
        {
            equippedSpells[_currentSlotIndex] = spellList[spellIndex].spellObj;
        }
        SaveSpellState();
    }

    public void UnequipSpell()
    {
        if (_currentSlotIndex >= 0)
        {
            equippedSpells[_currentSlotIndex] = null;
        }
        SaveSpellState();
    }

    public void ResetManaRegenTimer()
    {
        _timePassed = 0.0f;
    }

    public void SaveSpellState()
    {
        SpellState data = new SpellState();
        foreach (var spells in spellList)
        {
            if (spells.learned)
            {
                data.learnedSpellID.Add(spells.spellObj.spellID);
            }
        }
        for (int i = 0; i < equippedSpells.Count; i++)
        {
            if (equippedSpells[i] != null)
            {
                data.equippedSpellID.Add(equippedSpells[i].spellID);
            }
            else
            {
                data.equippedSpellID.Add(null);
            }
        }

        string json = JsonUtility.ToJson(data, true);
        File.WriteAllText(savePath, json);
    }

    public void LoadSpellState()
    {
        ResetEquippedSpellList();

        if (File.Exists(savePath))
        {
            string json = File.ReadAllText(savePath);
            var data = JsonUtility.FromJson<SpellState>(json);

            foreach (var spells in spellList)
            {
                if (data.learnedSpellID.Contains(spells.spellID))
                {
                    spells.learned = true;
                    if (data.equippedSpellID.Contains(spells.spellID))
                    {
                        equippedSpells[data.equippedSpellID.IndexOf(spells.spellID)] = spells.spellObj;
                    }
                }
                else
                {
                    spells.learned = false;
                }
            }
        }
        else
        {
            foreach (var spells in spellList)
            {
                spells.learned = spells.spellObj.defaultLearned;
            }
            int index = 0;
            while (!spellList[index].learned)
                index++;
            equippedSpells[0] = spellList[index].spellObj;
        }
    }

    public void ResetEquippedSpellList()
    {
        equippedSpells.Clear();
        for (int i = 0; i < maxSpellSlots; i++)
        {
            equippedSpells.Add(null);
        }
    }

    public class SpellState
    {
        public List<string> learnedSpellID = new List<string>();

        public List<string> equippedSpellID = new List<string>();
    }
}

[Serializable]
public class SpellList
{
    public SpellObject spellObj;
    public string spellID => spellObj.spellID;
    public bool learned = false;
}
