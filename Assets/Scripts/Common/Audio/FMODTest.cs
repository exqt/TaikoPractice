using FMOD;
using FMOD.Studio;
using FMODUnity;
using UnityEngine;
using Debug = UnityEngine.Debug;

public class FMODTest : MonoBehaviour
{
    // This class is a placeholder for testing FMOD functionality.
    // You can add your test code here to verify FMOD integration.

    // Example method to demonstrate FMOD functionality.

    FMOD.System coreSystem;
    FMOD.Studio.System studioSystem;

    public EventReference donRef;
    public FMOD.Studio.EventInstance donInstance;

    InputThread inputThread;

    public void Start()
    {
        Debug.Log("FMODTest started.");
        Test();
        inputThread = new InputThread();
        inputThread.OnKeyPressed += (key) =>
        {
            if (key == TaikoKeyType.LEFT_KA)
            {
                donInstance.start();
            }
        };
    }

    void OnDestroy()
    {
        donInstance.release();

        {
            var res = studioSystem.release();
            if (res != FMOD.RESULT.OK)
            {
                Debug.LogError("Failed to release FMOD Studio System: " + res);
            }
        }
    }

    public void Test()
    {
        // CoreSystem은 StudioSystem과 같이 release되는듯?
        {
            var res = RuntimeManager.StudioSystem.release();
            if (res != FMOD.RESULT.OK)
            {
                Debug.LogError("Failed to release FMOD Studio System: " + res);
                return;
            }
        }

        {
            var res = FMOD.Studio.System.create(out studioSystem);
            if (res != FMOD.RESULT.OK)
            {
                Debug.LogError("Failed to create FMOD Studio System: " + res);
                return;
            }
        }

        {
            var res = studioSystem.getCoreSystem(out coreSystem);
            if (res != FMOD.RESULT.OK)
            {
                Debug.LogError("Failed to get FMOD Core System: " + res);
                return;
            }
        }


        {
            var res = coreSystem.setDSPBufferSize(64, 2);
            if (res != FMOD.RESULT.OK)
            {
                Debug.LogError("Failed to set DSP buffer size: " + res);
                return;
            }
        }

        {
            var res = studioSystem.initialize(32, FMOD.Studio.INITFLAGS.NORMAL, FMOD.INITFLAGS.NORMAL, System.IntPtr.Zero);
            if (res != FMOD.RESULT.OK)
            {
                Debug.LogError("Failed to initialize FMOD Studio System: " + res);
                return;
            }
        }

        {
            var res = coreSystem.setOutput(FMOD.OUTPUTTYPE.ASIO);
            if (res != FMOD.RESULT.OK)
            {
                Debug.LogError("Failed to set output type: " + res);
                return;
            }
        }

        // Load Bankfile
        Bank bank;
        {
            var res = studioSystem.loadBankFile("Assets/StreamingAssets/Master.bank", FMOD.Studio.LOAD_BANK_FLAGS.NORMAL, out bank);
            if (res != FMOD.RESULT.OK)
            {
                Debug.LogError("Failed to load bank file: " + res);
                return;
            }
        }

        {
            bank.getEventList(out var events);
            foreach (var ev in events)
            {
                ev.getID(out var id);
                if (id == donRef.Guid)
                {
                    var dondesc = ev;
                    var res = dondesc.createInstance(out donInstance);
                    if (res != FMOD.RESULT.OK)
                    {
                        Debug.LogError("Failed to create instance of event: " + res);
                        return;
                    }
                    break;
                }
            }
        }
    }

    public void Update()
    {
        coreSystem.update();
        studioSystem.update();

        if (Input.GetKeyDown(KeyCode.Space))
        {
            coreSystem.getDSPBufferSize(out uint bufferLength, out int numBuffers);
            Debug.Log($"FMOD DSP Buffer Size: {bufferLength}, Num Buffers: {numBuffers}");

            coreSystem.getOutput(out var outputType);
            Debug.Log($"FMOD Output Type: {outputType}");
        }

        if (Input.GetKeyDown(KeyCode.P))
        {
            if (donInstance.isValid())
            {
                donInstance.start();
            }
        }
    }
}
