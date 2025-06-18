using System;
using System.Threading;
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
    [field: SerializeField] public TMP_Text PlayerLifeText { get; private set; }
    [field: SerializeField] public TMP_Text OpponentLifeText { get; private set; }
    [field: SerializeField] public TMP_Text TimerText { get; private set; }

    public Action<int> OnButtonSkipClicked;
    public Action<int> OnButtonPassMainPhase1Click;
    public Action<int> OnButtonPassCombatBeginningPhaseClick;

    private float _timer;

    void Awake()
    {
        ButtonSkip.onClick.AddListener(() => { OnButtonSkipClicked(0); });
        ButtonSkipOpponent.onClick.AddListener(() => { OnButtonSkipClicked(1); });
    }

    void Update()
    {
        _timer -= Time.deltaTime;
        if (_timer > 0)
            TimerText.text = _timer.ToString("0");
        else
            TimerText.text = "";
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

    public void UpdatePlayerLife(int playerIndex, int life)
    {
        if (playerIndex == 0)
            PlayerLifeText.text = life.ToString();
        else
            OpponentLifeText.text = life.ToString();
    }

    public void SetTimer(float time)
    {
        _timer = time;
    }

}
