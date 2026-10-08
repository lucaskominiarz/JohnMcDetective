using System;
using System.Collections;
using DG.Tweening;
using TMPro;
  using UnityEngine;
  using UnityEngine.EventSystems;
  using UnityEngine.SceneManagement;

  public class NPC : MonoBehaviour, IPointerClickHandler
{
    [SerializeField] private NPCScriptable npcData;
    
    [SerializeField] private SpriteRenderer spriteRenderer;
    [SerializeField] private Transform speechBubble;
    [SerializeField] private float tweenDuration = 0.5f;
    
    
    private bool _isRevealed = false;
    private TMP_Text[] _textZones;
    private bool _isSelected = false;

    private Vector3 _baseSpeechBubbleScale;
    private Vector2 _baseColliderScale;
    private float _colliderSizeMultiplier = 5f;
    private BoxCollider2D _collider;
    private void Awake()
    {
        if(npcData.NpcRevealedSprite) 
            spriteRenderer.sprite = npcData.NpcBaseSprite;
        if (speechBubble)
        {
            int size = speechBubble.childCount;
            _textZones = new TMP_Text[size];
            for (int i = 0; i < size; i++)
            {
                _textZones[i] = speechBubble.GetChild(i).GetComponent<TMP_Text>();
            }
            
        }

        _baseSpeechBubbleScale = speechBubble.localScale;
        speechBubble.localScale = Vector3.zero;
        _collider = GetComponent<BoxCollider2D>();
        _baseColliderScale = _collider.size;
    }

    
    public void OnPointerClick(PointerEventData eventData)
    {
        if (_isSelected)
        {
            speechBubble.DOScale(Vector3.zero, tweenDuration);
            _collider.size = _baseColliderScale;
        }
        else
        {
            speechBubble.DOScale(_baseSpeechBubbleScale, tweenDuration);
            PlayVoiceline();
            _collider.size *= _colliderSizeMultiplier;
        }
        _isSelected = !_isSelected;
    }
    

    void PlayVoiceline()
    {
        if (_textZones.Length == 0)
            return;
        _textZones[0].text = npcData.NpcName;
        string finalLine = npcData.VoiceLine;
        if (_isRevealed)
        {
            finalLine += " " + npcData.VoiceLineAlt;
        }
        if (!_textZones[1])
            return;
        _textZones[1].text = finalLine;
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

    public void SetSpeechBubble(Transform newSpeechBubble)
    {
        speechBubble = newSpeechBubble;
    }
    
    void Reveal()
    {
        _isRevealed = true;
        if(npcData.NpcRevealedSprite) 
            spriteRenderer.sprite = npcData.NpcRevealedSprite;
        speechBubble.DOScale(_baseSpeechBubbleScale, tweenDuration);
        PlayVoiceline();
    }

    void Arrest() // modifier pour mettre une vraie fin
    {
        if (npcData.IsMurderer)
        {
            Debug.Log("Tueur trouvé");
            if (!_textZones[1])
                return;
            _textZones[1].text = "Tu m'as démasqué";
            StartCoroutine(RestartLevelCoroutine());
            
            return;
        }
        if (!_textZones[1])
            return;
        _textZones[1].text = "Tu t'es trompé";
        StartCoroutine(RestartLevelCoroutine());
        Debug.Log("Pas le tueur");
    }

    private IEnumerator RestartLevelCoroutine() // juste pour le proto a dégager apres
    {
        yield return new WaitForSeconds(3f);
        SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
    }

    
}
