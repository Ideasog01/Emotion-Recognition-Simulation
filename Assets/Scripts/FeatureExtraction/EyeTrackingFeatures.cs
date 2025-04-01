using UnityEngine;
using Unity.XR.PXR;
using System.IO;
using System.Collections;
using System.Collections.Generic;

public class EyeTrackingFeatures : MonoBehaviour
{
    public enum FeatureType { Blink, Fixation, Saccade, MicroSaccade }

    [SerializeField] private int sessionNumber = 0;

    [Header("Blink Thresholds")]
    [SerializeField] private float blinkMinimumDuration = 50; //Minimum duration of blink in milliseconds
    [SerializeField] private float blinkClosedThreshold = 0.3f; //Threshold for eye closure. 0.0 = closed, 1.0 = open
    
    [Header("Fixation Thresholds")]
    [SerializeField] private float fixationMinimumDuration = 300; //Minimum duration of fixation in milliseconds
    [SerializeField] private float fixationMaximumAngularVelocity = 20; //Maximum angular velocity for fixation in degrees per second

    [Header("Saccade Thresholds")]
    [SerializeField] private float saccadeMinimumDuration = 30; //Minimum duration of saccade in milliseconds
    [SerializeField] private float saccadeMinimumAngularVelocity = 35; //Minimum angular velocity for saccade
    [SerializeField] private float saccadeMinimumAcceleration = 30; //Minimum acceleration for saccade
    [SerializeField] private float saccadeMinimumAmplitude = 5; //Minimum amplitude for saccade

    [Header("Micro-Saccade Thresholds")]
    [SerializeField] private float microSaccadeMinimumDuration = 400; //Minimum duration of micro-saccade in milliseconds
    [SerializeField] private float microSaccadeMaximumAngularVelocity = 20; //Maximum angular velocity for micro-saccade
    [SerializeField] private float microSaccadeFixationMinimumDurationThreshold = 400; //Minimum duration of fixation before micro-saccade


    private string[] _featureDurationArray = new string[3];
    private string[] _featurePerSecondArray = new string[4];

    private Vector3 _previousGaze;
    private Vector3 _currentGaze;

    private float _previousVelocity;

    private float _gazeAngularVelocity = 0;
    private float _gazeAcceleration = 0;
    private float _saccadeAmplitude = 0;

    private float _microSaccadePeakVelocity = 0;

    private (float, float) _microSaccadeAmplitude = (0, 0);

    private int[] _featureCountPerSecond = { 0, 0, 0, 0}; //Blinks = 0 and Fixations = 1 and Saccade = 2 and Micro-Saccade = 3

    private List<EyeFeature> _eyeFeatures = new List<EyeFeature>();

    private System.Diagnostics.Stopwatch _sessionTimer = new System.Diagnostics.Stopwatch();

    private void Update()
    {
        if(_sessionTimer != null && _sessionTimer.IsRunning)
        {
            PXR_EyeTracking.GetFoveatedGazeDirection(out Vector3 gazeDirection);

            if(_previousGaze == Vector3.zero)
            {
                _previousGaze = gazeDirection.normalized;
                return;
            }
            
            _currentGaze = gazeDirection.normalized;
            _gazeAngularVelocity = GetAngularVelocity();
            _gazeAcceleration = GetAngularVelocity();
            _saccadeAmplitude = Calculate_saccadeAmplitude();
            _microSaccadeAmplitude = Calculate_microSaccadeAmplitude();

            _previousGaze = _currentGaze;

            TrackEyeTrackingFeatures();
        }
    }

    #region Record Eye Features

    private void TrackEyeTrackingFeatures()
    {
        RecordFeature(FeatureType.Blink, blinkMinimumDuration);
        RecordFeature(FeatureType.Fixation, fixationMinimumDuration);
        RecordFeature(FeatureType.Saccade, saccadeMinimumDuration);
        RecordFeature(FeatureType.MicroSaccade, microSaccadeMinimumDuration);
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
                isFeatureActive = _gazeAngularVelocity <= fixationMaximumAngularVelocity; //Fixation detected
                break;
            case FeatureType.Saccade:
                isFeatureActive = _gazeAngularVelocity >= saccadeMinimumAngularVelocity && _gazeAcceleration >= saccadeMinimumAcceleration && _saccadeAmplitude >= saccadeMinimumAmplitude; //Saccade detected
                break;
            case FeatureType.MicroSaccade:
                EyeFeature fixation = GetEyeFeature(FeatureType.Fixation);
                EyeFeature saccade = GetEyeFeature(FeatureType.Saccade);

                if(fixation != null)
                {
                    isFeatureActive = _gazeAngularVelocity < microSaccadeMaximumAngularVelocity && saccade != null && fixation.featureDuration >= microSaccadeFixationMinimumDurationThreshold; //Micro-Saccade detected
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
                _eyeFeatures.Add(feature);
            }
            else
            {
                feature.featureDuration = (Time.time * 1000) - feature.featureStartTime;

                if(type == FeatureType.MicroSaccade)
                {
                    if(_gazeAngularVelocity > _microSaccadePeakVelocity)
                    {
                        _microSaccadePeakVelocity = _gazeAngularVelocity; //Record the peak velocity of the micro-saccade
                    }
                }
            }
        }
        else
        {
            if(feature != null)
            {
                feature.featureDuration = (Time.time * 1000) - feature.featureStartTime;

                if(feature.featureDuration >= minDuration) //Valid
                {
                    if(feature.featureDuration <= 1000) // Happened this second
                    {
                        _featureCountPerSecond[(int)type]++;
                    }

                    //Assign to string for writing to file at end of session
                    _featureDurationArray[(int)type] += feature.featureDuration.ToString() + "\n"; //Record the duration of the feature

                    if(type == FeatureType.Saccade)
                    {
                        WriteSaccadeFeatures();
                    }
                    else if(type == FeatureType.MicroSaccade)
                    {
                        WriteMicroSaccadeFeatures();
                    }

                }
                
                _eyeFeatures.Remove(feature); //Blink was too short, remove it
            }
        }
    }

    #endregion

    #region  Saccade and Micro-Saccade Features

    private string saccadeDirectionText = "";

    private void WriteSaccadeFeatures()
    {
        saccadeDirectionText += _currentGaze.ToString() + "\n"; //Record the direction of the saccade
    }

    private string _microSaccadePeakVelocityText = "";
    private string microSaccadeDirectionText = "";
    private string microSaccadeHorizontalAmplitudeText = "";
    private string microSaccadeVerticalAmplitudeText = "";

    private void WriteMicroSaccadeFeatures()
    {
        _microSaccadePeakVelocityText += _microSaccadePeakVelocity.ToString() + "\n"; //Record the peak velocity of the micro-saccade
        microSaccadeDirectionText += _currentGaze.ToString() + "\n"; //Record the direction of the micro-saccade
        microSaccadeHorizontalAmplitudeText += _microSaccadeAmplitude.Item1.ToString() + "\n"; //Record the horizontal amplitude of the micro-saccade
        microSaccadeVerticalAmplitudeText += _microSaccadeAmplitude.Item2.ToString() + "\n"; //Record the vertical amplitude of the micro-saccade
    }

    #endregion

    #region Eye Properties

    private bool IsEyesClosed()
    {
        PXR_EyeTracking.GetLeftEyeGazeOpenness(out float leftEyeOpenness);
        PXR_EyeTracking.GetRightEyeGazeOpenness(out float rightEyeOpenness);

        if(leftEyeOpenness <= blinkClosedThreshold && rightEyeOpenness <= blinkClosedThreshold) //Eye closed detected, check if min duration matches 50ms
        {
            return true;
        }
        else
        {
            return false;
        }
    }

    private float GetAngularVelocity()
    {
        float angleVelocity = 0;
        float deltaTime = Time.deltaTime;

        if (_previousGaze != Vector3.zero && deltaTime > 0) 
        {
            float dotProduct = Vector3.Dot(_previousGaze.normalized, _currentGaze.normalized);
            dotProduct = Mathf.Clamp(dotProduct, -1.0f, 1.0f);

            float angleRadians = Mathf.Acos(dotProduct);
            float angleDegrees = Mathf.Rad2Deg * angleRadians;

            angleVelocity = angleDegrees / deltaTime; // Convert to velocity
        }

        //Compute acceleration
        _gazeAcceleration = (angleVelocity - _previousVelocity) / deltaTime;
        _previousVelocity = angleVelocity;

        return angleVelocity;
    }

    private float Calculate_saccadeAmplitude()
    {
        float dotProduct = Vector3.Dot(_previousGaze.normalized, _currentGaze.normalized);
        dotProduct = Mathf.Clamp(dotProduct, -1.0f, 1.0f);

        float angleRadians = Mathf.Acos(dotProduct);
        float angleDegrees = Mathf.Rad2Deg * angleRadians;

        return angleDegrees;
    }

    private (float horizontalAmplitude, float verticalAmplitude) Calculate_microSaccadeAmplitude()
    {
        Vector2 start2D = new Vector2(_previousGaze.x, _previousGaze.y).normalized;
        Vector2 end2D = new Vector2(_currentGaze.x, _currentGaze.y).normalized;

        float horizontalDiff = Mathf.Atan2(end2D.x, 1) - Mathf.Atan2(start2D.x, 1);
        float verticalDiff = Mathf.Atan2(end2D.y, 1) - Mathf.Atan2(start2D.y, 1);

        float horizontalAmplitude = Mathf.Abs(Mathf.Rad2Deg * horizontalDiff);
        float verticalAmplitude = Mathf.Abs(Mathf.Rad2Deg * verticalDiff);

        return (horizontalAmplitude, verticalAmplitude);
    }


    private EyeFeature GetEyeFeature(FeatureType featureType)
    {
        foreach(EyeFeature feature in _eyeFeatures)
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

    public void StartSession()
    {
        _sessionTimer.Reset();
        for(int i = 0; i < 2; i++)
        {
            _featureDurationArray[i] = "";
            _featureCountPerSecond[i] = 0;
        }

        _microSaccadePeakVelocityText = "";
        _microSaccadePeakVelocity = 0;
        _previousVelocity = 0;
        microSaccadeDirectionText = "";
        microSaccadeHorizontalAmplitudeText = "";
        microSaccadeVerticalAmplitudeText = "";

        saccadeDirectionText = "";

        _previousGaze = Vector3.zero;
        _currentGaze = Vector3.zero;

        _sessionTimer.Start();
        sessionNumber++;

        StartCoroutine(PerSecondTimer());
    }

    public void EndSession()
    {
        //Write data to files
        _sessionTimer.Stop();
        float sessionDuration = (float)_sessionTimer.Elapsed.TotalSeconds; //Total time of session in seconds
        
        SaveManager.WriteFile(sessionDuration.ToString(), "ET_Data_S" + sessionNumber.ToString() + "_" + "SessionDuration" + ".txt");

        SaveManager.WriteFile(_featureDurationArray[(int)FeatureType.Fixation], "ET_Data_S" + sessionNumber.ToString() + "_" + "FixationsDuration" + ".txt");
        SaveManager.WriteFile(_featurePerSecondArray[(int)FeatureType.Fixation], "ET_Data_S" + sessionNumber.ToString() + "_" + "FixationsPerSecond" + ".txt");
        
        SaveManager.WriteFile(_featurePerSecondArray[(int)FeatureType.MicroSaccade], "ET_Data_S" + sessionNumber.ToString() + "_" + "MicroSaccadesPerSecond" + ".txt");
        SaveManager.WriteFile(_microSaccadePeakVelocityText, "ET_Data_S" + sessionNumber.ToString() + "_" + "MicroSaccadePeakVelocity" + ".txt");
        SaveManager.WriteFile(microSaccadeDirectionText, "ET_Data_S" + sessionNumber.ToString() + "_" + "MicroSaccadeDirection" + ".txt");
        SaveManager.WriteFile(microSaccadeHorizontalAmplitudeText, "ET_Data_S" + sessionNumber.ToString() + "_" + "MicroSaccadeHorizontalAmplitude" + ".txt");
        SaveManager.WriteFile(microSaccadeVerticalAmplitudeText, "ET_Data_S" + sessionNumber.ToString() + "_" + "MicroSaccadeVerticalAmplitude" + ".txt");

        SaveManager.WriteFile(_featurePerSecondArray[(int)FeatureType.Saccade], "ET_Data_S" + sessionNumber.ToString() + "_" + "SaccadesPerSecond" + ".txt");
        SaveManager.WriteFile(_featureDurationArray[(int)FeatureType.Saccade], "ET_Data_S" + sessionNumber.ToString() + "_" + "SaccadesDuration" + ".txt");
        SaveManager.WriteFile(saccadeDirectionText, "ET_Data_S" + sessionNumber.ToString() + "_" + "SaccadeDirection" + ".txt");

        SaveManager.WriteFile(_featurePerSecondArray[(int)FeatureType.Blink], "ET_Data_S" + sessionNumber.ToString() + "_" + "BlinksPerSecond" + ".txt");
        SaveManager.WriteFile(_featureDurationArray[(int)FeatureType.Blink], "ET_Data_S" + sessionNumber.ToString() + "_" + "BlinksDuration" + ".txt");
    }

    private IEnumerator PerSecondTimer() //For recording events every second that the session is active
    {
        yield return new WaitForSeconds(1);

        for(int i = 0; i < 4; i++)
        {
            _featurePerSecondArray[i] += _featureCountPerSecond[0].ToString() + "\n";
            _featureCountPerSecond[i] = 0;
        }

        if(_sessionTimer.IsRunning)
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