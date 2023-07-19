using UnityEngine;
using Unity.Netcode;
using UnityEngine.UI;
using TMPro;
using UnityEngine.SceneManagement;

public class HomeScript : MonoBehaviour
{
    public Button[] uiButtons;
    public TMP_Text counterPlayer;

    private void Update()
    {
        counterPlayer.text = $"{GameManager.Instance.playersCounter}/6";
        if (NetworkManager.Singleton.IsHost)
        {
            uiButtons[2].gameObject.SetActive(true);
        }
    }

    public void OnClickTeamButton(int teamType)
    {
        GameManager.Instance.team = teamType;


        if (teamType == 1)
        {
            NetworkManager.Singleton.StartHost();
        }
        else
        {
            NetworkManager.Singleton.StartClient();
        }
    }

    public void OnClickReadyButton()
    {
        var status = NetworkManager.Singleton.SceneManager.LoadScene("Game", LoadSceneMode.Single);
        if (status != SceneEventProgressStatus.Started)
        {
            Debug.LogWarning($"Failed to load {"Game"} " +
                  $"with a {nameof(SceneEventProgressStatus)}: {status}");
        }
    }
}
