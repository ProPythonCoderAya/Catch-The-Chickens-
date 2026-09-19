using System;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem;
using System.Collections.Generic;
using UnityEngine.UI;

public class UIPointerHandler : MonoBehaviour
{
    [Serializable]
    public class UIScreen
    {
        public Canvas canvas;
        public GameObject hitbox;
        [HideInInspector]
        public GraphicRaycaster canvasRaycaster;
        [HideInInspector]
        public RectTransform rectTransform;
    }

    private class DebugData
    {
        public Ray DebugRay;
        public bool DebugRayValid;
        public RaycastHit DebugHit;
        public bool DebugHitValid;
    }
    
    [Header("References")]
    public Camera playerCamera;
    public List<UIScreen> screens = new();

    [Header("Ray Settings")]
    public LayerMask hitMask = ~0;
    public float rayDistance = 5f;

    private GameObject _currentHover;
    private PointerEventData _pointerData;
    private EventSystem _eventSystem;
    private GameObject _dragObject;
    private GameObject _pointerDownObject;
    private readonly DebugData _debugData = new();
    private Vector2 _previousPointerPosition;

    private readonly List<RaycastResult> _results = new();

    private void Start()
    {
        foreach (UIScreen screen in screens)
        {
            screen.canvasRaycaster = screen.canvas.GetComponent<GraphicRaycaster>();
            screen.rectTransform = screen.canvas.GetComponent<RectTransform>();
        }
        _eventSystem = EventSystem.current;

        if (_eventSystem == null)
        {
            Debug.LogError("EventSystem missing.");
            enabled = false;
            return;
        }

        _pointerData = new PointerEventData(_eventSystem);
    }

    void Update()
    {
        var mouse = Mouse.current;
        if (mouse == null || !EventSystem.current) return;
        
        foreach (var screen in screens)
        {
            Vector2 center = new Vector2(Screen.width * 0.5f, Screen.height * 0.5f);
            var ray = playerCamera.ScreenPointToRay(center);

            _debugData.DebugRay = ray;
            _debugData.DebugRayValid = true;
            _debugData.DebugHitValid = false;

            if (!Physics.Raycast(ray, out RaycastHit hit, rayDistance, hitMask))
            {
                ClearHover();
                continue;
            }
            if (hit.collider.gameObject != screen.hitbox) continue; // we didnt hit the screen
            
            _debugData.DebugHit = hit;
            _debugData.DebugHitValid = true;
            
            
            Vector2 uv = Helpers.GetUV(hit);
            
            
            if (screen.canvas.renderMode == RenderMode.ScreenSpaceOverlay) continue;

            Vector2 screenPos = new Vector2(0f, 0f);
            if (screen.canvas.renderMode == RenderMode.ScreenSpaceCamera)
            {
                RenderTexture rt = screen.canvas.worldCamera.targetTexture;

                if (!rt)
                {
                    ClearHover();
                    continue;
                }

                screenPos = new Vector2(
                    uv.x * rt.width,
                    uv.y * rt.height
                );
            }
            else if (screen.canvas.renderMode == RenderMode.WorldSpace)
            {
                RectTransform rt = screen.rectTransform;
                
                screenPos = new Vector2(
                    uv.x * rt.rect.width,
                    uv.y * rt.rect.height
                );
            }

            
            _pointerData.position = screenPos;
            _pointerData.delta = (screenPos - _previousPointerPosition) / 2.0f;
            _previousPointerPosition = screenPos;
            _pointerData.scrollDelta = mouse.scroll.ReadValue();

            
            _results.Clear();
            screen.canvasRaycaster.Raycast(_pointerData, _results);

            GameObject newHover = _results.Count > 0 ? _results[0].gameObject : null;

            HandleHover(newHover);
            HandleMouseButtons(newHover);
            HandleScroll(newHover);
            return;
        }
    }

    // =========================================================
    // HOVER
    // =========================================================
    void HandleHover(GameObject newHover)
    {
        if (_currentHover != newHover)
        {
            if (_currentHover)
                ExecuteEvents.ExecuteHierarchy(_currentHover, _pointerData, ExecuteEvents.pointerExitHandler);

            if (newHover)
                ExecuteEvents.ExecuteHierarchy(newHover, _pointerData, ExecuteEvents.pointerEnterHandler);

            _currentHover = newHover;
        }

        if (_currentHover)
        {
            ExecuteEvents.ExecuteHierarchy(_currentHover, _pointerData, ExecuteEvents.pointerMoveHandler);
        }
    }

    // =========================================================
    // MOUSE
    // =========================================================
    void HandleMouseButtons(GameObject target)
    {
        var mouse = Mouse.current;
        if (mouse == null) return;

        if (mouse.leftButton.wasPressedThisFrame && target)
        {
            _pointerData.button = PointerEventData.InputButton.Left;

            _pointerDownObject =
                ExecuteEvents.GetEventHandler<IPointerClickHandler>(target);
            print(_pointerDownObject);

            _dragObject =
                ExecuteEvents.GetEventHandler<IDragHandler>(target);

            ExecuteEvents.ExecuteHierarchy(
                _pointerDownObject,
                _pointerData,
                ExecuteEvents.pointerDownHandler
            );

            if (_dragObject)
            {
                ExecuteEvents.Execute(
                    _dragObject,
                    _pointerData,
                    ExecuteEvents.beginDragHandler
                );
            }
        }

        if (mouse.leftButton.isPressed && _dragObject)
        {
            ExecuteEvents.Execute(
                _dragObject,
                _pointerData,
                ExecuteEvents.dragHandler
            );
        }

        if (mouse.leftButton.wasReleasedThisFrame)
        {
            _pointerData.button = PointerEventData.InputButton.Left;

            if (_pointerDownObject)
            {
                ExecuteEvents.ExecuteHierarchy(
                    _pointerDownObject,
                    _pointerData,
                    ExecuteEvents.pointerUpHandler
                );

                ExecuteEvents.ExecuteHierarchy(
                    _pointerDownObject,
                    _pointerData,
                    ExecuteEvents.pointerClickHandler
                );
            }

            if (_dragObject)
            {
                ExecuteEvents.Execute(
                    _dragObject,
                    _pointerData,
                    ExecuteEvents.endDragHandler
                );
            }

            _pointerDownObject = null;
            _dragObject = null;
        }

        if (mouse.rightButton.wasPressedThisFrame && target)
        {
            _pointerData.button = PointerEventData.InputButton.Right;

            ExecuteEvents.ExecuteHierarchy(
                target,
                _pointerData,
                ExecuteEvents.pointerDownHandler
            );
        }

        if (mouse.rightButton.wasReleasedThisFrame && target)
        {
            _pointerData.button = PointerEventData.InputButton.Right;

            ExecuteEvents.ExecuteHierarchy(
                target,
                _pointerData,
                ExecuteEvents.pointerUpHandler
            );

            ExecuteEvents.ExecuteHierarchy(
                target,
                _pointerData,
                ExecuteEvents.pointerClickHandler
            );
        }
    }

    void HandleScroll(GameObject target)
    {
        var mouse = Mouse.current;
        if (mouse == null || !target) return;

        Vector2 scroll = mouse.scroll.ReadValue();

        if (scroll.sqrMagnitude > 0.01f)
        {
            _pointerData.scrollDelta = scroll;
            ExecuteEvents.ExecuteHierarchy(target, _pointerData, ExecuteEvents.scrollHandler);
        }
    }

    // =========================================================
    // CLEAR HOVER
    // =========================================================
    void ClearHover()
    {
        if (!_currentHover) return;

        ExecuteEvents.ExecuteHierarchy(_currentHover, _pointerData, ExecuteEvents.pointerExitHandler);
        _currentHover = null;
    }
    
    private void OnDrawGizmos()
    {
        if (!_debugData.DebugRayValid)
            return;

        // Ray
        Gizmos.color = Color.green;
        
        Vector3 hitPoint = _debugData.DebugHit.point;

        Vector3 rayEnd = _debugData.DebugHitValid
            ? hitPoint
            : _debugData.DebugRay.origin + _debugData.DebugRay.direction * rayDistance;

        Gizmos.DrawLine(_debugData.DebugRay.origin, rayEnd);

        // Hit point
        if (_debugData.DebugHitValid)
        {
            Gizmos.color = Color.red;
            Gizmos.DrawSphere(hitPoint, 0.02f);

            // Little cross at hit point
            float size = 0.05f;

            Gizmos.DrawLine(
                hitPoint - Vector3.right * size,
                hitPoint + Vector3.right * size
            );

            Gizmos.DrawLine(
                hitPoint - Vector3.up * size,
                hitPoint + Vector3.up * size
            );

            Gizmos.DrawLine(
                hitPoint - Vector3.forward * size,
                hitPoint + Vector3.forward * size
            );
        }
    }
}