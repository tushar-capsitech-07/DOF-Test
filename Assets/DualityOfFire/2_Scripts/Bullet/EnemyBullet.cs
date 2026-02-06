using UnityEngine;

public class EnemyBullet : BulletController
{
    protected override Vector2 MoveDirection => -Vector2.right;
    protected override string TargetTag => "Player";
}
