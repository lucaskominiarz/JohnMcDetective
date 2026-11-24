using UnityEngine;
using NaughtyAttributes;

[CreateAssetMenu(fileName = "NPCScriptable", menuName = "Scriptable Objects/NPCScriptable")]
public class NPCScriptable : ScriptableObject
{
    [field: SerializeField] public NPCType NpcType { get; private set; }
    [field:SerializeField] public string NpcName { get; private set; }
    [field: SerializeField] public bool IsMurderer { get; private set; }
    [field:SerializeField] public string VoiceLine { get; private set; }
    
}
public enum NPCType 
{
    Humain,
    Protecteur,
    Chasseur,
    Renard
}
