using UnityEngine;
#if ENABLE_INPUT_SYSTEM
using UnityEngine.InputSystem;
#endif

// Unity needs one MonoBehaviour per file, named like the class.
/// <summary>On BolaRoja: pressing Space releases it and starts the stage clock.</summary>
public class RGTrigger : MonoBehaviour
{
    public static float T0 = -1f;   // time of the release; -1 until then

    Rigidbody rb;

    void Awake()
    {
        T0 = -1f;
        rb = GetComponent<Rigidbody>();
    }

    void Update()
    {
        if (T0 >= 0f) return;
        bool pressed = false;
#if ENABLE_INPUT_SYSTEM
        pressed = Keyboard.current != null && Keyboard.current.spaceKey.wasPressedThisFrame;
#elif ENABLE_LEGACY_INPUT_MANAGER
        pressed = Input.GetKeyDown(KeyCode.Space);
#endif
        if (!pressed) return;
        T0 = Time.time;
        rb.isKinematic = false;
        Debug.Log("Etapa 1 activada (t=0.00s)");
    }
}
