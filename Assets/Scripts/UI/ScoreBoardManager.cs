using Photon.Pun;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class ScoreBoardManager : MonoBehaviour
{
    public GameObject RedScoreText, BlueScoreText;
    public static ScoreBoardManager instance;

    private void Awake()
    {
        instance = this;
    }

    public void UpdateScoreBoard( int RedTeamScore, int BlueTeamScore)
    {
        Debug.Log(PhotonNetwork.NickName + "'s ScoreBoard updated to :" + RedTeamScore + "," + BlueTeamScore );
        RedScoreText.GetComponent<Text>().text = RedTeamScore.ToString();
        BlueScoreText.GetComponent<Text>().text = BlueTeamScore.ToString();
        Debug.Log(PhotonNetwork.NickName + "'s ScoreBoard updated to :" + RedScoreText.GetComponent<Text>().text + "," + BlueScoreText.GetComponent<Text>().text);
    }
}
