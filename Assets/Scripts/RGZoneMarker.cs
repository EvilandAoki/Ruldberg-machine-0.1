using UnityEngine;

/// <summary>Stage 5: a trigger volume over the funnel; BolitaVerde crosses it without touching the walls.</summary>
public class RGZoneMarker : MonoBehaviour
{
    public string etapa;
    public string cuerpo;
    bool hecho;

    void OnTriggerEnter(Collider other)
    {
        if (hecho || RGTrigger.T0 < 0f) return;
        Rigidbody rb = other.attachedRigidbody;
        if (rb == null || rb.gameObject.name != cuerpo) return;
        hecho = true;
        Debug.Log($"Etapa {etapa} activada (t={Time.time - RGTrigger.T0:F2}s)");
    }
}
