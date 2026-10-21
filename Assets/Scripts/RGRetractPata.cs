using UnityEngine;

/// <summary>
/// Retracts the leg once the trapdoor starts to fall: its collisions switch off and it drops through the table,
/// so it cannot stand in the sugar cube's path.
/// </summary>
public class RGRetractPata : MonoBehaviour
{
    public Transform trampilla;
    public Rigidbody pata;
    bool hecho;

    void FixedUpdate()
    {
        if (hecho || RGTrigger.T0 < 0f || trampilla == null || pata == null) return;
        if (Mathf.DeltaAngle(0f, trampilla.eulerAngles.z) > -2f) return;
        hecho = true;
        pata.detectCollisions = false;
        Destroy(pata.gameObject, 2f);
        Debug.Log($"Pata retraida (t={Time.time - RGTrigger.T0:F2}s)");
    }
}
