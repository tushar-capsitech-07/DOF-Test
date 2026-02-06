using UnityEngine;

public class PlayerBullet : BulletController
{
    protected override Vector2 MoveDirection => Vector2.right;
    protected override string TargetTag => "AIGun";
}
