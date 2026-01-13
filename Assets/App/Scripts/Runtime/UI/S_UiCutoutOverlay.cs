using UnityEngine;

public class S_UiCutoutOverlay : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private RectTransform _overlayRoot;
    [SerializeField] private RectTransform _highlight;
    [SerializeField] private RectTransform _top;
    [SerializeField] private RectTransform _bottom;
    [SerializeField] private RectTransform _left;
    [SerializeField] private RectTransform _right;

    [Header("Optional padding (in pixels UI)")]
    [SerializeField] private Vector2 padding = Vector2.zero;

    [Header("Input")]
    [SerializeField] RSE_OnChangeHighlightTarget _onChangeHighlightTarget;
    [SerializeField] RSE_OnChangeActiveStatePanelsFilters _onChangeActiveStatePanelsFilters;


    private void LateUpdate()
    {
        //if (_overlayRoot == null || _highlight == null) return; // maybe if we want movement highlight
        //UpdatePanels();
    }

    private void Start()
    {
        if (_overlayRoot == null || _highlight == null) return;
        UpdatePanels();
    }

    private void OnEnable()
    {
        _onChangeHighlightTarget.action += ChangeHighlightTarget;
        _onChangeActiveStatePanelsFilters.action += ChangeStatePanelCutout;
    }

    private void OnDisable()
    {
        _onChangeHighlightTarget.action -= ChangeHighlightTarget;
        _onChangeActiveStatePanelsFilters.action -= ChangeStatePanelCutout;
    }

    private void UpdatePanels()
    {
        // get corners of highlight in world space
        Vector3[] worldCorners = new Vector3[4];
        _highlight.GetWorldCorners(worldCorners);
        // Order: 0 = bottom-left, 1 = top-left, 2 = top-right, 3 = bottom-right

        Vector3 bl = _overlayRoot.InverseTransformPoint(worldCorners[0]);
        Vector3 tl = _overlayRoot.InverseTransformPoint(worldCorners[1]);
        Vector3 tr = _overlayRoot.InverseTransformPoint(worldCorners[2]);
        Vector3 br = _overlayRoot.InverseTransformPoint(worldCorners[3]);

        float holeMinX = bl.x;
        float holeMaxX = tr.x;
        float holeMinY = bl.y;
        float holeMaxY = tr.y;

        holeMinX -= padding.x;
        holeMaxX += padding.x;
        holeMinY -= padding.y;
        holeMaxY += padding.y;

        Rect r = _overlayRoot.rect;
        float rootMinX = r.xMin;
        float rootMaxX = r.xMax;
        float rootMinY = r.yMin;
        float rootMaxY = r.yMax;

        holeMinX = Mathf.Clamp(holeMinX, rootMinX, rootMaxX);
        holeMaxX = Mathf.Clamp(holeMaxX, rootMinX, rootMaxX);
        holeMinY = Mathf.Clamp(holeMinY, rootMinY, rootMaxY);
        holeMaxY = Mathf.Clamp(holeMaxY, rootMinY, rootMaxY);

        SetPanelRect(_top, rootMinX, holeMaxY, rootMaxX, rootMaxY);
        SetPanelRect(_bottom, rootMinX, rootMinY, rootMaxX, holeMinY);
        SetPanelRect(_left, rootMinX, holeMinY, holeMinX, holeMaxY);
        SetPanelRect(_right, holeMaxX, holeMinY, rootMaxX, holeMaxY);
    }

    private static void SetPanelRect(RectTransform panel, float minX, float minY, float maxX, float maxY)
    {
        if (panel == null) return;

        panel.anchorMin = panel.anchorMax = new Vector2(0.5f, 0.5f);
        panel.pivot = new Vector2(0.5f, 0.5f);

        float width = Mathf.Max(0, maxX - minX);
        float height = Mathf.Max(0, maxY - minY);

        float cx = (minX + maxX) * 0.5f;
        float cy = (minY + maxY) * 0.5f;

        panel.anchoredPosition = new Vector2(cx, cy);
        panel.SetSizeWithCurrentAnchors(RectTransform.Axis.Horizontal, width);
        panel.SetSizeWithCurrentAnchors(RectTransform.Axis.Vertical, height);
    }

    private void ChangeStatePanelCutout()
    {
        _highlight.gameObject.SetActive(!_highlight.gameObject.activeSelf);
        _top.gameObject.SetActive(!_top.gameObject.activeSelf);
        _bottom.gameObject.SetActive(!_bottom.gameObject.activeSelf);
        _left.gameObject.SetActive(!_left.gameObject.activeSelf);
        _right.gameObject.SetActive(!_right.gameObject.activeSelf);
    }

    private void ChangeHighlightTarget(RectTransform newTarget)
    {
        _highlight = newTarget;
        if (_overlayRoot == null || _highlight == null) return;
        UpdatePanels();
    }
}