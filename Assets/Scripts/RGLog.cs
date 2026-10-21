using System.Text;
using UnityEngine;

/// <summary>Writes one compact line every 0.5 s with the state of the key bodies (read it in the Console or Editor.log).</summary>
public class RGLog : MonoBehaviour
{
    static readonly string[] Names =
    {
        "BolaRoja", "Domino1", "Domino2", "Domino3", "Domino4", "Domino5", "Domino6", "BolitaVerde",
        "Pendulo", "Carrito", "Bloque", "BolaGris", "Pata", "Trampilla", "CuboAzucar",
    };

    Transform root;
    Rigidbody[] bodies;
    float next;

    void Start()
    {
        root = GameObject.Find("RubeGoldberg").transform;
        bodies = new Rigidbody[Names.Length];
        for (int i = 0; i < Names.Length; i++)
        {
            var t = root.Find(Names[i]);
            if (t != null) bodies[i] = t.GetComponent<Rigidbody>();
        }
    }

    void FixedUpdate()
    {
        if (RGTrigger.T0 < 0f) return;
        float t = Time.time - RGTrigger.T0;
        if (t < next || t > 30f) return;
        next += 0.5f;
        var sb = new StringBuilder($"RGLOG t={t:F1}");
        for (int i = 0; i < Names.Length; i++)
        {
            var rb = bodies[i];
            if (rb == null) continue;
            Vector3 p = rb.position;
#if UNITY_6000_0_OR_NEWER
            Vector3 v = rb.linearVelocity;
#else
            Vector3 v = rb.velocity;
#endif
            sb.Append($" | {Names[i]} {p.x:F3},{p.y:F3} r{rb.rotation.eulerAngles.z:F0} v{v.x:F2},{v.y:F2}{(rb.IsSleeping() ? " zZ" : "")}");
        }
        Debug.Log(sb.ToString());
    }
}
