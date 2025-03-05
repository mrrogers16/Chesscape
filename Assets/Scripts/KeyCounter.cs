using UnityEngine;
using UnityEditor.UI;
using TMPro;


public class KeyCounter : MonoBehaviour
{
    public TextMeshProUGUI keyText;
    public Player playerObject;
    void Start()
    {

    }

    void Update()
    {
        if (playerObject.hasKey)
        {
            keyText.text = "1";
        }
        else
        {
            keyText.text = "0";
        }

    }



}
