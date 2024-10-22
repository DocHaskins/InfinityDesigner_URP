using UnityEngine;
using System.Collections;
using System.Collections.Generic;

/// <summary>
/// The LightToggle script dims specified HDRP lights to a lower intensity and toggles other lights on or off when the space key is pressed. 
/// It preserves the original intensities of the excluded lights to restore them later. 
/// This functionality allows for dynamic lighting changes in the scene, enhancing visual effects or signaling events within a Unity game developed using HDRP.
/// </summary>

namespace doppelganger
{
    [System.Serializable]
    public class ExcludedLightSetting
    {
        public Light light; // Reference to the excluded light
        public float targetIntensity = 0.25f; // The intensity to apply when fading out
    }

    public class LightToggle : MonoBehaviour
    {
        [Tooltip("Lights in this list will have their intensity set to a custom value when space is pressed.")]
        public ExcludedLightSetting[] excludedLights; // Array to hold lights and their custom fade intensity

        private Dictionary<Light, float> originalIntensities = new Dictionary<Light, float>();
        private bool isDimmed = false;

        void Start()
        {
            // Store the original intensities of all lights in the scene, not just the excluded ones
            Light[] lights = FindObjectsOfType<Light>();

            foreach (Light light in lights)
            {
                if (light != null && !originalIntensities.ContainsKey(light))
                {
                    originalIntensities[light] = light.intensity;
                }
            }
        }

        private IEnumerator FadeOut(Light light, float targetIntensity, float duration)
        {
            float startIntensity = light.intensity;
            float elapsed = 0f;

            while (elapsed < duration)
            {
                elapsed += Time.deltaTime;
                light.intensity = Mathf.Lerp(startIntensity, targetIntensity, elapsed / duration);
                yield return null;
            }
            light.intensity = targetIntensity; // Ensure it ends at the target value
        }

        private IEnumerator FadeIn(Light light, float duration)
        {
            float startIntensity = light.intensity;
            float originalIntensity = originalIntensities[light];
            float elapsed = 0f;

            while (elapsed < duration)
            {
                elapsed += Time.deltaTime;
                light.intensity = Mathf.Lerp(startIntensity, originalIntensity, elapsed / duration);
                yield return null;
            }
            light.intensity = originalIntensity; // Ensure it ends at the original value
        }

        void Update()
        {
            // Check if the space bar was pressed down this frame
            if (Input.GetKeyDown(KeyCode.Space))
            {
                // Toggle the state between dimmed and original intensity
                isDimmed = !isDimmed;

                // Fade out or in all lights in the scene
                Light[] lights = FindObjectsOfType<Light>();

                if (isDimmed)
                {
                    // Fade out all lights
                    foreach (Light light in lights)
                    {
                        bool isExcluded = false;
                        foreach (ExcludedLightSetting setting in excludedLights)
                        {
                            if (setting.light == light)
                            {
                                isExcluded = true;
                                StartCoroutine(FadeOut(light, setting.targetIntensity, 1f)); // Apply custom fade for excluded lights
                                break;
                            }
                        }

                        if (!isExcluded)
                        {
                            StartCoroutine(FadeOut(light, 0f, 1f)); // Fade out other lights to 0 intensity
                        }
                    }
                }
                else
                {
                    // Fade in all lights
                    foreach (Light light in lights)
                    {
                        if (originalIntensities.ContainsKey(light))
                        {
                            StartCoroutine(FadeIn(light, 1f)); // Restore original intensity for all lights
                        }
                    }
                }
            }
        }
    }
}