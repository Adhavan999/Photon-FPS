using UnityEngine.UI;
using UnityEngine;
using Photon.Realtime;
using Photon.Pun;

/// <summary>
/// Player name input field. Let the user input his name, will appear above the player in the game.
/// </summary>
[RequireComponent(typeof(InputField))]
public class PlayerNameInputField : MonoBehaviour
{
    #region Private Constants
    const string PlayerNamePrefKey = "playernamekey";

    #endregion

    #region Serializable Constants
    [Tooltip("Enter the default player name that will show up if player never sets his own name")]
    [SerializeField]
    private string InitialPlayerDefaultName = "playerdefault";
    #endregion

    #region MonoBehaviour Callbacks
    /// <summary>
    /// Checks if PlayerPrefs has a previous value for name or not.
    /// </summary>
    private void Start()
    {
        string DefaultName = string.Empty;
        InputField inputField = this.GetComponent<InputField>();
        if (PlayerPrefs.HasKey(PlayerNamePrefKey))
        {
            DefaultName = PlayerPrefs.GetString(PlayerNamePrefKey, InitialPlayerDefaultName);
            Debug.Log("Player Name exists in preferences. Name:" + DefaultName);
            inputField.text = DefaultName;
        }
        else
        {
            PlayerPrefs.SetString(PlayerNamePrefKey, "");
        }
    }
    #endregion

    #region Public Functions


    /// <summary>
    /// The SetPlayerName() function is the final function called and is responsible of updating the PhotonNetwork's client name.
    /// It is called when the 'Play' button is pressed.
    /// </summary>
    public void SetPlayerName()
    {
        InputField inputField = GetComponent<InputField>();
        //saving name into PlayerPreferences
        PlayerPrefs.SetString(PlayerNamePrefKey, inputField.text);
        
        //setting name as photonNetwork nickname
        if (inputField.text != null)
        {
            PhotonNetwork.NickName = inputField.text;
        }
        else
        {
            PhotonNetwork.NickName = InitialPlayerDefaultName;
        }
    }
    
    #endregion
}