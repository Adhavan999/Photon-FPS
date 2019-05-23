using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.IO;
using Photon.Pun;
using UnityEngine.SceneManagement;
using Photon.Realtime;
using ExitGames.Client.Photon;
using System;

public class GameManager : MonoBehaviourPunCallbacks, IOnEventCallback
{
    #region Singleton Decleration 
    public static GameManager instance;
    #endregion

    #region Score Structure
    public struct Score
    {
        public int RedTeamScore;
        public int BlueTeamScore;
    }
    public Score GameScore;
    #endregion

    #region Refrences
    public GameObject ScoreBoard;
    #endregion

    #region MonoBehaviour Callbacks
    void Start()
    {
        GameScore.BlueTeamScore = 0;
        GameScore.RedTeamScore = 0;
        instance = this;
        SpawnPlayer();
        if(PhotonNetwork.IsMasterClient)
        {
            Debug.Log(PhotonNetwork.NickName + " has joined. He is the master client.");
            MasterClientSetup();
        }
    }
    #endregion
    
    #region Public Methods

    public void MasterClientSetup()
    {
        ScoreBoardManager.instance.UpdateScoreBoard(GameScore.RedTeamScore, GameScore.BlueTeamScore);
    }

    /// <summary>
    /// SpawnPlayer instantiates the local player and assignes it a spawn location and a team and sets up scoreboard.
    /// </summary>
    public void SpawnPlayer()
    {
        if(PlayerManager.LocalPlayerInstance == null)
        {
            if (PhotonNetwork.CurrentRoom.PlayerCount % 2 == 0)
            {
                int rand = UnityEngine.Random.Range(-4, 4);
                rand *= 10;
                Vector3 SpawnVector = new Vector3(25.7f, 1.117f, rand);
                GameObject player = PhotonNetwork.Instantiate(Path.Combine("PhotonPrefabs", "Player"), SpawnVector, Quaternion.identity);
                PhotonView PV;
                PV = player.GetComponent<PhotonView>();
                PV.RPC("SetTeamTag", RpcTarget.AllBuffered, "Blue");
                if (!PhotonNetwork.IsMasterClient)
                    PV.RPC("InitializePlayer", RpcTarget.MasterClient);
                Debug.Log("Spawning local player of Blue team");
            }
            else
            {
                int rand = UnityEngine.Random.Range(-4, 4);
                rand = rand * 10;
                Vector3 SpawnVector = new Vector3(-25.7f, 1.117f, rand);
                GameObject player = PhotonNetwork.Instantiate(Path.Combine("PhotonPrefabs", "Player"), SpawnVector, Quaternion.identity);
                PhotonView PV;
                PV = player.GetComponent<PhotonView>();
                PV.RPC("SetTeamTag", RpcTarget.AllBuffered, "Red");
                if (!PhotonNetwork.IsMasterClient)
                    PV.RPC("InitializePlayer", RpcTarget.MasterClient);
                Debug.Log("Spawning local player of Red team");
            }
        }
    }

    public void LeaveRoom()
    {
        Debug.Log("Leaving Room.");
        PhotonNetwork.LeaveRoom();
    }
    #endregion

    #region RPCs and Event Callbacks

    public void OnEvent(EventData photonEvent)
    {
        // For When the OnDeath Event has been triggered by a PlayerManager
        if (photonEvent.Code == PlayerManager.OnDeathEventTriggerCode)
        {
            Debug.Log("OnDeath Event called");
            string team = photonEvent.CustomData.ToString();
            if (team == "Blue")
            {
                GameScore.RedTeamScore++;
            }
            if (team == "Red")
            {
                GameScore.BlueTeamScore++;
            }
            ScoreBoardManager.instance.UpdateScoreBoard(GameScore.RedTeamScore, GameScore.BlueTeamScore);
        }
    }

    public override void OnEnable()
    {
        PhotonNetwork.AddCallbackTarget(this);
    }

    public override void OnDisable()
    {
        PhotonNetwork.RemoveCallbackTarget(this);
    }
    #endregion

    #region PUN Overrides

    public override void OnPlayerEnteredRoom(Player newPlayer)
    {
        Debug.Log("OnPlayerEnteredRoom() was called:" + "client that joined is " + newPlayer.NickName);
    }

    public override void OnLeftRoom()
    {
        Debug.Log("Local Player has left room");
        Debug.Log("Loading Lobby...");
        SceneManager.LoadScene(0);
    }

    public override void OnPlayerLeftRoom(Player otherPlayer)
    {
        Debug.Log("Player :" + otherPlayer.NickName + " has left the room");
    }

    #endregion
}
