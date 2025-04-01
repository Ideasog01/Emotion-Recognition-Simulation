using System.Collections;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class InterfaceManager : MonoBehaviour
{
    [Header("Assessment UI")]

    [SerializeField] private TextMeshProUGUI questionText;

    [SerializeField] private Image[] assessmentImageArray;

    [SerializeField] private GameObject optionDisplay;

    [SerializeField] private GameObject endMenu;

    [Header("Assessment Images")]

    [SerializeField] private Sprite[] arousalImages;
    [SerializeField] private Sprite[] valenceImages;
    [SerializeField] private Sprite[] dominanceImages;

    [Header("Image Display")]

    [SerializeField] private GameObject imageDisplayCanvas;

    [SerializeField] private Image imageStimuli;

    [SerializeField] private Animator imageDisplayAnimator;

    [SerializeField] private TextMeshProUGUI countdownText;

    [Header("Baseline Menu")]

    [SerializeField] private TextMeshProUGUI baselineTimerText;

    [SerializeField] private Button baselineContinueButton;

    private int _countdownTimer;

    public void ShowOptions(AssessmentManager.AssessmentMetric metric)
    {
        imageDisplayCanvas.SetActive(false);
        optionDisplay.SetActive(true);

        for(int i = 0; i < assessmentImageArray.Length; i++)
        {
            switch(metric)
            {
                case AssessmentManager.AssessmentMetric.Arousal:
                    questionText.text = "How strong was your emotion?";
                    assessmentImageArray[i].sprite = arousalImages[i];
                    break;
                case AssessmentManager.AssessmentMetric.Valence:
                questionText.text = "How positive was your emotion?";
                    assessmentImageArray[i].sprite = valenceImages[i];
                    break;
                case AssessmentManager.AssessmentMetric.Dominance:
                    questionText.text = "How dominant was your emotion?";
                    assessmentImageArray[i].sprite = dominanceImages[i];
                    break;
            }
        }
    }

    public void SelectOption(int index)
    {
        GameManager.AssessmentManager.SelectAssessmentOption(index);

        //Cycle through the metrics
        if(AssessmentManager.CurrentMetric == AssessmentManager.AssessmentMetric.Dominance)
        {
            optionDisplay.SetActive(false);
            GameManager.AssessmentManager.DisplayImage();
            return;
        }
        else if(AssessmentManager.CurrentMetric == AssessmentManager.AssessmentMetric.Arousal)
        {
            AssessmentManager.CurrentMetric = AssessmentManager.AssessmentMetric.Valence;
        }
        else if(AssessmentManager.CurrentMetric == AssessmentManager.AssessmentMetric.Valence)
        {
             AssessmentManager.CurrentMetric = AssessmentManager.AssessmentMetric.Dominance;
        }

        ShowOptions(AssessmentManager.CurrentMetric);
    }

    public void DisplayImageStimuli()
    {   
        imageDisplayCanvas.SetActive(true);
        imageStimuli.gameObject.SetActive(false);
        countdownText.gameObject.SetActive(true);

        _countdownTimer = 3;
        StartCoroutine(CountdownTimer());
    }

    private IEnumerator CountdownTimer()
    {
        imageDisplayAnimator.SetTrigger("countdown");
        countdownText.text = _countdownTimer.ToString();
        yield return new WaitForSeconds(1f);

        _countdownTimer--;

        if(_countdownTimer > 0)
        {
            StartCoroutine(CountdownTimer());
        }
        else
        {
            countdownText.gameObject.SetActive(false);
            imageStimuli.gameObject.SetActive(true);
            imageStimuli.sprite = GameManager.AssessmentManager.imageStimuliArray[AssessmentManager.ImageStimuliIndex];
        }
    }

    public void UpdateBaslineMenu(int timer)
    {
        int minute = timer / 60;
        int second = timer % 60;
        baselineTimerText.text = string.Format("{0:D2}:{1:D2}", minute, second);

        if(timer <= 0)
        {
            baselineContinueButton.interactable = true;
        }
    }

    public void DisplayEndMenu()
    {
        endMenu.SetActive(true);
    }
}
