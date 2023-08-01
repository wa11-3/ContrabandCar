using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class GameOverScript : MonoBehaviour
{
    public TMP_Text infoTx;

    void Start()
    {
        infoTx.text = $"Game Over\n{GameManager.Instance.winner} Win";
    }
}
