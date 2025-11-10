using UnityEngine;

public class Target : Cell
{
    public bool hasCorrectBlock = false;

    public void SetOccupied(bool state)
    {
        hasCorrectBlock = state;
        
        if (state)
            Debug.Log($"Block placed on target '{name}' at position {transform.position}");
        else
            Debug.Log($"Block removed from target '{name}' at position {transform.position}");
    }

    public bool IsFilled() => hasCorrectBlock;
    
}
