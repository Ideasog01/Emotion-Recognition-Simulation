using System.Collections;
using System.Collections.Generic;
using Unity.XR.PXR;
using UnityEngine;
using UnityEngine.Experimental.GlobalIllumination;

public class ETCharacter : ETObject
{
    private const string isFocused_Animation = "IsFocused";

    public GameObject SpotLight;

    public override void IsFocused()
    {
        base.IsFocused();
        SpotLight.SetActive(isFocused);
        GetComponent<Animator>().SetBool(isFocused_Animation, isFocused);
    }

    public override void UnFocused()
    {
        base.UnFocused();
        SpotLight.SetActive(isFocused);
        GetComponent<Animator>().SetBool(isFocused_Animation, isFocused);
    }
}
