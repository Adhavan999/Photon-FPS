using ExitGames.Client.Photon;
using Photon.Pun;
using Photon.Realtime;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerManager : MonoBehaviour, IPunObservable, IOnEventCallback
{
    /// <summary>
    /// This Script sets up the player movement scripts and camera in the case if the player is the local player and disables them in case it is a network player.
    /// In charge of managing player attributes such as health, speed etc...
    /// </summary>
    #region Public Attributes
    [NonSerialized]
    public bool isDead = false;
    public float Health = 100f;
    public GameObject NetworkPlayerUIPrefab;
    public PhotonView PV;
    #endregion

    #region Private Refrences/Fields
    public const byte OnDeathEventTriggerCode = 0;
    public const byte OnNewPlayerEventTriggerCode = 1;
    private PlayerController playerController;
    private PlayerMotor playerMotor;
    private bool IsNetworkPlayer = false;
    private Vector3 nRealPosition = Vector3.zero;
    private Quaternion nRealRotation = Quaternion.identity;
    private NP_UI _NP_UI;
    private bool NP_isVisable = true;
    private Renderer PR;
    #endregion

    #region Singelton Declaration
    public static PlayerManager LocalPlayerInstance;
    #endregion

    #region MonoBehaviour Callbacks
    void Start()
    {
        PR = GetComponent<MeshRenderer>();
        playerController = GetComponent<PlayerController>();
        playerMotor = GetComponent<PlayerMotor>();
        if(PV.IsMine)
        {
            LocalPlayerInstance = this;
            playerController.enabled = true;
            playerMotor.enabled = true;
            IsNetworkPlayer = false;
            transform.GetChild(0).gameObject.SetActive(true);
            NewPlayerEvent();
        }
        else
        {
            if (NetworkPlayerUIPrefab != null)
            {
                GameObject NetworkPlayerUI = Instantiate(NetworkPlayerUIPrefab);
                _NP_UI = NetworkPlayerUI.GetComponent<NP_UI>();
                _NP_UI.SetTarget(this);
            }
            else
            {
                Debug.Log("no NetworkPlayerUIPrefab is attached.");
            }
            playerController.enabled = false;
            playerMotor.enabled = false;
            IsNetworkPlayer = true;
            transform.GetChild(0).gameObject.SetActive(false);              //sets the camera attached to the non-local player to inactive
        }
    }

    private void Update()
    {
        if(IsNetworkPlayer)
        {
            UpdateNetworkPlayerPos();
        }
    }

    private void LateUpdate()
    {
        if(IsNetworkPlayer)
        {
            if (NP_isVisable)
            {
                if (!PR.isVisible)
                {
                    NP_isVisable = false;
                    _NP_UI.gameObject.SetActive(false);
                }
            }
            else
            {
                if (PR.isVisible)
                {
                    NP_isVisable = true;
                    _NP_UI.gameObject.SetActive(true);
                }
            }
        }
        
    }

    #endregion

    #region Public RPC Methods
    [PunRPC]
    public void TakeDamage(float damage)
    {
        Health -= damage;

        if (IsNetworkPlayer)
        {
            _NP_UI.UpdateHealthUI(Health);
            Debug.Log("Player:" + GetComponent<PhotonView>().name + " is updating health UI");
        }
        else
        {
            if(LP_HealthBar.instance != null)
            {
                Debug.Log("The local player has taken damage!");
                LP_HealthBar.instance.UpdateHealthValue(Health);
            }
        }

        Debug.Log("Player: " + PV.Owner.NickName + " has taken " + damage + " damage. Health Remaining: " + Health);
        if (Health <= 0)
        {
            Die();
        }
    }

    public void Die()
    {
        if(!isDead)
        {
            Debug.Log("Player: " + PV.Owner.NickName + " has died.");
            if (!IsNetworkPlayer)
            {
                OnDeathEvent();
                GameManager.instance.LeaveRoom();
            }
            isDead = true;
        }
    }

    [PunRPC]
    public void SetTeamTag(string tag)
    {
        this.gameObject.tag = tag;
        Debug.Log(GetComponent<PhotonView>().Owner.NickName + "'s player is in " + tag + " team.");
    }

    /// <summary>
    /// This method is only handled by the master client and is used to update the new player's GameManager on the current status of the game. Example, Score, Time remaining etc...
    /// called by a new player for:
    /// PlayerManager.InitializeScore()
    /// </summary>
    /// <param name="photonMessageInfo"></param>
    [PunRPC]
    public void InitializePlayer(PhotonMessageInfo photonMessageInfo)
    {
        if (!PhotonNetwork.IsMasterClient)
        {
            Debug.Log("InitializePlayer() called on a non-master client! Aborting the call....");
            return;
        }
        Debug.Log(PV.Owner.NickName + "'s (MasterClient) GameManger has performed InitializePlayer() RPC for " + photonMessageInfo.Sender.NickName + "'s GameManager");
        photonMessageInfo.photonView.RPC("InitializeScore", photonMessageInfo.Sender, GameManager.instance.GameScore.RedTeamScore, GameManager.instance.GameScore.BlueTeamScore);
    }

    [PunRPC]
    public void InitializeScore(int RedTeamScore, int BlueTeamScore)
    {
        Debug.Log("Initializing Score for " + PV.Owner.NickName);
        GameManager.instance.GameScore.RedTeamScore = RedTeamScore;
        GameManager.instance.GameScore.BlueTeamScore = BlueTeamScore;
        ScoreBoardManager.instance.UpdateScoreBoard(GameManager.instance.GameScore.RedTeamScore, GameManager.instance.GameScore.BlueTeamScore);
    }

    [PunRPC]
    public void UpdatePlayerHealth(float health)
    {
        if(IsNetworkPlayer)
        {
            Debug.Log(PV.Owner.NickName + ": Network player health updated to:" + health);
            Health = health;
            _NP_UI.UpdateHealthUI(Health);
        }
        else
        {
            Debug.LogError("UpdatePlayerHealth called for a local player!");
        }
    }
    #endregion

    #region Methods for updating network players through data stream
    public void OnPhotonSerializeView(PhotonStream stream, PhotonMessageInfo info)
    {
        if (stream.IsWriting)
        {
            stream.SendNext(transform.position);
            stream.SendNext(transform.rotation);
        }
        if (stream.IsReading)
        {
            nRealPosition = (Vector3)stream.ReceiveNext();
            nRealRotation = (Quaternion)stream.ReceiveNext();
        }
    }

    private void UpdateNetworkPlayerPos()
    {
        transform.position = Vector3.Lerp(transform.position, nRealPosition,0.1f);
        transform.rotation = Quaternion.Lerp(transform.rotation, nRealRotation,0.1f);
    }

    #endregion

    #region event raising methods
    private void NewPlayerEvent()
    {
        SendOptions sendOptions = new SendOptions { Reliability = true };
        RaiseEventOptions raiseEventOptions = new RaiseEventOptions() { Receivers = ReceiverGroup.All };
        PhotonNetwork.RaiseEvent(OnNewPlayerEventTriggerCode, null, raiseEventOptions, sendOptions);
        Debug.Log(PV.Owner.NickName + " Raised a new player event.");
    }

    private void OnDeathEvent()
    {
        SendOptions sendOptions = new SendOptions { Reliability = true };
        RaiseEventOptions raiseEventOptions = new RaiseEventOptions() { Receivers = ReceiverGroup.All };
        PhotonNetwork.RaiseEvent(OnDeathEventTriggerCode, this.gameObject.tag, raiseEventOptions, sendOptions);
    }

    #endregion

    #region Event Recieving Methods
    public void OnEvent(EventData photonEvent)
    {
        switch (photonEvent.Code)
        {
            case OnNewPlayerEventTriggerCode:
                TriggerOnNewPlayerEventCallback(photonEvent);
                Debug.Log(PhotonNetwork.NickName + "Recieved OnNewPlayerEvent");
                return;

            default:
                return;
        }
    }

    public void OnEnable()
    {
        PhotonNetwork.NetworkingClient.EventReceived += OnEvent;
    }

    public void OnDisable()
    {
        PhotonNetwork.NetworkingClient.EventReceived -= OnEvent;
    }

    private void TriggerOnNewPlayerEventCallback(EventData photonEvent)
    {
        if(!PV.IsMine)
        {
            return;
        }
        PV.RPC("UpdatePlayerHealth", RpcTarget.Others, Health);
    }
    #endregion
}
