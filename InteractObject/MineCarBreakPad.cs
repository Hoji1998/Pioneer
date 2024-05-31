using UnityEngine;

public class MineCarBreakPad : MonoBehaviour
{
    [Header("MineCarBreakPad Component")]
    [SerializeField] private MineCar.MoveDirection moveDirection;

    [Header("Effect")]
    [SerializeField] private GameObject breakParticle;

    private MineCar mineCar;
    private void OnTriggerEnter2D(Collider2D collision)
    {
        mineCar = collision.gameObject.GetComponent<MineCar>();

        if (mineCar == null)
            return;
        if (mineCar.mineCarMoveDrection != moveDirection)
            return;

        BreakPadOperate();
    }
    private void BreakPadOperate()
    {
        Instantiate(breakParticle, transform);
        mineCar.BreakSequance();
    }
}
