using System.Collections;
using UnityEngine;

public class AssessmentManager : MonoBehaviour
{
    public enum AssessmentMetric { Arousal, Valence, Dominance };

    public static AssessmentMetric CurrentMetric = AssessmentMetric.Arousal;

    public static int ImageStimuliIndex;

    [SerializeField] private int baselineTimer = 120;

    private int arousalRating;
    private int valenceRating;
    private int dominanceRating;

    private string ratingText;

    public void SelectAssessmentOption(int index)
    {
        ratingText += arousalRating + " " + valenceRating + " " + dominanceRating + "\n";
        ImageStimuliIndex++;
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
        else
        {

        }
    }
}
