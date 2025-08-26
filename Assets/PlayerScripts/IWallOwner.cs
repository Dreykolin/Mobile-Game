using UnityEngine;

public interface IWallOwner
{
    void NotifyWallDestroyed(Collider2D wallCollider);
}