using UnityEngine;
using System.Collections;
namespace UnityStandardAssets.ImageEffects
{
    [ExecuteInEditMode]
    [AddComponentMenu("Image Effects/Color Adjustments/Fog")]
    public class FogOfWar : ImageEffectBase
    {
        public Texture mask;
        public Texture playerViewTexture;

        // Called by camera to apply image effect
        void OnRenderImage(RenderTexture source, RenderTexture destination)
        {
            material.SetTexture("_MaskTex", mask);
            material.SetTexture("_PlayerViewTex", playerViewTexture);
            Graphics.Blit(source, destination, material);
        }
    }
}