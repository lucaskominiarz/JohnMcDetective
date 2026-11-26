using System;
using System.Collections;
using DG.Tweening;
using TMPro;
  using UnityEngine;
using UnityEngine.Events;
using UnityEngine.EventSystems;
  using UnityEngine.SceneManagement;

  public class NPC : MonoBehaviour, IPointerClickHandler
{
    [SerializeField] private NPCScriptable npcData;
    
    [SerializeField] private Transform speechBubble;
    [SerializeField] private float tweenDuration = 0.5f;
    
    
    
    //private bool _isRevealed = false;
    private bool _wasClickedOnce = false;
    private TMP_Text[] _textZones;
    private bool _isSelected = false;

    private Vector3 _baseSpeechBubbleScale = Vector3.one;
    private Vector2 _baseColliderScale;
    private float _colliderSizeMultiplier = 5f;
    private BoxCollider2D _collider;

    //Ajout GD pour détecter le premier dialogue
    public UnityEvent OnFirstDialogue;

    //Ajout GD Pour changer de scène
    public UnityEvent OnArrested;
    public bool nextSceneTransition;
    private void Awake()
    {
        if (speechBubble)
        {
            int size = speechBubble.childCount;
            _textZones = new TMP_Text[size];
            for (int i = 0; i < size; i++)
            {
                _textZones[i] = speechBubble.GetChild(i).GetComponent<TMP_Text>();
            }
            
        }
        speechBubble.localScale = Vector3.zero;
        _collider = GetComponent<BoxCollider2D>();
        _baseColliderScale = _collider.size;
    }

    public NPCType GetNpcType()
    {
        return npcData.NpcType;
    }

    public void SetNpcDatas(NPCScriptable newNpcData)
    {
        npcData = newNpcData;
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
            if (!_wasClickedOnce)
            {
                _wasClickedOnce = true;
                OnFirstDialogue.Invoke();
            }

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
        
        if (!_textZones[1])
            return;
        _textZones[1].text = npcData.VoiceLine;
    }

    public void UseObject(ObjectScriptable usedObject)
    {
        if (usedObject.ObjectType == ObjectType.Mandat)
        {
            Arrest();
            return;
        }
    }

    public void SetSpeechBubble(Transform newSpeechBubble)
    {
        speechBubble = newSpeechBubble;
    }
    
    void Arrest() // modifier pour mettre une vraie fin
    {
        if (npcData.IsMurderer)
        {
            Debug.Log("Tueur trouvé");
            if (!_textZones[1])
                return;
            _textZones[1].text = "Tu m'as démasqué";

            //Ajout GD pour changer de scène après avoir gagné
            if(nextSceneTransition) OnArrested.Invoke();
            else StartCoroutine(RestartLevelCoroutine());

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
