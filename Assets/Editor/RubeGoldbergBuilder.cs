using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
using UnityEngine.Rendering;

// Builds the corrected Rube Goldberg machine from the mockup table.
// Menu: Tools > Rube Goldberg > Construir escena
// All positions are in meters with Unity axes. The stage scripts are attached at Play time by RGSetup.cs.
public static class RubeGoldbergBuilder
{
    const string Root = "RubeGoldberg";
    const string MatFolder = "Assets/RubeGoldberg/Materials";

    // ---------- data ----------
    class Def
    {
        public string name, parent, shape, phys, color;
        public float x, y, z, rot, sx, sy, sz, mass;
        public bool dyn, noCol, kin, translucent;
    }
    class HingeDef { public string name; public float ax, ay; public bool limits; public float min, max; }

    // flags: 1 = no collider, 2 = kinematic until released, 4 = translucent
    static Def D(string name, string shape, float x, float y, float z, float rot, float sx, float sy, float sz,
                 char kind, float mass, string phys, string color, int flags) => new Def
    {
        name = name, shape = shape, x = x, y = y, z = z, rot = rot, sx = sx, sy = sy, sz = sz,
        dyn = kind == 'D', mass = mass, phys = phys, color = color,
        noCol = (flags & 1) != 0, kin = (flags & 2) != 0, translucent = (flags & 4) != 0,
    };

    // Child of a compound object; coordinates are local to the parent.
    static Def K(string parent, string name, string shape, float x, float y, float z, float rot,
                 float sx, float sy, float sz, string color, int flags) => new Def
    {
        parent = parent, name = name, shape = shape, x = x, y = y, z = z, rot = rot, sx = sx, sy = sy, sz = sz,
        color = color, noCol = (flags & 1) != 0,
    };

    static HingeDef H(string name, float ax, float ay, bool limits, float min, float max) =>
        new HingeDef { name = name, ax = ax, ay = ay, limits = limits, min = min, max = max };

    static readonly Def[] Defs =
    {
        D("Mesa", "cube", 2.8f, -0.05f, 0.0f, 0.0f, 6.0f, 0.1f, 0.6f, 'S', 0.0f, "Medio", "#D9B88F", 0),
        D("Fondo", "cube", 2.75f, 0.8f, 0.45f, 0.0f, 6.4f, 3.0f, 0.05f, 'S', 0.0f, null, "#DDE6EE", 1),
        D("PlataformaA", "cube", 0.85f, 0.5f, 0.0f, 0.0f, 1.7f, 1.0f, 0.6f, 'S', 0.0f, "Agarre", "#CFCFCF", 0),
        D("PlataformaB", "cube", 2.8f, 0.25f, 0.0f, 0.0f, 2.2f, 0.5f, 0.6f, 'S', 0.0f, "Medio", "#CFCFCF", 0),
        D("BolaRoja", "sphere", 0.15f, 1.245f, 0.0f, 0.0f, 0.15f, 0.15f, 0.15f, 'D', 0.3f, "Medio", "#E63946", 2),
        D("RampaInicial", "cube", 0.31f, 1.08f, 0.0f, -20.0f, 0.6f, 0.05f, 0.25f, 'S', 0.0f, "Medio", "#8ECAE6", 0),
        D("SoporteRampa", "cube", 0.1f, 1.07f, 0.0f, 0.0f, 0.08f, 0.14f, 0.25f, 'S', 0.0f, null, "#CFCFCF", 0),
        D("Domino1", "cube", 0.72f, 1.05f, 0.0f, 0.0f, 0.04f, 0.1f, 0.2f, 'D', 0.02f, "Agarre", "#FFD166", 0),
        D("Domino2", "cube", 0.81f, 1.06f, 0.0f, 0.0f, 0.04f, 0.12f, 0.2f, 'D', 0.024f, "Agarre", "#FFD166", 0),
        D("Domino3", "cube", 0.91f, 1.08f, 0.0f, 0.0f, 0.04f, 0.16f, 0.2f, 'D', 0.032f, "Agarre", "#FFD166", 0),
        D("Domino4", "cube", 1.03f, 1.11f, 0.0f, 0.0f, 0.04f, 0.22f, 0.2f, 'D', 0.044f, "Agarre", "#FFD166", 0),
        D("Domino5", "cube", 1.18f, 1.15f, 0.0f, 0.0f, 0.04f, 0.3f, 0.2f, 'D', 0.06f, "Agarre", "#FFD166", 0),
        D("Domino6", "cube", 1.37f, 1.2f, 0.0f, 0.0f, 0.04f, 0.4f, 0.2f, 'D', 0.08f, "Agarre", "#FFD166", 0),
        D("BolitaVerde", "sphere", 1.55f, 1.325f, 0.0f, 0.0f, 0.05f, 0.05f, 0.05f, 'D', 0.03f, "Medio", "#06D6A0", 0),
        D("EmbudoIzq", "cube", 2.2f, 0.86f, 0.0f, -45.0f, 0.24f, 0.02f, 0.25f, 'S', 0.0f, null, "#9B5DE5", 0),
        D("EmbudoDer", "cube", 2.5f, 0.86f, 0.0f, 45.0f, 0.24f, 0.02f, 0.25f, 'S', 0.0f, null, "#9B5DE5", 0),
        D("PanelSeguridad", "cube", 2.64f, 1.23f, 0.0f, 0.0f, 0.03f, 0.45f, 0.3f, 'S', 0.0f, null, "#9B5DE5", 4),
        D("SoporteRampaSalida", "cube", 2.27f, 0.61f, 0.0f, 0.0f, 0.04f, 0.22f, 0.25f, 'S', 0.0f, null, "#CFCFCF", 0),
        D("RampaSalida", "cube", 2.5f, 0.64f, 0.0f, -20.0f, 0.5f, 0.02f, 0.25f, 'S', 0.0f, "Medio", "#9B5DE5", 0),
        D("PosteP1", "cube", 2.9f, 0.75f, -0.13f, 0.0f, 0.04f, 0.5f, 0.04f, 'S', 0.0f, null, "#CFCFCF", 0),
        D("PosteP2", "cube", 2.9f, 0.75f, 0.13f, 0.0f, 0.04f, 0.5f, 0.04f, 'S', 0.0f, null, "#CFCFCF", 0),
        D("ViguetaPortico", "cube", 2.9f, 1.02f, 0.0f, 0.0f, 0.04f, 0.04f, 0.3f, 'S', 0.0f, null, "#CFCFCF", 0),
        D("Pendulo", "group", 2.9f, 1.0f, 0.0f, 0.0f, 1.0f, 1.0f, 1.0f, 'D', 0.03f, null, "#F15BB5", 0),
        K("Pendulo", "Hilo", "cyl", 0.0f, -0.21f, 0f, 0.0f, 0.008f, 0.21f, 0.008f, "#111111", 1),
        K("Pendulo", "Bob", "sphere", 0.0f, -0.42f, 0f, 0.0f, 0.12f, 0.12f, 0.12f, "#F15BB5", 0),
        D("Riel", "cube", 3.425f, 0.5225f, 0.0f, 0.0f, 0.85f, 0.045f, 0.3f, 'S', 0.0f, "Hielo", "#2A9D8F", 0),
        D("Carrito", "cube", 3.06f, 0.595f, 0.0f, 0.0f, 0.14f, 0.1f, 0.12f, 'D', 0.1f, "Hielo", "#2A9D8F", 0),
        D("Bloque", "cube", 3.55f, 0.605f, 0.0f, 0.0f, 0.1f, 0.12f, 0.12f, 'D', 0.08f, "Hielo", "#355070", 0),
        D("BolaGris", "sphere", 3.72f, 0.585f, 0.0f, 0.0f, 0.08f, 0.08f, 0.08f, 'D', 0.25f, "Medio", "#8D99AE", 0),
        D("RampaZ1", "cube", 4.1f, 0.47f, 0.0f, -15.0f, 0.45f, 0.02f, 0.25f, 'S', 0.0f, "Medio", "#8ECAE6", 0),
        D("RampaZ2", "cube", 4.33f, 0.29f, 0.0f, 15.0f, 0.45f, 0.02f, 0.25f, 'S', 0.0f, "Medio", "#8ECAE6", 0),
        D("TopeZ2", "cube", 4.575f, 0.44f, 0.0f, 0.0f, 0.02f, 0.18f, 0.25f, 'S', 0.0f, "Medio", "#8ECAE6", 0),
        D("RampaZ3", "cube", 4.22f, 0.07f, 0.0f, -15.0f, 0.45f, 0.02f, 0.25f, 'S', 0.0f, "Medio", "#8ECAE6", 0),
        D("TopeZ3", "cube", 3.975f, 0.2f, 0.0f, 0.0f, 0.02f, 0.2f, 0.25f, 'S', 0.0f, "Medio", "#8ECAE6", 0),
        D("Trampilla", "cube", 4.925f, 0.45f, 0.0f, 0.0f, 0.45f, 0.02f, 0.22f, 'D', 0.05f, "Hielo", "#8D5A36", 0),
        D("Pata", "cube", 4.95f, 0.22f, 0.0f, 0.0f, 0.04f, 0.44f, 0.22f, 'D', 0.05f, "Pata", "#8D5A36", 0),
        D("CuboAzucar", "cube", 4.78f, 0.48f, 0.0f, 0.0f, 0.04f, 0.04f, 0.04f, 'D', 0.02f, "Liso", "#FFFFFF", 0),
        D("TazaBase", "cube", 5.3f, 0.01f, 0.0f, 0.0f, 0.16f, 0.02f, 0.2f, 'S', 0.0f, null, "#FFFFFF", 0),
        D("TazaParedIzq", "cube", 5.23f, 0.065f, 0.0f, 0.0f, 0.02f, 0.09f, 0.2f, 'S', 0.0f, null, "#FFFFFF", 0),
        D("TazaParedDer", "cube", 5.37f, 0.065f, 0.0f, 0.0f, 0.02f, 0.09f, 0.2f, 'S', 0.0f, null, "#FFFFFF", 0),
        D("TazaCafe", "cube", 5.3f, 0.05f, 0.0f, 0.0f, 0.12f, 0.06f, 0.18f, 'S', 0.0f, null, "#4B2E1E", 0),
        D("TazaAsa", "cube", 5.415f, 0.065f, 0.0f, 0.0f, 0.04f, 0.06f, 0.02f, 'S', 0.0f, null, "#FFFFFF", 1),
        D("PostePelota", "cube", 1.55f, 1.15f, 0.0f, 0.0f, 0.04f, 0.3f, 0.2f, 'S', 0.0f, null, "#CFCFCF", 0),
        D("RampaPuente", "cube", 1.845f, 1.121f, 0.0f, -32.0f, 0.64f, 0.02f, 0.25f, 'S', 0.0f, "Medio", "#8ECAE6", 0),
    };

    static readonly HingeDef[] Hinges =
    {
        H("Pendulo", 0.0f, 0.0f, false, 0.0f, 0.0f),
        H("Trampilla", -0.5f, 0.0f, true, -25.0f, 0.0f),
    };

    static readonly Dictionary<string, float> Friction = new Dictionary<string, float>
    {
        { "Pata", 0.02f }, { "Hielo", 0.0f }, { "Liso", 0.1f }, { "Medio", 0.4f }, { "Agarre", 0.7f },
    };

    // ---------- menu ----------
    [MenuItem("Tools/Rube Goldberg/Construir escena")]
    public static void Build()
    {
        var old = GameObject.Find(Root);
        if (old != null)
        {
            if (!EditorUtility.DisplayDialog("Rube Goldberg", "Ya existe una escena construida. ¿Reemplazarla?", "Reemplazar", "Cancelar")) return;
            Undo.DestroyObjectImmediate(old);
        }

        SetFixedTimestep(0.01f);
        if (AssetDatabase.IsValidFolder(MatFolder)) AssetDatabase.DeleteAsset(MatFolder);
        if (!AssetDatabase.IsValidFolder("Assets/RubeGoldberg")) AssetDatabase.CreateFolder("Assets", "RubeGoldberg");
        AssetDatabase.CreateFolder("Assets/RubeGoldberg", "Materials");
        var phys = MakePhysicMaterials();
        var colors = new Dictionary<string, Material>();

        var root = new GameObject(Root);
        Undo.RegisterCreatedObjectUndo(root, "Construir Rube Goldberg");
        var byName = new Dictionary<string, GameObject>();

        foreach (var d in Defs)
        {
            GameObject go = Create(d, colors);
            go.name = d.name;
            if (d.parent == null)
            {
                go.transform.SetParent(root.transform, false);
                byName[d.name] = go;
            }
            else
            {
                go.transform.SetParent(byName[d.parent].transform, false);
            }
            go.transform.localPosition = new Vector3(d.x, d.y, d.z);
            go.transform.localRotation = Quaternion.Euler(0f, 0f, d.rot);
            go.transform.localScale = new Vector3(d.sx, d.sy, d.sz);

            var col = go.GetComponent<Collider>();
            if (d.noCol && col != null) Object.DestroyImmediate(col);
            else if (col != null && d.phys != null) col.sharedMaterial = phys[d.phys];
        }

        // Rigidbodies after hierarchy exists so compound objects get one body on the parent.
        foreach (var d in Defs)
        {
            if (d.parent != null || !d.dyn) continue;
            var go = byName[d.name];
            var rb = go.AddComponent<Rigidbody>();
            rb.mass = d.mass;
            rb.constraints = RigidbodyConstraints.FreezePositionZ | RigidbodyConstraints.FreezeRotationX | RigidbodyConstraints.FreezeRotationY;
            rb.collisionDetectionMode = CollisionDetectionMode.ContinuousDynamic;
            rb.isKinematic = d.kin;
            if (d.phys != null)
                foreach (var c in go.GetComponentsInChildren<Collider>()) c.sharedMaterial = phys[d.phys];
        }

        foreach (var h in Hinges)
        {
            var hj = byName[h.name].AddComponent<HingeJoint>();
            hj.autoConfigureConnectedAnchor = true;
            hj.anchor = new Vector3(h.ax, h.ay, 0f);
            hj.axis = new Vector3(0f, 0f, 1f);
            if (h.limits)
            {
                hj.useLimits = true;
                hj.limits = new JointLimits { min = h.min, max = h.max };
            }
        }

        SetupCamera();
        SetupLight();

        Selection.activeGameObject = root;
        Debug.Log("Escena Rube Goldberg construida. Los marcadores de etapa se añaden al pulsar Play. Pulsa Espacio para soltar la bola roja.");
    }

    [MenuItem("Tools/Rube Goldberg/Borrar escena")]
    public static void Clear()
    {
        var old = GameObject.Find(Root);
        if (old != null) Undo.DestroyObjectImmediate(old);
    }

    // ---------- helpers ----------
    static GameObject Create(Def d, Dictionary<string, Material> colors)
    {
        GameObject go;
        switch (d.shape)
        {
            case "sphere": go = GameObject.CreatePrimitive(PrimitiveType.Sphere); break;
            case "cyl": go = GameObject.CreatePrimitive(PrimitiveType.Cylinder); break;
            case "group": return new GameObject(d.name);
            default: go = GameObject.CreatePrimitive(PrimitiveType.Cube); break;
        }
        go.GetComponent<Renderer>().sharedMaterial = ColorMaterial(d.color, colors);
        return go;
    }

    static Material ColorMaterial(string hex, Dictionary<string, Material> cache)
    {
        if (cache.TryGetValue(hex, out var m)) return m;
        bool urp = GraphicsSettings.currentRenderPipeline != null;
        var shader = Shader.Find(urp ? "Universal Render Pipeline/Lit" : "Standard");
        m = new Material(shader);
        ColorUtility.TryParseHtmlString(hex, out var c);
        m.color = c;
        AssetDatabase.CreateAsset(m, $"{MatFolder}/RG_{hex.TrimStart('#')}.mat");
        cache[hex] = m;
        return m;
    }

#if UNITY_6000_0_OR_NEWER
    static Dictionary<string, PhysicsMaterial> MakePhysicMaterials()
    {
        var map = new Dictionary<string, PhysicsMaterial>();
        foreach (var kv in Friction)
        {
            var pm = new PhysicsMaterial("PM_" + kv.Key)
            {
                dynamicFriction = kv.Value, staticFriction = kv.Value, bounciness = 0f,
                frictionCombine = kv.Key == "Pata" ? PhysicsMaterialCombine.Minimum : PhysicsMaterialCombine.Average, bounceCombine = PhysicsMaterialCombine.Average,
            };
            AssetDatabase.CreateAsset(pm, $"{MatFolder}/PM_{kv.Key}.physicMaterial");
            map[kv.Key] = pm;
        }
        return map;
    }
#else
    static Dictionary<string, PhysicMaterial> MakePhysicMaterials()
    {
        var map = new Dictionary<string, PhysicMaterial>();
        foreach (var kv in Friction)
        {
            var pm = new PhysicMaterial("PM_" + kv.Key)
            {
                dynamicFriction = kv.Value, staticFriction = kv.Value, bounciness = 0f,
                frictionCombine = kv.Key == "Pata" ? PhysicMaterialCombine.Minimum : PhysicMaterialCombine.Average, bounceCombine = PhysicMaterialCombine.Average,
            };
            AssetDatabase.CreateAsset(pm, $"{MatFolder}/PM_{kv.Key}.physicMaterial");
            map[kv.Key] = pm;
        }
        return map;
    }
#endif

    static void SetupCamera()
    {
        var cam = Camera.main;
        if (cam == null)
        {
            var go = new GameObject("Main Camera") { tag = "MainCamera" };
            cam = go.AddComponent<Camera>();
        }
        cam.orthographic = true;
        cam.orthographicSize = 1.7f;
        cam.transform.position = new Vector3(2.75f, 0.80f, -8f);
        cam.transform.rotation = Quaternion.identity;
        cam.clearFlags = CameraClearFlags.SolidColor;
        cam.backgroundColor = new Color(0.79f, 0.83f, 0.87f);
    }

    static void SetupLight()
    {
#if UNITY_2023_1_OR_NEWER
        var light = Object.FindFirstObjectByType<Light>();
#else
        var light = Object.FindObjectOfType<Light>();
#endif
        if (light == null)
        {
            var go = new GameObject("Directional Light");
            light = go.AddComponent<Light>();
            light.type = LightType.Directional;
        }
        light.transform.rotation = Quaternion.Euler(50f, -30f, 0f);
    }

    static void SetFixedTimestep(float v)
    {
        // Unity 6 stores the timestep as a rational value, so SerializedProperty.floatValue fails.
        Time.fixedDeltaTime = v;
        AssetDatabase.SaveAssets();
    }
}
