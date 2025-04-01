using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class InterfaceManager : MonoBehaviour
{
    [Header("Assessment UI")]

    [SerializeField] private TextMeshProUGUI questionText;

    [SerializeField] private Image[] assessmentImageArray;

    [SerializeField] private GameObject optionDisplay;

    [Header("Assessment Images")]

    [SerializeField] private Sprite[] arousalImages;
    [SerializeField] private Sprite[] valenceImages;
    [SerializeField] private Sprite[] dominanceImages;

    [Header("Image Display")]

    [SerializeField] private Image imageStimuli;

    [SerializeField] private Sprite[] imageStimuliArray;

    public void ShowOptions(AssessmentManager.AssessmentMetric metric)
    {
        optionDisplay.SetActive(true);

        for(int i = 0; i < assessmentImageArray.Length; i++)
        {
            switch(metric)
            {
                case AssessmentManager.AssessmentMetric.Arousal:
                    assessmentImageArray[i].sprite = arousalImages[i];
                    break;
                case AssessmentManager.AssessmentMetric.Valence:
                    assessmentImageArray[i].sprite = valenceImages[i];
                    break;
                case AssessmentManager.AssessmentMetric.Dominance:
                    assessmentImageArray[i].sprite = dominanceImages[i];
                    break;
            }
        }
    }

    public void SelectOption(int index)
    {
        GameManager.AssessmentManager.SelectAssessmentOption(index);

        if(AssessmentManager.CurrentMetric == AssessmentManager.AssessmentMetric.Dominance)
        {
            optionDisplay.SetActive(false);
        }
        else
        {
            ShowOptions(AssessmentManager.CurrentMetric + 1);
        }
    }

    public void DisplayImageStimuli()
    {
        imageStimuli.gameObject.SetActive(true);
        imageStimuli.sprite = imageStimuliArray[AssessmentManager.ImageStimuliIndex];
    }
}
