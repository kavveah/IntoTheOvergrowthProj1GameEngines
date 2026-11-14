using UnityEngine;
using UnityEngine.Rendering;

public class ScreenFlash : MonoBehaviour
{
    private Volume volume;
    private float flashTimer;
    public float flashDuration = 0.5f; 

    private void Start()
    {
        
        volume = GetComponent<Volume>();
        if (volume != null)
        {
            volume.weight = 0f;
        }
    }

    private void Update()
    {
        if (flashTimer > 0f)
        {
            flashTimer -= Time.unscaledDeltaTime; 
            if (volume != null)
            {
                volume.weight = Mathf.Lerp(0f, 1f, flashTimer / flashDuration);
            }
        }
        else if (volume != null && volume.weight > 0f)
        {
            volume.weight = 0f; 
        }
    }
    
    public void Flash()
    {
        flashTimer = flashDuration;
        if (volume != null)
        {
            volume.weight = 1f;
        }
    }
}