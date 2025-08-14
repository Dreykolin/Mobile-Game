using UnityEngine;

public interface IBombOwner
{
    void NotifyBombDestroyed(Collider2D bombCollider);
}
