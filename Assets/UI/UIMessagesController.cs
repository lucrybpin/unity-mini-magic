using System;
using System.Threading.Tasks;
using TMPro;
using UnityEngine;

[Serializable]
public class UIMessagesController
{
    [field: SerializeField] public GameObject ContainerMessage { get; private set; }
    [field: SerializeField] public TMP_Text Text { get; private set; }

    public async Task ShowMessage(string message)
    {
        Text.text = message;
        ContainerMessage.SetActive(true);
        await Task.Delay(TimeSpan.FromSeconds(2f));
        Text.text = "";
        ContainerMessage.SetActive(false);
    }
}
