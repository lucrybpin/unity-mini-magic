using System;
using System.Threading.Tasks;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class UIController : MonoBehaviour
{
    [field: SerializeField] public UIMessagesController UIMessagesController { get; private set; }
    [field: SerializeField] public Button ButtonSkip { get; private set; }
    [field: SerializeField] public TMP_Text ButtonSkipText { get; private set; }

    [field: SerializeField] public Button ButtonSkipOpponent { get; private set; }
    [field: SerializeField] public TMP_Text ButtonSkipOpponentText { get; private set; }

    public Action<int> OnButtonSkipClicked;
    public Action<int> OnButtonPassMainPhase1Click;
    public Action<int> OnButtonPassCombatBeginningPhaseClick;

    void Awake()
    {
        ButtonSkip.onClick.AddListener(() => { OnButtonSkipClicked(0); });
        ButtonSkipOpponent.onClick.AddListener(() => { OnButtonSkipClicked(1); });
    }

    public Task ShowMessage(string message)
    {
        return UIMessagesController.ShowMessage(message);
    }

    public void SetButtonSkipVisibility(bool isVisibile, string buttonText = "")
    {
        ButtonSkip.gameObject.SetActive(isVisibile);
        ButtonSkipText.text = buttonText;
    }

    public void SetButtonSkipOpponentVisibility(bool isVisibile, string buttonText = "")
    {
        ButtonSkipOpponent.gameObject.SetActive(isVisibile);
        ButtonSkipOpponentText.text = buttonText;
    }

}
