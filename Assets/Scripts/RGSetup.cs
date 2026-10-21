using UnityEngine;

// Attaches the trigger and the stage markers when Play starts, so the saved scene never holds
// references to these scripts (those references broke when the scene was reloaded).
public static class RGSetup
{
    // stage, object that carries the marker, object whose first hit counts
    static readonly string[][] Stages =
    {
        new[] { "2", "PlataformaA", "BolaRoja" },
        new[] { "3", "Domino1", "BolaRoja" },
        new[] { "4", "BolitaVerde", "Domino6" },
        new[] { "6", "RampaSalida", "BolitaVerde" },
        new[] { "7", "Pendulo", "BolitaVerde" },
        new[] { "8", "Carrito", "Pendulo" },
        new[] { "9", "Bloque", "Carrito" },
        new[] { "10", "BolaGris", "Bloque" },
        new[] { "10a", "RampaZ1", "BolaGris" },
        new[] { "10b", "RampaZ2", "BolaGris" },
        new[] { "10c", "RampaZ3", "BolaGris" },
        new[] { "11", "Pata", "BolaGris" },
        new[] { "12", "TazaCafe", "CuboAzucar" },
    };

    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterSceneLoad)]
    static void Attach()
    {
        var root = GameObject.Find("RubeGoldberg");
        if (root == null) return;

        var ball = Find(root, "BolaRoja");
        if (ball != null) ball.AddComponent<RGTrigger>();

        foreach (var s in Stages)
        {
            var go = Find(root, s[1]);
            if (go == null) { Debug.LogWarning($"RGSetup: no existe {s[1]} (etapa {s[0]})."); continue; }
            var m = go.AddComponent<RGStepMarker>();
            m.etapa = s[0];
            m.golpeadoPor = new[] { s[2] };
        }

        // PhysX defaults suit meter-sized objects. This scene has 2-5 cm parts and balls that spin fast.
        foreach (var rb in root.GetComponentsInChildren<Rigidbody>())
        {
            rb.maxAngularVelocity = 100f;   // default 7 rad/s: a 5 cm ball at 1 m/s needs 40
            rb.solverIterations = 30;
            rb.solverVelocityIterations = 10;
            rb.sleepThreshold = 0f;
        }
        foreach (var c in root.GetComponentsInChildren<Collider>()) c.contactOffset = 0.002f;   // default 0.01

        new GameObject("RGLog").AddComponent<RGLog>();

        var tr = Find(root, "Trampilla");
        var pt = Find(root, "Pata");
        if (tr != null && pt != null)
        {
            var r = new GameObject("RGRetractPata").AddComponent<RGRetractPata>();
            r.trampilla = tr.transform;
            r.pata = pt.GetComponent<Rigidbody>();
        }
        var pata = Find(root, "Pata");
        if (pata != null) pata.AddComponent<RGContactLog>();
        var tramp = Find(root, "Trampilla");
        if (tramp != null) tramp.AddComponent<RGContactLog>();

        var zone = new GameObject("ZonaEmbudo");
        zone.transform.SetParent(root.transform, false);
        zone.transform.localPosition = new Vector3(2.30f, 0.85f, 0f);
        var box = zone.AddComponent<BoxCollider>();
        box.isTrigger = true;
        box.size = new Vector3(0.20f, 0.20f, 0.30f);
        var z = zone.AddComponent<RGZoneMarker>();
        z.etapa = "5";
        z.cuerpo = "BolitaVerde";
    }

    static GameObject Find(GameObject root, string name)
    {
        var t = root.transform.Find(name);
        return t != null ? t.gameObject : null;
    }
}
