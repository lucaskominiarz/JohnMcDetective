  using System;
  using TMPro;
  using UnityEngine;
  using UnityEngine.SceneManagement;

  public class NPC : MonoBehaviour
{
    [SerializeField] private NPCScriptable npcData;
    [SerializeField] private TMP_Text[] textZones;
    [SerializeField] private SpriteRenderer spriteRenderer;

    private bool _isRevealed = false;

    private void Awake()
    {
        if(npcData.NpcRevealedSprite) 
            spriteRenderer.sprite = npcData.NpcBaseSprite;

    }

    public void PlayVoiceline()
    {
        if (!textZones[0])
            return;
        textZones[0].text = npcData.NpcName;
        string finalLine = npcData.VoiceLine;
        if (_isRevealed)
        {
            finalLine += " " + npcData.VoiceLineAlt;
        }
        if (!textZones[1])
            return;
        textZones[1].text = finalLine;
    }

    public void UseObject(ObjectScriptable usedObject)
    {
        if (usedObject.ObjectType == ObjectType.Mandat)
        {
            Arrest();
            return;
        }

        if (usedObject.ObjectType == npcData.RevealObjectType)
        {
            Reveal();
        }
        else
        {
            Debug.Log("Mauvais objet"); // a changer
        }
    }
    
    void Reveal()
    {
        _isRevealed = true;
        if(npcData.NpcRevealedSprite) 
            spriteRenderer.sprite = npcData.NpcRevealedSprite;
        PlayVoiceline();
    }

    void Arrest() // modifier pour mettre une vraie fin
    {
        if (npcData.IsMurderer)
        {
            Debug.Log("Tueur trouvé");
            SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
            return;
        }
        Debug.Log("Pas le tueur");
    }
}
