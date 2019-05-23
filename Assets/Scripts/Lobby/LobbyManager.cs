using Photon.Pun;
using Photon.Realtime;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LobbyManager : MonoBehaviourPunCallbacks
{
    /// <summary>
    /// This script is in charge of controlling flow of UI Components along with connecting to photon and joining a lobby. (Basically everything that happens in the lobby) 
    /// </summary>

    #region Private Serializable Fields
    [SerializeField]
    private byte maxPlayers;
    #endregion

    #region Singleton Declaration
    public static LobbyManager lobbyManager;
    #endregion

    #region Public Fields
    public GameObject CancelButton;
    public GameObject LaunchButton;
    public GameObject ConnectingText;
    [Tooltip("The scene number for the arena")]
    public int multiplayerScene = 1;
    #endregion

    #region MonoBehaviourPun Callbacks and Overides

    private void Awake()
    {
        CancelButton.SetActive(false);
        LaunchButton.SetActive(false);
        ConnectingText.SetActive(true);
        lobbyManager = this;                    //creates the singleton, lives within the main menu scene
    }

    private void Start()
    {
        PhotonNetwork.ConnectUsingSettings(); 
    }

    public override void OnConnectedToMaster()
    {
        Debug.Log("Player has established connection to the photon server");
        PhotonNetwork.AutomaticallySyncScene = true;
        CancelButton.SetActive(false);
        ConnectingText.SetActive(false);
        LaunchButton.SetActive(true);
    }

    public override void OnJoinedRoom()
    {
        Debug.Log("Player has sucessfully joined a room");
        if (PhotonNetwork.IsMasterClient && PhotonNetwork.CurrentRoom.PlayerCount == 1)
        {
            StartGame();
        }
        else
            return;
    }

    public override void OnJoinRandomFailed(short returnCode, string message)
    {
        Debug.Log("Failed to join random room, there must be no rooms available.......creating room....");
        CreateRoom();
    }

    public override void OnCreateRoomFailed(short returnCode, string message)
    {
        Debug.Log("Creating Room has Failed.(might already a room with same name). Going to try agian. ERROR:" + message);
        CreateRoom();
    }
    #endregion

    #region Public Methods

    public void OnLaunchButtonClicked()
    {
        LaunchButton.SetActive(false);
        CancelButton.SetActive(true);
        if (PhotonNetwork.IsConnected)
        {
            PhotonNetwork.JoinRandomRoom();
            Debug.Log("JoinRandomRoom is called.");
        }
    }

    void CreateRoom()
    {
        int randomroomname = Random.Range(0, 1000);
        RoomOptions roomOptions = new RoomOptions() { IsOpen = true, IsVisible = true, MaxPlayers = maxPlayers };
        Debug.Log("Creating room :" + "Room_" + randomroomname.ToString());
        PhotonNetwork.CreateRoom("Room_" + randomroomname.ToString(), roomOptions);
        //check OnCreateRoomFailed for when CreateRoom Fails
    }

    void StartGame()
    {
        Debug.Log("Loading Level");
        PhotonNetwork.LoadLevel(multiplayerScene);
    }

    public void OnCancelButtonClicked()
    {
        PhotonNetwork.LeaveRoom();
        CancelButton.SetActive(false);
        LaunchButton.SetActive(true);
    }
    #endregion
}
