using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem;

public class InteractionManager : MonoBehaviour
{
    [SerializeField] private Camera cam;
    public GameObject GO_test;
    private float lastClickTime;
    private const float doubleClickDelay = 0.25f;

    void Update()
    {
        InteractionContext context = new InteractionContext
        {
            screenPosition = Mouse.current.position.ReadValue(),
            camera = cam
        };
        HandleHover(context);
        HandleRotation();
        HandleCancel();
        if (!Mouse.current.leftButton.wasPressedThisFrame)
            return;



        bool isDouble = IsDoubleClick();

        if (HandleUI(context, isDouble)) return;
        HandleWorld(context, isDouble);
    }

    bool IsDoubleClick()
    {
        return false;// erase this if double click is in game
        /*float time = Time.time;

        if (time - lastClickTime < doubleClickDelay)
        {
            lastClickTime = 0;
            return true;
        }

        lastClickTime = time;
        return false;*/
    }

    bool HandleUI(InteractionContext context, bool isDouble)
    {
        PointerEventData pointerData = new PointerEventData(EventSystem.current)
        {
            position = context.screenPosition
        };

        List<RaycastResult> results = new List<RaycastResult>();
        EventSystem.current.RaycastAll(pointerData, results);

        foreach (var result in results)
        {
            if (result.gameObject.TryGetComponent<IInteractable>(out var interactable))
            {
                if (interactable.CanInteract(context))
                {
                    if (isDouble)
                        interactable.OnDoubleInteract(context);
                    else
                        interactable.OnInteract(context);

                    return true;
                }
            }
        }

        return false;
    }

    void HandleWorld(InteractionContext context, bool isDouble)
    {
        Ray ray = context.camera.ScreenPointToRay(context.screenPosition);

        if (!Physics.Raycast(ray, out RaycastHit hit))
        {
            Debug.Log("raycastWorld Didn't Hit a thing");
            return;
        }

        // ==========================
        // UNIT SELECTION
        // =========================

        if (hit.collider.TryGetComponent<IInteractable>(out var interactableUnit))
        {
            interactableUnit.OnInteract(context);
        }

        // ==========================
        // TILE INTERACTION
        // =========================

        BaseTile tile = hit.collider.GetComponent<BaseTile>();

        if (tile != null)
        {
            // ---------- MOVE UNIT ----------
            if (UnitSelectionSystem.Instance.SelectedUnit != null)
            {
                UnitSelectionSystem.Instance.MoveSelected(tile.ceilClass.hexCoord);

                return;
            }

            // ---------- TILE INTERACTION ----------
            if (hit.collider.TryGetComponent<IInteractable>(out var interactableTile))
            {
                if (interactableTile.CanInteract(context))
                {
                    if (isDouble)
                        interactableTile.OnDoubleInteract(context);
                    else
                        interactableTile.OnInteract(context);
                }
            }

            // ---------- BUILD SYSTEM ----------
            if (PreviewSystem.Instance.GetCurrentPrefab() != null)
            {
                CommandInvoker.Instance.Execute( new PlaceTileCommand(hit.point));
            }
        }
    }


    //CommandInvoker.Instance.Execute(new PlaceTileCommand(hit.point));


    void HandleHover(InteractionContext context)
    {
        if (PreviewSystem.Instance.GetCurrentPrefab() == null)
            return;

        Ray ray = context.camera.ScreenPointToRay(context.screenPosition);

        if (Physics.Raycast(ray, out RaycastHit hit))
        {
            PreviewSystem.Instance.UpdatePreview(hit.point);
        }
    }

    void HandleRotation()
    {
        if (Keyboard.current.qKey.wasPressedThisFrame)
            PreviewSystem.Instance.Rotate(-1);

        if (Keyboard.current.eKey.wasPressedThisFrame)
            PreviewSystem.Instance.Rotate(1);
    }


    void HandleCancel()
    {
        if (Keyboard.current.escapeKey.wasPressedThisFrame)
        {
            PreviewSystem.Instance.ClearPreview();
            ShopSystem.Instance.currentSelectedPrefab = null;
        }
    }
}