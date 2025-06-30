using DG.Tweening;
using UnityEngine;
using UnityEngine.InputSystem;
using FMODUnity;
using FMOD.Studio;
using System.Collections;
using UnityEngine.UI;

[DefaultExecutionOrder(-5000)]
public class TaikoUIMoveManager : MonoBehaviour
{
    public static TaikoUIMoveManager Instance { get; private set; }
    public ITaikoUIMove current;
    public RectTransform highlight;

    // Reference your InputActionAsset in the inspector
    public InputActionAsset inputActions;

    private InputAction leftKaAction, leftDonAction, rightKaAction, rightDonAction;

    public EventReference skipHitSound;
    EventInstance skipHitSoundInstance;

    TaikoUIMoveSkipHitWatcher skipHitWatcher;

    public Image cursorImage;

    #region Input Block
    bool inputBlocked;
    Coroutine inputBlockCoroutine;
    public void BlockInput(float seconds)
    {
        if (inputBlockCoroutine != null) StopCoroutine(inputBlockCoroutine);
        inputBlockCoroutine = StartCoroutine(BlockInputCoroutine(seconds));
    }
    IEnumerator BlockInputCoroutine(float seconds)
    {
        inputBlocked = true;
        cursorImage.color = new Color(cursorImage.color.r, cursorImage.color.g, cursorImage.color.b, 0.2f); // Dim the cursor
        yield return new WaitForSeconds(seconds);
        inputBlocked = false;
        cursorImage.color = new Color(cursorImage.color.r, cursorImage.color.g, cursorImage.color.b, 1f); // Restore the cursor
        inputBlockCoroutine = null;
    }
    #endregion

    void Awake()
    {
        if (Instance == null) Instance = this;
        else
        {
            Destroy(gameObject);
            return;
        }

        cursorImage = GetComponent<Image>();

        // Get actions from your input map
        var globalMap = inputActions.FindActionMap("Drum");
        leftKaAction = globalMap.FindAction("LeftKa");
        rightKaAction = globalMap.FindAction("RightKa");
        leftDonAction = globalMap.FindAction("LeftDon");
        rightDonAction = globalMap.FindAction("RightDon");

        skipHitWatcher = new TaikoUIMoveSkipHitWatcher(OnSkipHit);
        skipHitSoundInstance = RuntimeManager.CreateInstance(skipHitSound);
    }

    void OnEnable()
    {
        leftKaAction.performed += OnLeft;
        rightKaAction.performed += OnRight;
        leftDonAction.performed += OnSelect;
        rightDonAction.performed += OnSelect;

        leftKaAction.Enable();
        rightKaAction.Enable();
        leftDonAction.Enable();
        rightDonAction.Enable();
    }

    void OnDisable()
    {
        leftKaAction.performed -= OnLeft;
        rightKaAction.performed -= OnRight;
        leftDonAction.performed -= OnSelect;
        rightDonAction.performed -= OnSelect;

        leftKaAction.Disable();
        rightKaAction.Disable();
        leftDonAction.Disable();
        rightDonAction.Disable();
    }

    void OnLeft(InputAction.CallbackContext ctx)
    {
        if (inputBlocked) return;
        if (current != null)
        {
            current.OnTaikoUIMoveLeft();
            skipHitWatcher.HandleHit(-1);
        }
    }

    void OnRight(InputAction.CallbackContext ctx)
    {
        if (inputBlocked) return;
        if (current != null)
        {
            current.OnTaikoUIMoveRight();
            skipHitWatcher.HandleHit(+1);
        }
    }

    void OnSelect(InputAction.CallbackContext ctx)
    {
        if (inputBlocked) return;
        if (current != null) current.OnTaikoUISelect();
    }

    void OnSkipHit(int delta)
    {
        if (inputBlocked) return;
        skipHitSoundInstance.start();
        while (delta != 0)
        {
            if (delta > 0)
            {
                if (current != null) current.OnTaikoUIMoveRight();
                delta--;
            }
            else
            {
                if (current != null) current.OnTaikoUIMoveLeft();
                delta++;
            }
        }
    }

    public void Clear()
    {
        current?.OnTaikoUIMoveLeave();
        current = null;
        transform.localPosition = new Vector3(0, 3000, 0);
    }

    public void SetItem(GameObject item)
    {
        if (item == null)
        {
            Clear();
            return;
        }

        var taikoUIMove = item.GetComponent<ITaikoUIMove>();
        if (taikoUIMove == null)
        {
            Debug.LogWarning("SetItem: item does not implement ITaikoUIMove");
            return;
        }

        SetItem(taikoUIMove);
    }

    public void SetItem(ITaikoUIMove item)
    {
        if (item == null)
        {
            Clear();
            return;
        }

        if (current != null) current.OnTaikoUIMoveLeave();

        current = item;
        current.OnTaikoUIMoveEnter();
        gameObject.SetActive(true);
    }

    void SetHighlight(GameObject obj)
    {
        var itemRect = obj.GetComponent<RectTransform>();
        var itemLeftTop = itemRect.TransformPoint(new Vector3(-itemRect.rect.width / 2, itemRect.rect.height / 2, 0));
        var itemRightBottom = itemRect.TransformPoint(new Vector3(itemRect.rect.width / 2, -itemRect.rect.height / 2, 0));

        highlight.position = (itemLeftTop + itemRightBottom) / 2;
        highlight.sizeDelta = new Vector2(itemRect.rect.width, itemRect.rect.height);
    }

    void LateUpdate()
    {
        if (current == null) return;

        highlight.gameObject.SetActive(true);
        var currentObj = current as MonoBehaviour;
        if (currentObj == null) return;

        if (!currentObj.gameObject.activeInHierarchy)
        {
            highlight.gameObject.SetActive(false);
            current = null;
            return;
        }

        SetHighlight((current as MonoBehaviour).gameObject);
    }
}
