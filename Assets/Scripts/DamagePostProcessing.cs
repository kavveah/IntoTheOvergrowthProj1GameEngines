using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;

public class DamagePostProcessing : MonoBehaviour
{
    public static DamagePostProcessing Instance;

    private Volume volume;
    private float flashTimer;
    public float flashDuration = 0.5f;

    private void Awake()
    {
        // Singleton setzen + Volume vorbereiten
        Instance = this;
        EnsureSetup();
        if (volume != null) volume.weight = 0f;
    }

    private void EnsureSetup()
    {
        Component oldFlash = GetComponent("ScreenFlash");
        if (oldFlash != null) Destroy(oldFlash);

        gameObject.layer = 0;

        volume = GetComponent<Volume>();
        if (volume == null)
        {
            volume = gameObject.AddComponent<Volume>();
        }

        volume.isGlobal = true;
        volume.priority = 999f; //damit immer sichtbar

        if (volume.profile == null)
        {
            volume.profile = ScriptableObject.CreateInstance<VolumeProfile>();
        }

        // rote Vignette für den Schadens-Effekt hinzufügen(für lvl1 weil dauerhaft fehler)
        if (!volume.profile.TryGet<Vignette>(out var vignette))
        {
            vignette = volume.profile.Add<Vignette>(true);
        }
        vignette.active = true;
        vignette.color.Override(Color.red);
        vignette.intensity.Override(0.55f);

        if (Camera.main != null)
        {
            var camData = Camera.main.GetComponent<UniversalAdditionalCameraData>();
            if (camData != null)
            {
                camData.renderPostProcessing = true;
                camData.volumeLayerMask = ~0; 
            }
        }
    }

    private void Update()
    {
        // flash langsam ausblenden nach treffer
        if (flashTimer > 0f)
        {
            flashTimer -= Time.unscaledDeltaTime;
            if (volume != null)
            {
                volume.weight = Mathf.Lerp(0f, 1f, flashTimer / flashDuration);
            }
        }
        // sicherstellen dass der Effekt komplett aus ist wenn der Timer abgelaufen ist
        else if (volume != null && volume.weight > 0f)
        {
            volume.weight = 0f;
        }
    }
    public void Flash()
    {
        EnsureSetup();
        flashTimer = flashDuration;
        if (volume != null) volume.weight = 1f;
    }
}