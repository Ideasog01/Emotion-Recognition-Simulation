using System.Collections;
using UnityEngine;

public class AssessmentManager : MonoBehaviour
{
    public enum AssessmentMetric { Arousal, Valence, Dominance };

    public static AssessmentMetric CurrentMetric = AssessmentMetric.Arousal;

    public static int ImageStimuliIndex;

    public Sprite[] imageStimuliArray;

    [SerializeField] private float imageDisplayTime = 20;

    [SerializeField] private int baselineTimer = 120;

    private int arousalRating;
    private int valenceRating;
    private int dominanceRating;

    private string ratingText;

    public void SelectAssessmentOption(int index)
    {
        switch(CurrentMetric)
        {
            case AssessmentMetric.Arousal:
                arousalRating = index;
                break;
            case AssessmentMetric.Valence:
                valenceRating = index;
                break;
            case AssessmentMetric.Dominance:
                dominanceRating = index;
                ratingText += arousalRating + " " + valenceRating + " " + dominanceRating + "\n";
                break;
        }
    }

    public void StartBaselineTimer()
    {
        GameManager.InterfaceManager.UpdateBaslineMenu(baselineTimer);
        StartCoroutine(BaselineTimer());
    }

    private IEnumerator BaselineTimer()
    {
        yield return new WaitForSeconds(1);
        baselineTimer--;

        GameManager.InterfaceManager.UpdateBaslineMenu(baselineTimer);

        if(baselineTimer > 0)
        {
            StartCoroutine(BaselineTimer());
        }
    }

    public void DisplayImage()
    {
        if(ImageStimuliIndex < imageStimuliArray.Length)
        {
            GameManager.InterfaceManager.DisplayImageStimuli();
            StartCoroutine(ImageDisplayTimer());
        }
        else
        {
            GameManager.InterfaceManager.DisplayEndMenu();
            SaveManager.WriteFile(ratingText, "SAM_Data" + ".txt");
        }
    }

    private IEnumerator ImageDisplayTimer()
    {
        yield return new WaitForSeconds(3);
        GameManager.EyeTrackingFeatures.StartSession();
        yield return new WaitForSeconds(imageDisplayTime);
        ImageStimuliIndex++;
        CurrentMetric = AssessmentMetric.Arousal;

        GameManager.InterfaceManager.ShowOptions(CurrentMetric);
        GameManager.EyeTrackingFeatures.EndSession();
    }
}
