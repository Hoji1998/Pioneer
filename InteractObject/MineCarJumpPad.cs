using UnityEngine;

public class MineCarJumpPad : MonoBehaviour
{
    [Header("MineCarJumpPad Component")]
    [SerializeField] private float power = 5f;
    [SerializeField] private MineCar.MoveDirection moveDirection;

    [Header("Effect")]
    [SerializeField] private GameObject jumpParticle;

    private MineCar mineCar;
    private void OnTriggerEnter2D(Collider2D collision)
    {
        mineCar = collision.gameObject.GetComponent<MineCar>();

        if (mineCar == null)
            return;
        if (mineCar.mineCarMoveDrection != moveDirection)
            return;

        JumpPadOperate();
    }
    private void JumpPadOperate()
    {
        Instantiate(jumpParticle, transform);
        mineCar.GetComponent<Character>().velocity.y += power;
    }
}
