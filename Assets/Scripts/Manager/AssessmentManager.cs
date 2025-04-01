using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AssessmentManager : MonoBehaviour
{
    public enum AssessmentMetric { Arousal, Valence, Dominance };

    public static AssessmentMetric CurrentMetric = AssessmentMetric.Arousal;

    public static int ImageStimuliIndex;

    private int arousalRating;
    private int valenceRating;
    private int dominanceRating;

    private string ratingText;

    public void SelectAssessmentOption(int index)
    {
        ratingText += arousalRating + " " + valenceRating + " " + dominanceRating + "\n";
        ImageStimuliIndex++;
    }
}
