using UnityEngine;

public class NPC : MonoBehaviour
{
    [SerializeField] private NPCScriptable npcDatas;

    private bool _isRevealed = false;

    public void PlayVoiceline()
    {
        // afficher la voice line 1
        if (_isRevealed || npcDatas.VoiceLineAlt != "")
        {
            // afficher la voice line 2
        }
    }

    public void UseObject() // mettre en parametre le type d'objet et si c'est le bon, reveal
    {
        
    }
    
    void Reveal()
    {
        _isRevealed = true;
    }
}
