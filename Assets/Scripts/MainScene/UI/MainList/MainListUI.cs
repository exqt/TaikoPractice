using System.Collections;
using System.Collections.Generic;
using System.Linq;
using DG.Tweening;
using UnityEngine;


public class UIMainList : MonoBehaviour
{
    public class State
    {
        public Dictionary<string, bool> isDirectoryOpen = new();
        public string recentItemName = "";
    }

    public GameObject mainListDirectoryPrefab;

    public static readonly float ySpcacing = 10f;
    public static readonly float tweenTime = 0.4f;

    UIMainListItem[] items;

    float scrollY = 0f;

    public int SelectedIndex { get; private set; } = 0;

    public void UpdateItemList()
    {
        items = GetComponentsInChildren<UIMainListItem>();
    }

    void Awake()
    {
        CreatePatternMapItem();
        UpdateItemList();
        UpdateItemPositions();
    }

    void CreatePatternMapItem()
    {
        var directories = FileManager.Instance.GetDirectories(Paths.patternMapsDirectory);

        foreach (var dir in directories)
        {
            var item = Instantiate(mainListDirectoryPrefab, transform).GetComponent<UIMainListItem>();
            item.title.text = dir;
            item.gameObject.name = dir;
        }
    }

    void Start()
    {
        SelectItem(items[0]);
    }

    void Update()
    {
        UpdateItemPositions();
    }

    float _moveAnimX = 0f;
    Tween _moveAnimTween = null;
    public void MoveItemCursor(int index, bool immediate = false)
    {
        if (items == null || items.Length == 0) return;

        _moveAnimTween?.Kill();

        _moveAnimX = SelectedIndex;

        SelectedIndex = Mathf.Clamp(index, 0, items.Length - 1);
        SelectItem(items[SelectedIndex], immediate);

        if (immediate)
        {
            _moveAnimX = SelectedIndex;
            return;
        }

        _moveAnimTween = DOTween.To(() => _moveAnimX, x => _moveAnimX = x, SelectedIndex, tweenTime).SetEase(Ease.OutCubic);
    }

    public void MoveItemCursor(UIMainListItem targetItem, bool immediate = false)
    {
        if (items == null || items.Length == 0) return;

        int index = items.ToList().IndexOf(targetItem);
        if (index < 0) return; // item not found

        MoveItemCursor(index, immediate);
    }

    public State GetState()
    {
        var state = new State();
        foreach (var item in items)
        {
            if (item is UIMainListDirectory directoryItem)
            {
                state.isDirectoryOpen[item.gameObject.name] = directoryItem.isExpanded;
            }
        }

        state.recentItemName = items[SelectedIndex].gameObject.name;
        return state;
    }

    public void RecoverState(State state)
    {
        IEnumerator RecoverStateInternal()
        {
            while (items == null || items.Length == 0)
            {
                yield return null; // wait until items are initialized
            }

            for (int i = 0; i < items.Length; i++)
            {
                var item = items[i];
                if (item is UIMainListDirectory dirItem)
                {
                    if (state.isDirectoryOpen.TryGetValue(item.gameObject.name, out var expanded))
                    {
                        if (expanded) dirItem.Expand();
                    }
                }
            }

            yield return new WaitForEndOfFrame(); // wait for the fold/unfold to complete

            UpdateItemPositions();

            var index = 0;
            for (int i = 0; i < items.Length; i++)
            {
                if (items[i].gameObject.name == state.recentItemName)
                {
                    index = i;
                    break;
                }
            }

            MoveItemCursor(index, immediate: true);
            UpdateItemPositions();
        }

        StartCoroutine(RecoverStateInternal());
    }

    public void UpdateItemPositions()
    {
        var itemCenterYs = CaluclateItemsCenterY(true);

        for (int i = 0; i < items.Length; i++)
        {
            var item = items[i];
            var itemCenterY = itemCenterYs[i];
            var itemY = itemCenterY + scrollY;
            var itemX = (Mathf.Cos((i - _moveAnimX) / 3f) - 1f) * 250f;

            item.transform.localPosition = new Vector3(itemX, itemY, 0);
        }
    }

    Tween _scrollTween = null;
    void ScrollTo(float targetY, bool immediate = false)
    {
        _scrollTween?.Kill();

        if (immediate)
        {
            scrollY = targetY;
            UpdateItemPositions();
            return;
        }

        _scrollTween = DOTween.To(() => scrollY, y => scrollY = y, targetY, tweenTime).SetEase(Ease.OutCubic);
    }

    List<float> CaluclateItemsCenterY(bool isRealTime = false)
    {
        var itemCenterYList = new List<float>();
        var itemCenterY = 0f;
        for (int i = 0; i < items.Length; i++)
        {
            var item = items[i];
            var sizeY = isRealTime ? item.GetComponent<RectTransform>().sizeDelta.y : item.GetHeight();

            if (i > 0) itemCenterY -= sizeY / 2;
            itemCenterYList.Add(itemCenterY);
            itemCenterY -= sizeY / 2 + ySpcacing;
        }

        return itemCenterYList;
    }

    void SelectItem(UIMainListItem targetItem, bool immediate = false)
    {
        for (int i = 0; i < items.Length; i++)
        {
            var item = items[i];
            if (item == targetItem)
            {
                var ys = CaluclateItemsCenterY();
                ScrollTo(-ys[i], immediate);
            }
            else
            {
                // if (!item.IsFolded) item.Fold();
            }
        }

        TaikoUIMoveManager.Instance.SetItem(targetItem.GetComponent<ITaikoUIMove>());
    }

    public void OpenItem(UIMainListItem item)
    {
        int index = items.ToList().IndexOf(item);
        ScrollTo(-CaluclateItemsCenterY()[index]);
    }

    [ContextMenu("AlignOnEditor")]
    public void AlignOnEditor()
    {
        var items = GetComponentsInChildren<UIMainListItem>(true);
        var y = 0f;
        var spcacing = 10f;
        foreach (var item in items)
        {
            item.transform.localPosition = new Vector3(0, y, 0);
            var rectTransform = item.GetComponent<RectTransform>();
            y -= rectTransform.sizeDelta.y + spcacing;
        }
    }

    public void OnTaikoUIMoveLeft()
    {
        MoveItemCursor(SelectedIndex - 1);
    }

    public void OnTaikoUIMoveRight()
    {
        MoveItemCursor(SelectedIndex + 1);
    }

    public void OnTaikoUISelect()
    {
        OpenItem(items[SelectedIndex]);
    }
}
