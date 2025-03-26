using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Experimental.GlobalIllumination;

public class ETCube : ETObject
{
    public Color HighLightColor;

    private Color originColor;
    private Renderer targetRenderer;

    private void Start()
    {
        if (gameObject.TryGetComponent<Renderer>(out targetRenderer))
        {
            originColor = targetRenderer.material.color;
        }
    }

    public override void IsFocused()
    {
        base.IsFocused();
        targetRenderer.material.color = HighLightColor;
    }

    public override void UnFocused()
    {
        base.UnFocused();
        targetRenderer.material.color = originColor;
    }
}
