# Eye Tracking Unity Demo

- If you have any questions/comments, please visit [**Pico Developer Support Portal**](https://picodevsupport.freshdesk.com/support/home) and raise your question there.

## Environment：

- PUI 5.4.0
- Unity 2021.3.13f1
- Pico Unity Integration SDK 2.1.4

## Applicable devices:

- PICO 4 Pro

## Description：
To enable eye tracking feature you need to mark the Eye Tracking check box on PXR_Manager:
![Screenshot](https://github.com/picoxr/EyeTrackingDemo/blob/eb8677aca7d30c2506d2e8ab0b0ed992c00e9d8a/Screenshots/PXR_Manager.png)

- There are 3 parts in this project. A spot light is used to show an approximate eye gaze area.

1.  **User Calibration**

This part shows how to get eye-tracking data with PXR_EyeTracking.GetCombineEyeGazeVector(out combineEyeGazeVector) and PXR_EyeTracking.GetCombineEyeGazePoint(out combineEyeGazeOrigin). Then by applying the HeadPostMatrix, you can make a sphere cast to interact with 3D objects. You can also adjust eye tracking offest with trigger button on right controller.

![Screenshot](https://github.com/picoxr/EyeTrackingDemo/blob/bd7e1f592971bd35fc4fca292f05afb7add51ab5/Screenshots/Calibration.png)

2.  **3D models**

This part shows you how to detect if a 3D model with animation is focused or unfocused by eye-tracking. To create your own eye tracking interactive game object, you can simply derive from ETObject and implement IsFocused() and UnFocused().

![Screenshot](https://github.com/picoxr/EyeTrackingDemo/blob/eb8677aca7d30c2506d2e8ab0b0ed992c00e9d8a/Screenshots/3DModels.png)

3. **Avatar**

This part shows you how to get and apply eye openness to an avatar by calling PXR_EyeTracking.GetLeftEyeGazeOpenness(out leftEyeOpenness) and PXR_EyeTracking.GetRightEyeGazeOpenness(out rightEyeOpenness).

![Screenshot](https://github.com/picoxr/EyeTrackingDemo/blob/eb8677aca7d30c2506d2e8ab0b0ed992c00e9d8a/Screenshots/Avatar.png)
