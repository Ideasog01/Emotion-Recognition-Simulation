using UnityEngine;
using Unity.XR.PXR;
using System.IO;
using System.Collections;
using System.Collections.Generic;

public class EyeTrackingFeatures : MonoBehaviour
{
    public enum FeatureType { Blink, Fixation, Saccade, MicroSaccade }

    private System.Diagnostics.Stopwatch sessionTimer = new System.Diagnostics.Stopwatch();

    private int sessionNumber = 0;

    private string[] featureDurationArray = new string[3];
    private string[] featurePerSecondArray = new string[4];
    private string saccadeDirectionText;

    private Vector3 previousGaze;
    private Vector3 currentGaze;

    private float previousVelocity;

    private float gazeAngularVelocity = 0;
    private float gazeAcceleration = 0;

    private int[] featureCountPerSecond = { 0, 0}; //Blinks = 0 and Fixations = 1

    private List<EyeFeature> eyeFeatures = new List<EyeFeature>();

    private void Update()
    {
        if(sessionTimer != null && sessionTimer.IsRunning)
        {
            gazeAngularVelocity = GetAngularVelocity();
            TrackEyeTrackingFeatures();
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

    private void TrackEyeTrackingFeatures()
    {
        RecordFeature(FeatureType.Blink, 50);
        RecordFeature(FeatureType.Fixation, 300);
        RecordFeature(FeatureType.Saccade, 30);
    }

    private void RecordFeature(FeatureType type, float minDuration)
    {
        EyeFeature feature = GetEyeFeature(type);

        bool isFeatureActive = false;
        
        switch(type)
        {
            case FeatureType.Blink:
                isFeatureActive = IsEyesClosed(); //Blink detected
                break;
            case FeatureType.Fixation:
                isFeatureActive = gazeAngularVelocity < 20; //Fixation detected
                break;
            case FeatureType.Saccade:
                isFeatureActive = gazeAngularVelocity > 35 && gazeAcceleration > 400 && CalculateSaccadeAmplitude() > 5; //Saccade detected
                break;
            case FeatureType.MicroSaccade:
                
                EyeFeature fixation = GetEyeFeature(FeatureType.Fixation);
                EyeFeature saccade = GetEyeFeature(FeatureType.Saccade);

                if(fixation != null)
                {
                    isFeatureActive = gazeAngularVelocity < 20 && saccade != null && fixation.featureDuration >= 400; //Micro-Saccade detected
                }
                break;
            default:
                break;
        }

        if(isFeatureActive)
        {
            if(feature == null) //Feature just started
            {
                feature = new EyeFeature(type);
                feature.featureStartTime = Time.time * 1000; //Convert to milliseconds
                eyeFeatures.Add(feature);
            }
            else
            {
                feature.featureDuration = (Time.time * 1000) - feature.featureStartTime;
            }
        }
        else
        {
            if(feature != null)
            {
                feature.featureDuration = (Time.time * 1000) - feature.featureStartTime;

                if(feature.featureDuration > minDuration) //Valid
                {
                    if(feature.featureDuration < 1000) // Happened this second
                    {
                        featureCountPerSecond[(int)type]++;
                    }

                    //Assign to string for writing to file at end of session
                    featureDurationArray[(int)type] += feature.featureDuration.ToString() + "\n"; //Record the duration of the feature

                    //Assign to string for Saccade direction (if applicable)
                    if(type == FeatureType.Saccade)
                    {
                        PXR_EyeTracking.GetFoveatedGazeDirection(out Vector3 gazeDirection);
                        gazeDirection.Normalize();

                        Vector3 previousDirection = new Vector3(previousGaze.x, 0, previousGaze.z).normalized;
                        Vector3 currentDirection = new Vector3(currentGaze.x, 0, currentGaze.z).normalized;

                        float saccadeAngle = Vector3.SignedAngle(previousDirection, currentDirection, Vector3.up);

                        if(saccadeAngle < 0)
                        {
                            saccadeAngle += 360; //Convert to positive angle;
                        }

                        saccadeDirectionText += saccadeAngle.ToString() + "\n"; //Record the angle of the saccade
                    }
                }
                
                eyeFeatures.Remove(feature); //Blink was too short, remove it
            }
        }
    }

    #region Eye Properties

    private float GetAngularVelocity()
    {
        PXR_EyeTracking.GetFoveatedGazeDirection(out Vector3 gazeDirection);
        currentGaze = gazeDirection.normalized;

        float angleVelocity = 0;
        float deltaTime = Time.deltaTime;

        if (previousGaze != Vector3.zero && deltaTime > 0) 
        {
            float dotProduct = Vector3.Dot(previousGaze, currentGaze);
            dotProduct = Mathf.Clamp(dotProduct, -1.0f, 1.0f);

            float angleRadians = Mathf.Acos(dotProduct);
            float angleDegrees = Mathf.Rad2Deg * angleRadians;

            angleVelocity = angleDegrees / deltaTime; // Convert to velocity
        }

        //Compute acceleration
        gazeAcceleration = (angleVelocity - previousVelocity) / deltaTime;
        previousVelocity = angleVelocity;

        previousGaze = currentGaze;
        return angleVelocity;
    }

    private bool IsEyesClosed()
    {
        PXR_EyeTracking.GetLeftEyeGazeOpenness(out float leftEyeOpenness);
        PXR_EyeTracking.GetRightEyeGazeOpenness(out float rightEyeOpenness);

        if(leftEyeOpenness < 0.25f && rightEyeOpenness < 0.25f) //Eye closed detected, check if min duration matches 50ms
        {
            return true;
        }
        else
        {
            return false;
        }
    }

    private float CalculateSaccadeAmplitude()
    {
        PXR_EyeTracking.GetFoveatedGazeDirection(out Vector3 gazeDirection);
        currentGaze = gazeDirection.normalized;

        float dotProduct = Vector3.Dot(previousGaze, currentGaze);
        dotProduct = Mathf.Clamp(dotProduct, -1.0f, 1.0f);

        float angleRadians = Mathf.Acos(dotProduct);
        float angleDegrees = Mathf.Rad2Deg * angleRadians;

        return angleDegrees;
    }


    private EyeFeature GetEyeFeature(FeatureType featureType)
    {
        foreach(EyeFeature feature in eyeFeatures)
        {
            if(feature.featureType == featureType)
            {
                return feature;
            }
        }

        return null;
    }

    #endregion

    #region Session Management

    private void StartSession()
    {
        sessionTimer.Start();
        sessionNumber++;

        for(int i = 0; i < 2; i++)
        {
            featureDurationArray[i] = "";
            featureCountPerSecond[i] = 0;
        }   

        StartCoroutine(PerSecondTimer());
    }

    private void EndSession()
    {
        //Write data to files
        sessionTimer.Stop();
        float sessionDuration = (float)sessionTimer.Elapsed.TotalSeconds; //Total time of session in seconds
        
        WriteFile(sessionDuration.ToString(), "SessionDuration");

        WriteFile(featureDurationArray[(int)FeatureType.Blink], "BlinksDuration");
        WriteFile(featurePerSecondArray[(int)FeatureType.Blink], "BlinksPerSecond");

        WriteFile(featureDurationArray[(int)FeatureType.Fixation], "FixationsDuration");
        WriteFile(featurePerSecondArray[(int)FeatureType.Fixation], "FixationsPerSecond");
    }

    private IEnumerator PerSecondTimer() //For recording events every second that the session is active
    {
        yield return new WaitForSeconds(1);

        for(int i = 0; i < 4; i++)
        {
            featurePerSecondArray[i] += featureCountPerSecond[0].ToString() + "\n";
            featureCountPerSecond[i] = 0;
        }

        if(sessionTimer.IsRunning)
        {
            StartCoroutine(PerSecondTimer());
        }
    }

    #endregion
}

public class EyeFeature
{
    public float featureStartTime;
    public float featureDuration;

    public EyeTrackingFeatures.FeatureType featureType;

    public EyeFeature(EyeTrackingFeatures.FeatureType type)
    {
        featureType = type;
    }
}