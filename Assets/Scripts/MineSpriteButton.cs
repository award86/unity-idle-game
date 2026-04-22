using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;

public class MineSpriteButton : MonoBehaviour
{
    [SerializeField] private GameManager gameManager;
    [SerializeField] private bool addColliderIfMissing = true;
    [SerializeField] private bool useScenePosition = true;
    [SerializeField] private bool autoConvertCanvasSpriteToWorld = true;
    [SerializeField] private Vector2 viewportPosition = new Vector2(0.5f, 0.33f);
    [SerializeField] private float spriteWorldHeight = 1.4f;
    [SerializeField] private bool resizeSpriteOnStart;
    [SerializeField] private int sortingOrder = 20;
    [SerializeField] private bool animateFramesOnClick = true;
    [SerializeField] private Sprite[] animationFrames;
    [SerializeField] private int generatedFrameColumns = 5;
    [SerializeField] private int generatedFrameRows = 2;
    [SerializeField] private int generatedFrameCount = 10;
    [SerializeField] private float animationFramesPerSecond = 12f;
    [SerializeField] private bool restoreFirstFrameAfterAnimation = true;
    [SerializeField] private Vector3 pressedOffset = new Vector3(0f, -0.15f, 0f);
    [SerializeField] private float pressedMoveDuration = 0.07f;
    [SerializeField] private bool showFloatingOreText = true;
    [SerializeField] private Vector3 floatingOreTextOffset = new Vector3(0f, 0.8f, 0f);
    [SerializeField] private Vector2 floatingOreTextRandomOffsetPixels = new Vector2(18f, 10f);
    [SerializeField] private float floatingOreTextRiseDistance = 0.35f;
    [SerializeField] private float floatingOreTextLifetime = 0.8f;
    [SerializeField] private int floatingOreTextFontSize = 48;
    [SerializeField] private float floatingOreTextCharacterSize = 0.04f;
    [SerializeField] private Color floatingOreTextColor = new Color(1f, 0.88f, 0.25f, 1f);
    [SerializeField] private Font floatingOreTextFont;

    private SpriteRenderer spriteRenderer;
    private Sprite idleSprite;
    private Sprite[] generatedFrames;
    private Vector3 startLocalPosition;
    private Coroutine pressAnimation;
    private Coroutine frameAnimation;

    private void Awake()
    {
        spriteRenderer = GetComponent<SpriteRenderer>();
        idleSprite = spriteRenderer != null ? spriteRenderer.sprite : null;
        startLocalPosition = transform.localPosition;
        EnsureCollider();
    }

    private void Start()
    {
        if (gameManager == null)
        {
            gameManager = FindAnyObjectByType<GameManager>();
        }

        PrepareSpriteRenderer();
        startLocalPosition = transform.localPosition;

        if (gameManager != null)
        {
            gameManager.RegisterMineSpriteButton(this);
        }
    }

    private void OnMouseDown()
    {
        if (IsPointerOverUi())
        {
            return;
        }

        if (gameManager == null)
        {
            gameManager = FindAnyObjectByType<GameManager>();
        }

        if (gameManager == null)
        {
            return;
        }

        int minedOre = gameManager.MineOreFromSpriteClick();

        if (minedOre <= 0)
        {
            return;
        }

        ShowFloatingOreText(minedOre);
        PlayPressAnimation();
        PlayFrameAnimation();
    }

    private void OnDisable()
    {
        if (pressAnimation != null)
        {
            StopCoroutine(pressAnimation);
            pressAnimation = null;
        }

        if (frameAnimation != null)
        {
            StopCoroutine(frameAnimation);
            frameAnimation = null;
        }

        if (spriteRenderer != null && idleSprite != null)
        {
            spriteRenderer.sprite = idleSprite;
        }

        transform.localPosition = startLocalPosition;
    }

    private void EnsureCollider()
    {
        if (!addColliderIfMissing ||
            GetComponent<Collider2D>() != null ||
            GetComponent<Collider>() != null)
        {
            return;
        }

        BoxCollider2D clickCollider = gameObject.AddComponent<BoxCollider2D>();
        if (spriteRenderer != null && spriteRenderer.sprite != null)
        {
            clickCollider.offset = spriteRenderer.sprite.bounds.center;
            clickCollider.size = spriteRenderer.sprite.bounds.size;
        }
    }

    private void PrepareSpriteRenderer()
    {
        if (spriteRenderer == null)
        {
            spriteRenderer = GetComponent<SpriteRenderer>();
        }

        if (spriteRenderer == null || spriteRenderer.sprite == null)
        {
            return;
        }

        if (idleSprite == null)
        {
            idleSprite = spriteRenderer.sprite;
        }

        spriteRenderer.sortingOrder = sortingOrder;

        bool isInsideCanvasTransform = transform.parent is RectTransform || GetComponentInParent<Canvas>() != null;

        if (useScenePosition)
        {
            if (isInsideCanvasTransform && autoConvertCanvasSpriteToWorld)
            {
                MoveCanvasSpriteIntoCameraView(spriteRenderer);
                return;
            }

            if (resizeSpriteOnStart)
            {
                ResizeSprite(spriteRenderer);
            }

            return;
        }

        Camera mainCamera = Camera.main;

        if (mainCamera == null || !mainCamera.orthographic)
        {
            return;
        }

        if (isInsideCanvasTransform)
        {
            transform.SetParent(null, true);
        }

        float zPosition = 0f;
        float cameraDistance = Mathf.Abs(zPosition - mainCamera.transform.position.z);
        Vector3 worldPosition = mainCamera.ViewportToWorldPoint(new Vector3(
            Mathf.Clamp01(viewportPosition.x),
            Mathf.Clamp01(viewportPosition.y),
            cameraDistance));
        transform.position = new Vector3(worldPosition.x, worldPosition.y, zPosition);

        ResizeSprite(spriteRenderer);
    }

    private void MoveCanvasSpriteIntoCameraView(SpriteRenderer targetSpriteRenderer)
    {
        Camera mainCamera = Camera.main;

        if (mainCamera == null || !mainCamera.orthographic)
        {
            return;
        }

        Vector3 targetWorldPosition;

        if (!TryConvertCurrentCanvasPositionToWorld(mainCamera, out targetWorldPosition))
        {
            float cameraDistance = Mathf.Abs(mainCamera.transform.position.z);
            targetWorldPosition = mainCamera.ViewportToWorldPoint(new Vector3(
                Mathf.Clamp01(viewportPosition.x),
                Mathf.Clamp01(viewportPosition.y),
                cameraDistance));
        }

        transform.SetParent(null, true);
        transform.position = new Vector3(targetWorldPosition.x, targetWorldPosition.y, 0f);
        ResizeSprite(targetSpriteRenderer);
    }

    private bool TryConvertCurrentCanvasPositionToWorld(Camera mainCamera, out Vector3 worldPosition)
    {
        worldPosition = Vector3.zero;

        Vector2 screenPosition = RectTransformUtility.WorldToScreenPoint(null, transform.position);

        bool isOnScreen =
            screenPosition.x >= 0f &&
            screenPosition.x <= Screen.width &&
            screenPosition.y >= 0f &&
            screenPosition.y <= Screen.height;

        if (!isOnScreen)
        {
            return false;
        }

        float cameraDistance = Mathf.Abs(mainCamera.transform.position.z);
        worldPosition = mainCamera.ScreenToWorldPoint(new Vector3(screenPosition.x, screenPosition.y, cameraDistance));
        return true;
    }

    private void ResizeSprite(SpriteRenderer targetSpriteRenderer)
    {
        if (targetSpriteRenderer == null || spriteWorldHeight <= 0f)
        {
            return;
        }

        float currentHeight = targetSpriteRenderer.bounds.size.y;

        if (currentHeight <= 0f)
        {
            return;
        }

        float uniformScale = spriteWorldHeight / currentHeight;
        transform.localScale = new Vector3(
            transform.localScale.x * uniformScale,
            transform.localScale.y * uniformScale,
            transform.localScale.z);
    }

    private bool IsPointerOverUi()
    {
        if (EventSystem.current == null)
        {
            return false;
        }

        if (Input.touchCount > 0)
        {
            return EventSystem.current.IsPointerOverGameObject(Input.GetTouch(0).fingerId);
        }

        return EventSystem.current.IsPointerOverGameObject();
    }

    private void PlayPressAnimation()
    {
        if (pressAnimation != null)
        {
            StopCoroutine(pressAnimation);
        }

        pressAnimation = StartCoroutine(AnimatePress());
    }

    private void PlayFrameAnimation()
    {
        if (!animateFramesOnClick)
        {
            return;
        }

        if (frameAnimation != null)
        {
            StopCoroutine(frameAnimation);
        }

        frameAnimation = StartCoroutine(AnimateFrames());
    }

    private void ShowFloatingOreText(int minedOre)
    {
        if (!showFloatingOreText || minedOre <= 0)
        {
            return;
        }

        GameObject textObject = new GameObject("MineFloatingOreText");
        textObject.transform.position = GetFloatingOreTextStartPosition();
        textObject.transform.rotation = Quaternion.identity;

        TextMesh textMesh = textObject.AddComponent<TextMesh>();
        textMesh.text = "+" + NumberFormatter.FormatInt(minedOre);
        textMesh.anchor = TextAnchor.MiddleCenter;
        textMesh.alignment = TextAlignment.Center;
        textMesh.fontSize = Mathf.Max(1, floatingOreTextFontSize);
        textMesh.characterSize = Mathf.Max(0.001f, floatingOreTextCharacterSize);
        textMesh.color = floatingOreTextColor;

        if (floatingOreTextFont != null)
        {
            textMesh.font = floatingOreTextFont;
        }

        MeshRenderer textRenderer = textObject.GetComponent<MeshRenderer>();

        if (textRenderer != null)
        {
            textRenderer.sortingOrder = sortingOrder + 30;
        }

        StartCoroutine(AnimateFloatingOreText(textObject, textMesh));
    }

    private Vector3 GetFloatingOreTextStartPosition()
    {
        Vector3 basePosition = transform.position + floatingOreTextOffset;
        Camera mainCamera = Camera.main;

        if (mainCamera == null)
        {
            return basePosition;
        }

        float cameraDistance = Mathf.Abs(basePosition.z - mainCamera.transform.position.z);
        Vector3 screenPosition = mainCamera.WorldToScreenPoint(basePosition);
        screenPosition.x += Random.Range(-floatingOreTextRandomOffsetPixels.x, floatingOreTextRandomOffsetPixels.x);
        screenPosition.y += Random.Range(-floatingOreTextRandomOffsetPixels.y, floatingOreTextRandomOffsetPixels.y);

        Vector3 worldPosition = mainCamera.ScreenToWorldPoint(new Vector3(
            screenPosition.x,
            screenPosition.y,
            cameraDistance));
        worldPosition.z = basePosition.z;
        return worldPosition;
    }

    private IEnumerator AnimateFloatingOreText(GameObject textObject, TextMesh textMesh)
    {
        if (textObject == null || textMesh == null)
        {
            yield break;
        }

        Vector3 startPosition = textObject.transform.position;
        Vector3 endPosition = startPosition + Vector3.up * Mathf.Max(0f, floatingOreTextRiseDistance);
        Color startColor = floatingOreTextColor;
        Color endColor = new Color(startColor.r, startColor.g, startColor.b, 0f);
        float duration = Mathf.Max(0.01f, floatingOreTextLifetime);
        float elapsed = 0f;

        while (elapsed < duration)
        {
            elapsed += Time.deltaTime;
            float progress = Mathf.Clamp01(elapsed / duration);
            float easedProgress = Mathf.SmoothStep(0f, 1f, progress);

            textObject.transform.position = Vector3.LerpUnclamped(startPosition, endPosition, easedProgress);
            textMesh.color = Color.LerpUnclamped(startColor, endColor, progress);
            yield return null;
        }

        Destroy(textObject);
    }

    private IEnumerator AnimatePress()
    {
        Vector3 pressedPosition = startLocalPosition + pressedOffset;

        yield return MoveTo(pressedPosition, pressedMoveDuration);
        yield return MoveTo(startLocalPosition, pressedMoveDuration);

        transform.localPosition = startLocalPosition;
        pressAnimation = null;
    }

    private IEnumerator MoveTo(Vector3 targetPosition, float duration)
    {
        Vector3 startPosition = transform.localPosition;

        if (duration <= 0f)
        {
            transform.localPosition = targetPosition;
            yield break;
        }

        float elapsed = 0f;

        while (elapsed < duration)
        {
            elapsed += Time.deltaTime;
            float progress = Mathf.Clamp01(elapsed / duration);
            float easedProgress = Mathf.SmoothStep(0f, 1f, progress);
            transform.localPosition = Vector3.LerpUnclamped(startPosition, targetPosition, easedProgress);
            yield return null;
        }

        transform.localPosition = targetPosition;
    }

    private IEnumerator AnimateFrames()
    {
        Sprite[] frames = GetFrames();

        if (spriteRenderer == null || frames == null || frames.Length == 0)
        {
            frameAnimation = null;
            yield break;
        }

        float frameDelay = 1f / Mathf.Max(1f, animationFramesPerSecond);

        for (int i = 0; i < frames.Length; i++)
        {
            if (frames[i] != null)
            {
                spriteRenderer.sprite = frames[i];
            }

            yield return new WaitForSeconds(frameDelay);
        }

        if (restoreFirstFrameAfterAnimation && idleSprite != null)
        {
            spriteRenderer.sprite = idleSprite;
        }

        frameAnimation = null;
    }

    private Sprite[] GetFrames()
    {
        if (animationFrames != null && animationFrames.Length > 0)
        {
            return animationFrames;
        }

        if (generatedFrames != null && generatedFrames.Length > 0)
        {
            return generatedFrames;
        }

        generatedFrames = GenerateFramesFromSpriteSheet();
        return generatedFrames;
    }

    private Sprite[] GenerateFramesFromSpriteSheet()
    {
        if (spriteRenderer == null || spriteRenderer.sprite == null || spriteRenderer.sprite.texture == null)
        {
            return null;
        }

        Texture2D texture = spriteRenderer.sprite.texture;
        int columns = Mathf.Max(1, generatedFrameColumns);
        int rows = Mathf.Max(1, generatedFrameRows);
        int frameCount = Mathf.Clamp(generatedFrameCount, 1, columns * rows);
        int frameWidth = texture.width / columns;
        int frameHeight = texture.height / rows;

        if (frameWidth <= 0 || frameHeight <= 0)
        {
            return null;
        }

        Sprite sourceSprite = spriteRenderer.sprite;
        Vector2 normalizedPivot = new Vector2(
            sourceSprite.pivot.x / Mathf.Max(1f, sourceSprite.rect.width),
            sourceSprite.pivot.y / Mathf.Max(1f, sourceSprite.rect.height));
        List<Sprite> frames = new List<Sprite>(frameCount);

        for (int i = 0; i < frameCount; i++)
        {
            int column = i % columns;
            int rowFromTop = i / columns;
            int row = rows - 1 - rowFromTop;
            Rect frameRect = new Rect(column * frameWidth, row * frameHeight, frameWidth, frameHeight);

            frames.Add(Sprite.Create(
                texture,
                frameRect,
                normalizedPivot,
                sourceSprite.pixelsPerUnit,
                0,
                SpriteMeshType.FullRect));
        }

        return frames.ToArray();
    }
}
