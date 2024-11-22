using UnityEngine;

public class JacketHoldAction : MonoBehaviour
{
    [SerializeField] UpdatorOnChange updator;
    [SerializeField] HoldableButton holdableButton;

    void Awake()
    {
        if (holdableButton != null)
        {
            holdableButton.OnHold += SpecialAction;
        }
    }

    public void SpecialAction()
    {
        if (updator.SelectedFumenAddress == "F_H_AprilRabbit")
        {
            holdableButton.OnInput -= SpecialAction;
            InGameTransition.Transition2InGame("F_E_AprilRabbit", Difficulty.Extra);
        }
    }
}
