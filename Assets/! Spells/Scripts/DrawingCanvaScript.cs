using System.Collections.Generic;
using UnityEngine;

public class DrawingCanvaScript : MonoBehaviour
{
    public GameObject spellRefObj;
    public GameObject spellRefHandleObj;
    public Transform spellRefHandleEntry;

    private Animator animator;
    private GameObject currentHandleObj;

    private void Awake()
    {
        animator = GetComponent<Animator>();
    }

    public void OpenDrawing()
    {
        animator.SetBool("StartDrawing", true);

        currentHandleObj = Instantiate(spellRefHandleObj, spellRefHandleEntry);

        List<SpellObject> equippedSpells = SpellsManager.instance.equippedSpells;
        for (int i = 0; i < equippedSpells.Count; i++)
        {
            if (equippedSpells[i] != null)
            {
                ReferenceSpellItem item = Instantiate(spellRefObj, currentHandleObj.transform).GetComponent<ReferenceSpellItem>();
                item.SetSpellRef(equippedSpells[i].spellArrayStructure, equippedSpells[i].spellID);
            }
        }
    }

    public void CloseDrawing()
    {
        animator.SetBool("StartDrawing", false);

        Destroy(currentHandleObj);
    }
}
