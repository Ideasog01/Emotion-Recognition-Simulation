using UnityEngine;
using Unity.XR.PXR;
using System.IO;

public class EyeTrackingFeatures : MonoBehaviour
{
    private System.Diagnostics.Stopwatch sessionTimer = new System.Diagnostics.Stopwatch();

    private int sessionNumber = 0;

    string durationOfBlinksText;
    string blinksPerSecondText;

    string fixationDurationText;
    string fixationsPerSecondText;

    private float blinkStartTime;
    private bool blinkInProgress;
    private int numberOfBlinksThisSecond;

    Vector3 previousGaze;
    Vector3 currentGaze;

    private float fixationStartTime;
    private int numberOfFixationsThisSecond;

    private void Awake()
    {
        
    }

    private void Update()
    {
        if(sessionTimer != null && sessionTimer.IsRunning)
        {
            GetEyeTrackingFeatures();

            if(blinkInProgress)
            {
                CheckBlink();
            }
        }
    }

    private void WriteFile(string content, string dataType)
    {
        string filePath = Path.Combine(Application.persistentDataPath, "ET_Data_S" + sessionNumber.ToString() + "_" + dataType + ".txt");
        
        try
        {
            File.WriteAllText(filePath, content);
            Debug.Log("Eye Tracking Data written to: " + filePath);
        }
        catch (IOException e)
        {
            Debug.LogError("Failed to write Eye Tracking Data: " + e.Message);
        }
    }

    private void GetEyeTrackingFeatures()
    {
        #region Calculate Blinks (Number of Blinks per Second & Duration) 

        //Openness 0.0 indicates closed, 1.0 indicates open
        PXR_EyeTracking.GetLeftEyeGazeOpenness(out float leftEyeOpenness);
        PXR_EyeTracking.GetRightEyeGazeOpenness(out float rightEyeOpenness);

        if(leftEyeOpenness < 0.25f && rightEyeOpenness < 0.25f) //Eye closed detected, check if min duration matches 50ms
        {
            if(blinkStartTime == 0)
            {
                blinkStartTime = Time.time;
                Invoke("CheckBlink", 0.05f); //Invoke CheckBlink after 50ms
            }
        }

        #endregion

        #region Fixation (Stillness of Eye Movement overtime)
        CheckFixation();
        #endregion
    }

    private void CheckBlink() //Calls after 50ms
    {
        blinkInProgress = true;

        PXR_EyeTracking.GetLeftEyeGazeOpenness(out float leftEyeOpenness);
        PXR_EyeTracking.GetRightEyeGazeOpenness(out float rightEyeOpenness);

        if(leftEyeOpenness > 0.5f && rightEyeOpenness > 0.5f)
        {
            if(blinkStartTime == Time.time)
            {
                numberOfBlinksThisSecond++;
            }
            else
            {
                blinksPerSecondText += "\n" + numberOfBlinksThisSecond.ToString(); //Number of blinks in this second
                numberOfBlinksThisSecond = 0;
            }

            durationOfBlinksText += "\n" + (blinkStartTime - Time.time).ToString(); //Duration of blink in seconds
            Debug.Log("Blink detected!");
            blinkStartTime = 0;
            blinkInProgress = false;
        }
    }

    private void CheckFixation()
    {
         PXR_EyeTracking.GetFoveatedGazeDirection(out Vector3 gazeDirection);
        currentGaze = gazeDirection;

        if(previousGaze != Vector3.zero) //We can check for fixation
        {
            //Calculate Gaze Angle Change
            previousGaze.Normalize();
            currentGaze.Normalize();

            float dotProduct = Vector3.Dot(previousGaze, currentGaze);
            dotProduct = Mathf.Clamp(dotProduct, -1.0f, 1.0f); //Clamp to avoid NaN

            float angleRadians = Mathf.Acos(dotProduct);
            float angleDegrees = Mathf.Rad2Deg * angleRadians;

            if(angleDegrees < 2.0f) //If angle is less than 2 degrees, it is a fixation
            {
                fixationStartTime = Time.time; //Start fixation timer
            }
            else
            {
                if(fixationStartTime == Time.time)
                {
                    numberOfFixationsThisSecond++;
                }
                else
                {
                    fixationsPerSecondText += "\n" + numberOfFixationsThisSecond.ToString(); //Number of fixations in this second
                    numberOfFixationsThisSecond = 0;
                }

                float fixationDuration = Time.time - fixationStartTime; //Calculate fixation duration
                fixationDurationText += "\n" + fixationDuration.ToString(); //Duration of fixation in seconds
            }
        }

        previousGaze = currentGaze;
    }

    private void StartSession()
    {
        sessionTimer.Start();
        sessionNumber++;

        durationOfBlinksText = "";
        blinksPerSecondText = "";
        fixationDurationText = "";
        fixationsPerSecondText = "";
    }

    private void EndSession()
    {
        //Write data to files
        sessionTimer.Stop();
        float sessionDuration = (float)sessionTimer.Elapsed.TotalSeconds; //Total time of session in seconds
        
        WriteFile(sessionDuration.ToString(), "SessionDuration");

        WriteFile(durationOfBlinksText, "DurationOfBlinks");
        WriteFile(blinksPerSecondText, "BlinksPerSecond");

        WriteFile(fixationDurationText, "DurationsOfFixations");
        WriteFile(fixationsPerSecondText, "FixationsPerSecond");
        
    }

    //The duration of each blink
    //Blinks per second
}
