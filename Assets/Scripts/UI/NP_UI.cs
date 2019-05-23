using Photon.Pun;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class NP_UI : MonoBehaviour
{
    #region Public Fields
    public Color Blue;
    public Color Red;
    #endregion

    #region Private Fields
    float CharacterHeight = 0f;
    Transform targetTransform;
    Vector3 targetPosition;
    private PlayerManager target;

    [Tooltip("Pixel offset from the player target")]
    [SerializeField]
    private Vector3 screenOffset = new Vector3(0f, 30f, 0f);

    [Tooltip("UI Text to display Player's Name")]
    [SerializeField]
    private Text playerNameText;


    [Tooltip("UI Slider to display Player's Health")]
    [SerializeField]
    private Slider playerHealthSlider;
    #endregion

    #region MonoBehaviour Callbacks

    private void LateUpdate()
    {
        if (targetTransform != null)
        {
            targetPosition = targetTransform.position;
            targetPosition.y += CharacterHeight;
            this.transform.position = Camera.main.WorldToScreenPoint(targetPosition) + screenOffset;
        }
        else
        {
            Destroy(this.gameObject);
        }
    }

    void Awake()
    {
        transform.SetParent(GameObject.Find("HUD").transform);
        playerHealthSlider = GetComponent<Slider>();
    }

    #endregion


    #region Public Methods

    public void SetTarget(PlayerManager _target)
    {
        if (_target == null)
        {
            Debug.LogError("<Color=Red><a>Missing</a></Color> PlayMakerManager target for PlayerUI.SetTarget.", this);
            return;
        }
        target = _target;
        if (playerNameText != null)
        {
            playerNameText.text = target.GetComponent<PhotonView>().Owner.NickName;
        }
        if(target.gameObject.tag == "Blue")
        {
            playerNameText.GetComponent<Text>().color = Blue;
        }
        else if(target.gameObject.tag == "Red")
        {
            playerNameText.GetComponent<Text>().color = Red;
        }
        targetTransform = target.GetComponent<Transform>();
        CapsuleCollider characterMesh = target.GetComponent<CapsuleCollider>();
        if (characterMesh != null)
        {
            Debug.Log("Character height:" + characterMesh.height);  
            CharacterHeight = characterMesh.height;
        }
    }

    public void UpdateHealthUI(float Health)
    {
        if(playerHealthSlider != null)
        {
            playerHealthSlider.value = Health;
        }
    }

    #endregion
}
