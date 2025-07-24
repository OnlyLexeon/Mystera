using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "IncantationData", menuName = "Scriptable Objects/IncantationData")]
public class IncantationData : ScriptableObject
{
    public GameObject spell;
    public List<string> incantations = new List<string>();
}
