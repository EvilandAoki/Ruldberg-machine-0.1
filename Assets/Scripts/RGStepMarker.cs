using UnityEngine;

/// <summary>Logs the stage the first time one of the expected objects touches this one after the release.</summary>
public class RGStepMarker : MonoBehaviour
{
    public string etapa;
    public string[] golpeadoPor;
    bool hecho;

    void OnCollisionEnter(Collision c)
    {
        if (hecho || RGTrigger.T0 < 0f) return;
        Rigidbody other = c.collider.attachedRigidbody;
        string n = other != null ? other.gameObject.name : c.gameObject.name;
        foreach (string g in golpeadoPor)
        {
            if (n != g) continue;
            hecho = true;
            Debug.Log($"Etapa {etapa} activada (t={Time.time - RGTrigger.T0:F2}s)");
            return;
        }
    }
}
