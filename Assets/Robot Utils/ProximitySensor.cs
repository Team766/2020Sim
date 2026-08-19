using System.Collections.Generic;
using Team766.Simulator;
using UnityEngine;

/// <summary>
/// Simulates a time-of-flight distance sensor (e.g. VL53L1X-style) with a conical
/// field of view. Reports the minimum distance to any surface within that cone.
///
/// Performance notes:
///  - Ray directions are precomputed once and cached (no per-frame allocation).
///  - Sampling runs on a timer, not every Update(), since real ToF sensors only
///    refresh at 15-60 Hz anyway. This is the biggest lever if you have many sensors.
///  - Use the LayerMask to exclude anything the sensor shouldn't "see" (triggers,
///    VFX, UI, etc.) - fewer candidate colliders = cheaper raycasts.
/// </summary>
[DisallowMultipleComponent]
public class ProximitySensor : RobotSensor
{
    [Header("Sensor Geometry")]
    [Tooltip("Full field of view angle, in degrees. 27 matches many real ToF modules.")]
    [Range(1f, 90f)] public float fieldOfViewDegrees = 27f;

    [Tooltip("Maximum sensing range. Objects beyond this simply won't be detected.")]
    public float maxDistance = 4f;

    [Header("Sampling (accuracy vs. performance)")]
    [Tooltip("Number of concentric rings sampled inside the cone, in addition to the center ray.")]
    [Range(0, 8)] public int sampleRings = 3;

    [Tooltip("Rays per ring. Total ray count = 1 + sampleRings * raysPerRing.")]
    [Range(1, 16)] public int raysPerRing = 6;

    [Header("Update Rate")]
    [Tooltip("How many times per second the sensor takes a reading. Real ToF sensors are typically 15-60 Hz.")]
    [Range(1f, 120f)] public float updateRateHz = 30f;

    [Header("Physics")]
    public LayerMask detectionMask = ~0;
    public QueryTriggerInteraction triggerInteraction = QueryTriggerInteraction.Ignore;

    [Header("Debug")]
    public bool drawGizmos = true;
    public Color gizmoColor = new Color(0f, 1f, 0.4f, 0.6f);

    /// <summary>Latest measured distance. Equals maxDistance when nothing is detected.</summary>
    public float CurrentDistance { get; private set; }

    /// <summary>True if the last reading hit something within range.</summary>
    public bool HasTarget { get; private set; }

    Vector3[] _localDirections; // cached, sensor-local space (forward-aligned)
    float _timer;
    float _interval;

    void Awake()
    {
        BuildSamplePattern();
        CurrentDistance = maxDistance;
        // Stagger the initial timer per-instance so many sensors don't all sample on the same frame.
        _timer = Random.Range(0f, 1f / Mathf.Max(1f, updateRateHz));
    }

    void OnValidate()
    {
        // Rebuild in editor when tweaking values, so gizmos stay accurate.
        BuildSamplePattern();
    }

    void BuildSamplePattern()
    {
        var dirs = new List<Vector3>(1 + sampleRings * raysPerRing) { Vector3.forward };

        float halfAngle = fieldOfViewDegrees * 0.5f;
        for (int ring = 1; ring <= sampleRings; ring++)
        {
            // sqrt spacing => rings cover roughly equal solid angle rather than
            // bunching samples near the cone axis.
            float ringAngle = halfAngle * Mathf.Sqrt(ring / (float)sampleRings);
            float azimuthOffset = ring * (180f / Mathf.Max(1, raysPerRing)); // stagger rings for coverage

            for (int i = 0; i < raysPerRing; i++)
            {
                float azimuth = (360f / raysPerRing) * i + azimuthOffset;
                Quaternion rot = Quaternion.AngleAxis(azimuth, Vector3.forward) * Quaternion.AngleAxis(ringAngle, Vector3.up);
                dirs.Add(rot * Vector3.forward);
            }
        }

        _localDirections = dirs.ToArray();
        _interval = 1f / Mathf.Max(1f, updateRateHz);
    }

    void Update()
    {
        _timer += Time.deltaTime;
        if (_timer >= _interval)
        {
            _timer -= _interval;
            Sample();
        }
    }

    void Sample()
    {
        float min = maxDistance;
        bool hitAnything = false;

        for (int i = 0; i < _localDirections.Length; i++)
        {
            Vector3 dir = transform.TransformDirection(_localDirections[i]);
            if (Physics.Raycast(transform.position, dir, out RaycastHit hit, maxDistance, detectionMask, triggerInteraction))
            {
                hitAnything = true;
                if (hit.distance < min)
                    min = hit.distance;
            }
        }

        CurrentDistance = min;
        HasTarget = hitAnything;
    }

    /// <summary>Force an immediate reading, bypassing the update-rate timer.</summary>
    public float SampleNow()
    {
        Sample();
        _timer = 0f;
        return CurrentDistance;
    }

    void OnDrawGizmosSelected()
    {
        if (!drawGizmos) return;

        if (_localDirections == null || _localDirections.Length == 0)
            BuildSamplePattern();

        Gizmos.color = gizmoColor;
        foreach (var localDir in _localDirections)
        {
            Vector3 dir = transform.TransformDirection(localDir);
            Gizmos.DrawRay(transform.position, dir * maxDistance);
        }

        // Highlight the last measured hit distance along the center axis.
        if (Application.isPlaying)
        {
            Gizmos.color = HasTarget ? Color.red : Color.gray;
            Gizmos.DrawWireSphere(transform.position + transform.forward * CurrentDistance, 0.03f);
        }
    }

    public override void UpdateSensorValue(SensorProto value)
    {
        value.Proximity = new() { Distance = CurrentDistance };
    }
}