using UnityEngine;

/// <summary>Debug: logs the contacts of the object it is attached to (who touches it, normal and impulse).</summary>
public class RGContactLog : MonoBehaviour
{
    int budget = 120;

    void OnCollisionEnter(Collision c) { Log("enter", c); }
    void OnCollisionStay(Collision c) { if (Time.frameCount % 3 == 0) Log("stay", c); }

    void Log(string kind, Collision c)
    {
        if (RGTrigger.T0 < 0f || budget <= 0) return;
        // only the final phase: the grey ball is close to the leg
        var ball = GameObject.Find("BolaGris");
        if (ball == null || ball.transform.position.x < 4.8f) return;
        budget--;
        var cp = c.GetContact(0);
        Debug.Log($"RGCONTACT t={Time.time - RGTrigger.T0:F2} {name}<-{c.gameObject.name} {kind} n=({cp.normal.x:F2},{cp.normal.y:F2}) p=({cp.point.x:F3},{cp.point.y:F3}) imp={c.impulse.magnitude:F4} relv=({c.relativeVelocity.x:F2},{c.relativeVelocity.y:F2}) sep={cp.separation:F4}");
    }
}
