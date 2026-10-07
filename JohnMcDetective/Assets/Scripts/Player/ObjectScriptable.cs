using UnityEngine;
using NaughtyAttributes;
using UnityEditor.ShaderKeywordFilter;

[CreateAssetMenu(fileName = "ObjectScriptable", menuName = "Scriptable Objects/ObjectScriptable")]
public class ObjectScriptable : ScriptableObject
{
    [field: SerializeField] public ObjectType ObjectType { get; private set; }
    [field:SerializeField] public string ObjectName { get; private set; }
    [field: SerializeField] public Sprite ObjectSprite { get; private set; }
    [field: SerializeField] public Vector2 ObjectScale { get; private set; }
    
}
public enum ObjectType 
{
    Mandat,
    Encens
}