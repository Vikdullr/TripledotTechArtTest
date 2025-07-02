using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Events;
using TMPro;

public class StarCollectionController : MonoBehaviour
{
    [Header("Particle Settings")]
    public ParticleSystem controlledParticleSystem;
    public int particlesToSpawn = 100;

    [Header("Growth Targets")]
    public GameObject objectToScale;
    public Image imageToFadeIn;
    public TextMeshProUGUI amountText;

    [Header("Growth Settings")]
    public int maxAmountForFullGrowth = 100;
    public Vector3 startScale = Vector3.one;
    public Vector3 finalScale = new Vector3(2, 2, 2);
    public AnimationCurve scaleCurve = AnimationCurve.Linear(0, 0, 1, 1);
    
    [Header("Fade Settings")]
    [Range(0, 1)] public float startAlpha = 0f;
    [Range(0, 1)] public float finalAlpha = 1.0f;
    public AnimationCurve fadeCurve = AnimationCurve.Linear(0, 0, 1, 1);
    
    [Header("Events")]
    public UnityEvent onMaxGrowthReached;
    public UnityEvent onEmissionComplete;

    private int currentAmount = 0;
    private Coroutine emissionCoroutine;

    void OnEnable()
    {
        currentAmount = 0;
        if (emissionCoroutine != null)
        {
            StopCoroutine(emissionCoroutine);
            emissionCoroutine = null;
        }
        
        ApplyInitialValues();
        ApplyGrowth();
        SetupParticleSystem();
        Play();
    }

    private void SetupParticleSystem()
    {
        if (controlledParticleSystem == null) return;
        
        controlledParticleSystem.Stop(true, ParticleSystemStopBehavior.StopEmittingAndClear);

        var main = controlledParticleSystem.main;
        main.playOnAwake = false;
        var emission = controlledParticleSystem.emission;
        emission.enabled = false;
    }
    
    public void Play()
    {
        if (controlledParticleSystem == null) return;

        if (emissionCoroutine != null)
        {
            StopCoroutine(emissionCoroutine);
        }
        emissionCoroutine = StartCoroutine(EmitParticles());
    }
    
    public void AddGrowthUnit()
    {
        currentAmount++;
        ApplyGrowth();

        if (currentAmount == maxAmountForFullGrowth)
        {
            onMaxGrowthReached.Invoke();
        }
    }

    private void ApplyInitialValues()
    {
        if (objectToScale != null) objectToScale.transform.localScale = startScale;
        
        if (imageToFadeIn != null)
        {
            Color c = imageToFadeIn.color;
            imageToFadeIn.color = new Color(c.r, c.g, c.b, startAlpha);
        }
    }
    
    private void ApplyGrowth()
    {
        float growthRatio = (float)currentAmount / maxAmountForFullGrowth;
        float clampedRatio = Mathf.Clamp01(growthRatio);

        if (objectToScale != null)
        {
            float scaleEase = scaleCurve.Evaluate(clampedRatio);
            objectToScale.transform.localScale = Vector3.LerpUnclamped(startScale, finalScale, scaleEase);
        }

        if (imageToFadeIn != null)
        {
            float fadeEase = fadeCurve.Evaluate(clampedRatio);
            float newAlpha = Mathf.LerpUnclamped(startAlpha, finalAlpha, fadeEase);
            Color c = imageToFadeIn.color;
            imageToFadeIn.color = new Color(c.r, c.g, c.b, newAlpha);
        }

        if (amountText != null)
        {
            amountText.text = currentAmount.ToString();
        }
    }

    private IEnumerator EmitParticles()
    {
        controlledParticleSystem.Clear();
        controlledParticleSystem.Play();
        
        var emissionModule = controlledParticleSystem.emission;
        emissionModule.enabled = true;

        float duration = controlledParticleSystem.main.duration;
        float originalTotal = CalculateTotalEmission(controlledParticleSystem);
        float scaleFactor = (originalTotal > 0) ? (float)particlesToSpawn / originalTotal : 0;

        var rateOverTime = emissionModule.rateOverTime;
        float rateMultiplier = rateOverTime.curveMultiplier * scaleFactor;
        AnimationCurve curve = rateOverTime.curve;
        
        var bursts = new ParticleSystem.Burst[emissionModule.burstCount];
        emissionModule.GetBursts(bursts);
        var burstsToProcess = bursts.OrderBy(b => b.time).ToList();

        int particlesEmittedTotal = 0;
        float emissionTime = 0f;
        float emissionRateAccumulator = 0f;

        while (emissionTime < duration && particlesEmittedTotal < particlesToSpawn)
        {
            emissionTime += Time.deltaTime;
            float normalizedTime = emissionTime / duration;
            float frameParticles = 0;

            emissionRateAccumulator += curve.Evaluate(normalizedTime) * rateMultiplier * Time.deltaTime;
            frameParticles += Mathf.FloorToInt(emissionRateAccumulator);
            emissionRateAccumulator -= Mathf.FloorToInt(emissionRateAccumulator);

            while (burstsToProcess.Any() && burstsToProcess[0].time <= emissionTime)
            {
                frameParticles += burstsToProcess[0].count.constant * scaleFactor;
                burstsToProcess.RemoveAt(0);
            }

            int particlesToEmit = Mathf.RoundToInt(frameParticles);
            if (particlesToEmit > 0)
            {
                int remainingParticles = particlesToSpawn - particlesEmittedTotal;
                int finalEmitCount = Mathf.Min(particlesToEmit, remainingParticles);

                controlledParticleSystem.Emit(finalEmitCount);
                particlesEmittedTotal += finalEmitCount;
            }
            yield return null;
        }
        
        controlledParticleSystem.Stop(true, ParticleSystemStopBehavior.StopEmitting);
        onEmissionComplete.Invoke();
    }
    
    private static float CalculateTotalEmission(ParticleSystem system)
    {
        var emission = system.emission;
        emission.enabled = true;
        
        float duration = system.main.duration;
        float totalParticles = 0;

        totalParticles += CalculateRateOverTime(emission, duration);
        totalParticles += CalculateBursts(emission);
        
        emission.enabled = false;
        return totalParticles;
    }
    
    private static float CalculateRateOverTime(ParticleSystem.EmissionModule emission, float duration)
    {
        var rateOverTime = emission.rateOverTime;
        if (rateOverTime.mode == ParticleSystemCurveMode.Constant)
        {
            return rateOverTime.constant * duration;
        }

        if (rateOverTime.mode == ParticleSystemCurveMode.Curve)
        {
            AnimationCurve rateCurve = rateOverTime.curve;
            float multiplier = rateOverTime.curveMultiplier;
            float rateParticles = 0;
            int sampleCount = 100;
            float timeStep = duration / sampleCount;

            for (int i = 0; i < sampleCount; i++)
            {
                float rate = rateCurve.Evaluate((float)i / sampleCount) * multiplier;
                rateParticles += rate * timeStep;
            }
            return rateParticles;
        }
        return 0;
    }

    private static float CalculateBursts(ParticleSystem.EmissionModule emission)
    {
        float burstParticles = 0;
        for (int i = 0; i < emission.burstCount; i++)
        {
            var burst = emission.GetBurst(i);
            float avgCount = (burst.count.constantMin + burst.count.constantMax) / 2f;
            burstParticles += avgCount * burst.cycleCount;
        }
        return burstParticles;
    }
}