using TMPro;
using UnityEngine;

public class FPSUpdater : MonoBehaviour
{
    [SerializeField] private TextMeshProUGUI fps_tmp;
    [SerializeField] private float updateInterval = 1f;
    [SerializeField] private float targetFps = 60f;
    private int frameCount;
    private float timeAccumulator;

    void Start()
    {
        UpdateFps();
    }

    void Update()
    {
        frameCount++;
        timeAccumulator += Time.deltaTime;

        if (timeAccumulator >= updateInterval)
        { 
            UpdateFps();
            timeAccumulator = 0f;
            frameCount = 0;
        }

    }

    void UpdateFps()
    {
        float averageFps = frameCount / timeAccumulator;
        fps_tmp.text = $"FPS: {averageFps:F0}";

        if (averageFps >= targetFps * 0.8f) // above 80% performance
        {
            fps_tmp.color = Color.green;
        }
        else
        if (averageFps <= targetFps * 0.2f) // below 20% performance
        {
            fps_tmp.color = Color.red;
        }
        else // medium performance
        {
            fps_tmp.color = Color.yellow;
        }

    }
}
