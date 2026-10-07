using UnityEngine;
using NaughtyAttributes;
using UnityEditor.ShaderKeywordFilter;

[CreateAssetMenu(fileName = "NPCScriptable", menuName = "Scriptable Objects/NPCScriptable")]
public class NPCScriptable : ScriptableObject
{
    [field: SerializeField] public NPCType NpcType { get; private set; }
    [field: SerializeField] public ObjectType RevealObjectType { get; private set; }
    [field:SerializeField] public string NpcName { get; private set; }
    [field: SerializeField] public Sprite NpcBaseSprite { get; private set; }
    [field: SerializeField] public Sprite NpcRevealedSprite { get; private set; }
    [field: SerializeField] public bool IsMurderer { get; private set; }
    [field:SerializeField] public string VoiceLine { get; private set; }
    [field:ShowIf("HaveAltVoiceline")][field:SerializeField] public string VoiceLineAlt { get; private set; }

    bool HaveAltVoiceline() { return NpcType != NPCType.Renard; }
}
public enum NPCType 
{
    Humain,
    BaiZe,
    Mei,
    Renard
}
