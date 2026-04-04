using UnityEngine;

public class IntroStepController : MonoBehaviour
{
    [SerializeField] private FirstLaunchFlowController firstLaunchFlowController;

    public void ContinueToMainMenu()
    {
        if (firstLaunchFlowController != null)
            firstLaunchFlowController.FinishFirstLaunchFlow();
    }
}