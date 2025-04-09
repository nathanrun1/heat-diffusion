using Blizzard.Temperature;
using UnityEngine;
using UnityEngine.UI;

public class RenderComputeTexture : MonoBehaviour
{
    [SerializeField] private TemperatureCompute compute;
    [SerializeField] private Image image;

    // Update is called once per frame
    void Update()
    {
        if (compute.renderTexture != null) image.material.SetTexture("_MainTex", compute.renderTexture);
    }
}
