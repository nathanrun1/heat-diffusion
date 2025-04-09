using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class FPSCounter : MonoBehaviour
{
    [SerializeField] private TextMeshProUGUI _fpsText;
    [SerializeField] private float _updateDelay = 0.5f;

    private float _timeSinceUpdate = 0.0f;

    // Update is called once per frame
    void Update()
    {
        _timeSinceUpdate += Time.unscaledDeltaTime;
        if (_timeSinceUpdate > _updateDelay)
        {
            int fps = Mathf.RoundToInt(1f / Time.unscaledDeltaTime);
            _timeSinceUpdate -= _updateDelay;
            _fpsText.text = $"FPS: {fps}\nFrame time: {Mathf.RoundToInt(Time.unscaledDeltaTime * 1000)}ms";
        }
    }
}
