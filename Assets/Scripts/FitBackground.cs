using UnityEngine;
using UnityEngine.UI;

public class FitBackground : MonoBehaviour
{
    [SerializeField] private Camera targetCamera;
    [SerializeField] private bool preserveAspectRatio;
    [SerializeField] private bool moveUiImageToCanvasRoot = true;
    [SerializeField] private bool sendUiImageBehindOtherUi = true;

    private int lastScreenWidth;
    private int lastScreenHeight;

    private void Awake()
    {
        Fit();
    }

    private void OnEnable()
    {
        Fit();
    }

    private void Start()
    {
        Fit();
    }

    private void LateUpdate()
    {
        if (lastScreenWidth == Screen.width && lastScreenHeight == Screen.height)
        {
            return;
        }

        Fit();
    }

    private void OnRectTransformDimensionsChange()
    {
        Fit();
    }

    public void Fit()
    {
        if (TryFitUiImage())
        {
            return;
        }

        FitSpriteRenderer();
    }

    private bool TryFitUiImage()
    {
        RectTransform rectTransform = GetComponent<RectTransform>();
        Image image = GetComponent<Image>();

        if (rectTransform == null || image == null)
        {
            return false;
        }

        Canvas parentCanvas = GetComponentInParent<Canvas>();

        if (Application.isPlaying &&
            moveUiImageToCanvasRoot &&
            parentCanvas != null &&
            rectTransform.parent != parentCanvas.transform)
        {
            rectTransform.SetParent(parentCanvas.transform, false);
        }

        rectTransform.anchorMin = Vector2.zero;
        rectTransform.anchorMax = Vector2.one;
        rectTransform.pivot = new Vector2(0.5f, 0.5f);
        rectTransform.offsetMin = Vector2.zero;
        rectTransform.offsetMax = Vector2.zero;
        rectTransform.sizeDelta = Vector2.zero;
        rectTransform.anchoredPosition = Vector2.zero;
        rectTransform.localScale = Vector3.one;

        image.preserveAspect = preserveAspectRatio;

        if (sendUiImageBehindOtherUi)
        {
            rectTransform.SetAsFirstSibling();
        }

        CacheScreenSize();
        return true;
    }

    private void FitSpriteRenderer()
    {
        SpriteRenderer spriteRenderer = GetComponent<SpriteRenderer>();

        if (spriteRenderer == null || spriteRenderer.sprite == null)
        {
            Debug.LogWarning("FitBackground needs a SpriteRenderer or UI Image on " + name + ".");
            return;
        }

        Camera cameraToUse = targetCamera != null ? targetCamera : Camera.main;

        if (cameraToUse == null || !cameraToUse.orthographic)
        {
            Debug.LogWarning("FitBackground needs an orthographic camera for SpriteRenderer backgrounds.");
            return;
        }

        float worldHeight = cameraToUse.orthographicSize * 2f;
        float worldWidth = worldHeight * cameraToUse.aspect;
        float zScale = Mathf.Approximately(transform.localScale.z, 0f) ? 1f : transform.localScale.z;

        // Reset first and measure the real rendered world size. This is more reliable
        // than sprite.bounds for imported photos with unusual pixels-per-unit values.
        transform.localScale = new Vector3(1f, 1f, zScale);
        Vector3 spriteSize = spriteRenderer.bounds.size;

        if (spriteSize.x <= 0f || spriteSize.y <= 0f)
        {
            return;
        }

        if (preserveAspectRatio)
        {
            float uniformScale = Mathf.Max(worldWidth / spriteSize.x, worldHeight / spriteSize.y);
            transform.localScale = new Vector3(uniformScale, uniformScale, 1f);
        }
        else
        {
            transform.localScale = new Vector3(
                worldWidth / spriteSize.x,
                worldHeight / spriteSize.y,
                1f);
        }

        transform.position = new Vector3(
            cameraToUse.transform.position.x,
            cameraToUse.transform.position.y,
            transform.position.z);

        CacheScreenSize();
    }

    private void CacheScreenSize()
    {
        lastScreenWidth = Screen.width;
        lastScreenHeight = Screen.height;
    }
}
