using UnityEngine;

public class GameManager : MonoBehaviour
{
    public static InterfaceManager InterfaceManager;

    public static EyeTrackingFeatures EyeTrackingFeatures;
    public static AssessmentManager AssessmentManager;

    public static SaveManager SaveManager;

    private void Awake()
    {
        InterfaceManager = this.GetComponent<InterfaceManager>();
        EyeTrackingFeatures = this.GetComponent<EyeTrackingFeatures>();
        AssessmentManager = this.GetComponent<AssessmentManager>();
        SaveManager = this.GetComponent<SaveManager>();
    }

    public void StartExperiment()
    {
        
    }

    public void ExitApplication()
    {
        Application.Quit();
    }
}
