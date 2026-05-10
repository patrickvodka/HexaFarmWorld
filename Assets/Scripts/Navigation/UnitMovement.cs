using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEditor.ShaderGraph;
using UnityEngine;
[RequireComponent(typeof(UnitInteractable))]
public class UnitMovement : MonoBehaviour
{
    public float moveSpeed = 5f;
    public float delayBeforeInit;
    public Vector3 CurrentHex;
    private Coroutine moveRoutine;
    public Material outlineMat;
    private Material runtimeOutlineMat;
    public bool IsMoving { get; private set; }

    private void Awake()
    {
        var matList = new List<Material>();
        var meshRend = GetComponent<MeshRenderer>();
        if (meshRend != null)
        {
            meshRend.GetMaterials(matList);
            matList.Add(outlineMat);
            meshRend.SetMaterials(matList);
            runtimeOutlineMat = meshRend.materials[meshRend.materials.Length - 1];
        }
        else 
        {
            Debug.LogWarning("No mesh renderer here ! ");
        }
    }
    private void Start()
    {
        UnitSelectionSystem.Instance.UnitSelected += HandleSelect;
        UnitSelectionSystem.Instance.UnitDeselected += HandleDeselect;
        StartCoroutine(GetSpawnPoint(delayBeforeInit));
    }
    #region Events
    
    private void HandleSelect(UnitMovement selectedUnit)
    {
        bool isMe = selectedUnit == this;

        SetOutlineVisual(isMe);
    }

    private void HandleDeselect(UnitMovement deselectedUnit)
    {
        if (deselectedUnit == this)
        {
            SetOutlineVisual(false);
        }
    }
    #endregion Events
    public void MoveTo(Vector3 targetHex)
    {
        Vector3 startHex = GameManager.Instance.wfc.FindClosestHex(transform.position);

        List<Vector3> path = HexPathfinding.Instance.FindPath(startHex, targetHex);

        if (path == null || path.Count == 0)
        {
            Debug.Log("No path");
            return;
        }

        if (moveRoutine != null)
        {
            StopCoroutine(moveRoutine);
        }

        moveRoutine = StartCoroutine(FollowPath(path));
    }
    private void OnDestroy()
    {
        if (UnitSelectionSystem.Instance == null)
        return;
        UnitSelectionSystem.Instance.UnitSelected -= HandleSelect;
        UnitSelectionSystem.Instance.UnitDeselected -= HandleDeselect;
    }

    private IEnumerator FollowPath(List<Vector3> path)
    {
        IsMoving = true;

        foreach (var hex in path)
        {
            Vector3 worldPos = GameManager.Instance.wfc.HexToWorldPosition(hex);

            Vector3 targetPos = new Vector3(worldPos.x, transform.position.y, worldPos.z);

            while (Vector3.Distance(transform.position, targetPos) > 0.05f)
            {
                transform.position = Vector3.MoveTowards(transform.position, targetPos, moveSpeed * Time.deltaTime);
                yield return null;
            }

            transform.position = targetPos;
            CurrentHex = hex;
        }

        IsMoving = false;
        moveRoutine = null;
    }

    private IEnumerator GetSpawnPoint(float delay)
    {
        yield return new WaitForSeconds(delay);

        CurrentHex = GameManager.Instance.wfc.FindClosestHex(transform.position);
    }
    #region visual

    public void SetOutlineVisual(bool activate)
    {
        if (activate)
        {
            runtimeOutlineMat.SetFloat("_outlineScale", 1.1f);
        }
        else
        {
            runtimeOutlineMat.SetFloat("_outlineScale", 0f);
        }
    }

    #endregion visual 
}
