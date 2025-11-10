using UnityEngine;

public class Breakable : Block
{
    [Header("Breakable Settings")]
    [SerializeField] private int maxPushes = 4;
    [SerializeField] private GameObject breakEffectPrefab;
    [SerializeField] private GameManager gameManager;

    private int currentPushes = 0;
    private bool pushedByPlayer = false;

    protected override void StartMove(Cell newParent, int _deltaX, int _deltaY)
    {
        // Check if pushed by player
        int behindX = gridPos.x - _deltaX;
        int behindY = gridPos.y - _deltaY;

        if (behindX >= 0 && behindY >= 0 &&
            behindX < gridManager.gridList.Count &&
            behindY < gridManager.gridList[0].Count)
        {
            Cell behindCell = gridManager.gridList[behindX][behindY].GetComponent<Cell>();
            if (behindCell.CheckContainObj())
            {
                GameObject behindObj = behindCell.ContainObj;
                pushedByPlayer = behindObj.GetComponent<Player>() != null;
            }
        }

        // Clear target on old cell
        Cell oldCell = gridManager.gridList[gridPos.x][gridPos.y].GetComponent<Cell>();
        Target oldTarget = oldCell.GetComponentInChildren<Target>();
        if (oldTarget != null) oldTarget.SetOccupied(false);

        // Move block and update gridPos
        base.StartMove(newParent, _deltaX, _deltaY);

        Debug.Log($"{gameObject.name} moved to grid position: {gridPos}");

        // Notify GameManager
        gameManager?.OnBlockMoved();
    }

    private void BlockMoved(Vector2Int direction)
    {
        if (pushedByPlayer)
        {
            currentPushes++;
            if (currentPushes >= maxPushes)
            {
                BreakBlock();
                return;
            }
        }
        pushedByPlayer = false;
    }

    private void BreakBlock()
    {
        if (breakEffectPrefab != null)
            Instantiate(breakEffectPrefab, transform.position, Quaternion.identity);

        Cell currentCell = gridManager.gridList[gridPos.x][gridPos.y].GetComponent<Cell>();
        currentCell.RemoveContainObj();
        Destroy(gameObject);
    }

    // -------------------------------
    // Reset block to a start position
    // -------------------------------
    public void ResetToStart(Vector2Int startPos)
    {
        // Remove from old cell if valid
        if (gridPos != Vector2Int.zero)
        {
            Cell oldCell = gridManager.gridList[gridPos.x][gridPos.y].GetComponent<Cell>();
            oldCell.RemoveContainObj();
        }

        // Update gridPos
        gridPos = startPos;

        // Place in new cell
        Cell newCell = gridManager.gridList[gridPos.x][gridPos.y].GetComponent<Cell>();
        newCell.ContainObj = gameObject;

        // Update visual
        transform.position = newCell.transform.position;
    }
}
