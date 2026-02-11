using UnityEngine;


public class NetworkEnemyBullet : NetworkBulletController
{
    protected override string TargetTag => "Player";
    protected override Vector2 MoveDirection => Vector2.left;
}