// Edited ThalmicHub script simpler.
// by Hyung-il Kim

using UnityEngine;

#if UNITY_EDITOR
using UnityEditor;
#endif

using System.Collections.Generic;

using LockingPolicy = Thalmic.Myo.LockingPolicy;

public class SingleMyoHub : MonoBehaviour
{
    // The single instance of SingleMyoHub. Set during Awake.
    public static SingleMyoHub instance
    {
        get { return _instance; }
    }

    public string applicationIdentifier = "com.example.myo-unity";

    // If set to None, pose events are always sent. If set to Standard, pose events are not sent while a Myo is locked.
    public LockingPolicy lockingPolicy;

    // True if and only if the hub initialized successfully; typically this is set during Awake, but it can also be
    // set by calling ResetHub() explicitly. The typical reason for initialization to fail is that Myo Connect is not
    // running.
    public bool hubInitialized
    {
        get { return _hub != null; }
    }

    // Reset the hub. This function is typically used if initialization failed to attempt to initialize again (e.g.
    // after asking the user to ensure that Myo Connect is running).
    public bool ResetHub()
    {
        if (_hub != null)
        {
            _hub.Dispose();
            _hub = null;

            _myo.internalMyo = null;
        }
        return createHub();
    }

    void Awake()
    {
        // Ensure that there is only one Hub.
        if (_instance != null)
        {
#if UNITY_EDITOR
            EditorUtility.DisplayDialog("Can only have one SingleMyoHub",
                                        "Your scene contains more than one SingleMyoHub. Remove all but one SingleMyoHub.",
                                        "OK");
#endif
            Destroy(this.gameObject);
            return;
        }
        else
        {
            _instance = this;
        }

        // Do not destroy this game object. This will ensure that it remains active even when
        // switching scenes.
        DontDestroyOnLoad(this);


        _myo = GetComponent<ThalmicMyo>();

        if (_myo == null)
        {
            string errorMessage = "The SingleMyoHub's GameObject must have ThalmicMyo component.";
#if UNITY_EDITOR
            EditorUtility.DisplayDialog("SingleMyoHub has no ThalmicMyo component", errorMessage, "OK");
#else
            throw new UnityException (errorMessage);
#endif
        }

        createHub();
    }

    private bool createHub()
    {
        try
        {
            _hub = new Thalmic.Myo.Hub(applicationIdentifier, hub_MyoPaired);
            _hub.SetLockingPolicy(lockingPolicy);
        }
        catch (System.Exception)
        {
            Debug.Log("SingleMyoHub failed to initialize.");
            return false;
        }
        return true;
    }

    void OnApplicationQuit()
    {
        if (_hub != null)
        {
            _hub.Dispose();
            _hub = null;
        }
    }

    void Update()
    {
    }

    void hub_MyoPaired(object sender, Thalmic.Myo.MyoEventArgs e)
    {
        if (_myo.internalMyo == null)
        {
            _myo.internalMyo = e.Myo;
        }
    }

    private static SingleMyoHub _instance = null;

    private Thalmic.Myo.Hub _hub = null;

    private ThalmicMyo _myo = null;
}
