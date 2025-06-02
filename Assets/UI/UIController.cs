using System;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.UI;

public class UIController : MonoBehaviour
{
    [field: SerializeField] public UIMessagesController UIMessagesController { get; private set; }
    [field: SerializeField] public Button ButtonPassUpkeep { get; private set; }
    [field: SerializeField] public Button ButtonPassMainPhase1 { get; private set; }
    [field: SerializeField] public Button ButtonPassCombatBeginningPhase { get; private set; }
    [field: SerializeField] public Button ButtonPassCombatBeginningPhaseOpponent { get; private set; }


    public Action<int> OnButtonPassUpkeepClick;
    public Action<int> OnButtonPassMainPhase1Click;
    public Action<int> OnButtonPassCombatBeginningPhaseClick;


    void Awake()
    {
        ButtonPassUpkeep.onClick.AddListener(() => { OnButtonPassUpkeepClick.Invoke(1); });
        ButtonPassMainPhase1.onClick.AddListener(() => { OnButtonPassMainPhase1Click.Invoke(1); });
        ButtonPassCombatBeginningPhase.onClick.AddListener(() => { OnButtonPassCombatBeginningPhaseClick(1); });
        ButtonPassCombatBeginningPhaseOpponent.onClick.AddListener(() => { OnButtonPassCombatBeginningPhaseClick(2); });
    }

    public Task ShowMessage(string message)
    {
        return UIMessagesController.ShowMessage(message);
    }

    public void SetButtonPassUpkeepVisibility(bool isVisible)
    {
        ButtonPassUpkeep.gameObject.SetActive(isVisible);
    }

    public void SetButtonPassMainPhase1Visibility(bool isVisible)
    {
        ButtonPassMainPhase1.gameObject.SetActive(isVisible);
    }

    public void SetButonPassCombatBeginningPhase(bool isVisible)
    {
        ButtonPassCombatBeginningPhase.gameObject.SetActive(isVisible);
        ButtonPassCombatBeginningPhaseOpponent.gameObject.SetActive(isVisible);
    }



}
