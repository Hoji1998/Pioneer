using UnityEngine;
using UnityEngine.TextCore.Text;
using System.Collections;
using System.Collections.Generic;
using UnityEngine.Rendering.Universal;

public class Character : MonoBehaviour
{
    [Header("Character Component")]
    public CharacterType characterType;
    [SerializeField] private GameObject Model;
    [SerializeField] private bool authorizedFilp = true;
    public CharacterCondition characterCondition;
    public CharacterMoveState characterMoveState;
    public CharacterMoveState[] BlockMovementState;

    [Header("Damage Option")]
    public float headbuttDamage = 10f;

    [Header("Light Option")]
    public Light2D characterLight;

    [Header("Physics Option")]
    public float gravityScale = -30f;
    public float initialGravityScale = -55f;
    public float oneWayPlatformBuffer = 0.05f;
    [SerializeField] private float maxClimbAngle = 80f;
    [SerializeField] private float maxDescendAngle = 75f;
    [SerializeField] private float maximumGravityScale = -55f;
    [SerializeField] private LayerMask CollisionLayer;
    [SerializeField] private LayerMask OneWayPlatformLayer;
    [SerializeField] private LayerMask MovingPlatformLayer;
    [SerializeField] private LayerMask SandLayer;
    [SerializeField] private LayerMask PlayerLayer;
    public bool IsGrounded = false;
    public bool IsSand = false;
    public bool IsFlying = false;
    public bool IsOneWayPlatform = false;
    public bool IsMovingPlatform = false;
    public DirectionCollideState directionCollideState;

    [HideInInspector] public GameObject model { get => Model; }
    [HideInInspector] public LayerMask collisionLayer { get => CollisionLayer; }
    [HideInInspector] public LayerMask onewayPlatformLayer { get => OneWayPlatformLayer; }
    [HideInInspector] public LayerMask movingPlatformLayer { get => MovingPlatformLayer; }
    [HideInInspector] public LayerMask playerLayer { get => PlayerLayer; }
    [HideInInspector] public float CurrentSlopeAngle { get => currentSlopeAngle; }
    [HideInInspector] public float horizontalSpeed = 0f;
    [HideInInspector] public float verticalSpeed = 0f;
    [HideInInspector] public Room currentRoom;

    [Range(2, 100)] public int horizontalRayCount = 2;
    [Range(2, 100)] public int verticalRayCount = 2;
    [SerializeField] private float cliffDistance = 5f;

    [HideInInspector] public Animator animator;
    [HideInInspector] public Vector2 velocity;
    [HideInInspector] public Vector2 movingPlatformVelocity;
    [HideInInspector] public Health health;
    [HideInInspector] public CharacterJumping characterJumping;
    [HideInInspector] public CharacterDig characterDig;
    [HideInInspector] public CharacterWeaponAttack characterWeaponAttack;
    [HideInInspector] public CharacterAction characterAction;
    [HideInInspector] public CharacterMoving characterMoving;
    [HideInInspector] public CharacterDash characterDash;
    [HideInInspector] public Collider2D physicsCollider;
    [HideInInspector] public AiBrain aiBrain;
    [HideInInspector] public ColliderCorner2D colliderCorner2D;

    private float currentSlopeAngle = 0f;
    private float horizontalRaySpacing = 0f;
    private float verticalRaySpacing;
    private CollisionChecker2D collisionChecker2D;
    private float skinwidth = 0.015f;
    private bool IsRaycastVerticalPause = false;
    //https://www.youtube.com/watch?v=vfpnyz1a_no Raycast Ãæµ¹Ã³¸® ÂüÁ¶
    public enum CharacterType { Player, AI }
    public enum CharacterCondition { Normal, Stunned, Pause }
    public enum CharacterMoveState { Idle, Walking, Jumping, Falling, Attack, Dig, Hanging, Dash, Fly }

    public struct DirectionCollideState
    {
        public bool leftWall;
        public bool rightWall;
        public bool leftCliff;
        public bool rightCliff;
    }
    void Start()
    {
        Initialized();
    }

    private void Initialized()
    {
        characterCondition = CharacterCondition.Normal;
        characterMoveState = CharacterMoveState.Idle;
        animator = model.GetComponent<Animator>();
        physicsCollider = GetComponent<Collider2D>();
        health = GetComponent<Health>();
        characterJumping= GetComponent<CharacterJumping>();
        characterDig = GetComponent<CharacterDig>();
        characterWeaponAttack = GetComponent<CharacterWeaponAttack>();
        characterAction = GetComponent<CharacterAction>();
        characterMoving = GetComponent<CharacterMoving>();
        characterDash = GetComponent<CharacterDash>();

        if (characterType == CharacterType.AI)
        {
            aiBrain = GetComponent<AiBrain>();
        }
    }

    public void InitializedAnimation()
    {
        characterMoveState = Character.CharacterMoveState.Idle;
        animator.SetBool("Idle", true);
    }
    public void FlipCharacter(float flipValue)
    {
        if (!authorizedFilp)
            return;

        if (flipValue == 0)
            return;

        if (flipValue > 0)
            flipValue= 1f;
        else
            flipValue= -1f;

        gameObject.transform.localScale = new Vector2(flipValue, gameObject.transform.localScale.y);
    }

    private void FixedUpdate()
    {
        CalculateRaySpacing();
        UpdateColliderCorner2D();
        collisionChecker2D.Reset();
        currentSlopeAngle = 0f;
        IsOneWayPlatform = false;
        if (IsMovingPlatform)
        {
            velocity += movingPlatformVelocity;
        }
        IsMovingPlatform = false;
        Movement();

        //ÃµÀå, º®, ¹Ù´ÚÃ¼Å©
        IsGrounded = collisionChecker2D.down;
        directionCollideState.leftWall = collisionChecker2D.left;
        directionCollideState.rightWall = collisionChecker2D.right;
        directionCollideState.leftCliff = collisionChecker2D.leftCliff;
        directionCollideState.rightCliff = collisionChecker2D.rightCliff;

        if (collisionChecker2D.up || collisionChecker2D.down)
        {
            velocity.y = 0;
        }

        if (characterType != CharacterType.Player)
            return;

        horizontalSpeed = Input.GetAxisRaw("Horizontal");
        verticalSpeed = Input.GetAxisRaw("Vertical");
    }
    private void CheckAnimation()
    {
        foreach (CharacterMoveState blockState in BlockMovementState)
        {
            if (characterMoveState == blockState)
            {
                return;
            }
        }

        if (IsFlying)
        {
            return;
        }

        GravityScaleUpdate();

        if (characterCondition == CharacterCondition.Stunned)
            return;

        if (velocity.y > 0)
        {
            characterMoveState = Character.CharacterMoveState.Jumping;
        }
        else if (velocity.y < 0)
        {
            characterMoveState = Character.CharacterMoveState.Falling;
        }

        if (IsGrounded)
        {
            InitializedAnimation();
        }

        FlipCharacter(horizontalSpeed);
    }
    public void GravityScaleUpdate()
    {
        if (characterJumping != null)
        {
            characterJumping.LongJumpGravityCheck(this);
        }
        velocity.y += gravityScale * Time.deltaTime;
    }
    public void Movement()
    {
        CheckAnimation();

        //¼Ó·Â
        Vector3 currentVelocity = velocity * Time.deltaTime;
        collisionChecker2D.velocityOld = currentVelocity;

        if (currentVelocity.y < 0)
        {
            DescendSlope(ref currentVelocity);
        }

        if (currentVelocity.x != 0)
        {
            RaycastHorizontal(ref currentVelocity);
            RaycastCliff(ref currentVelocity);
        }

        if (currentVelocity.y < maximumGravityScale)
        {
            currentVelocity.y = maximumGravityScale;
        }

        if (currentVelocity.y != 0 && !IsRaycastVerticalPause)
        {
            RaycastVertical(ref currentVelocity);
        }

        transform.position += currentVelocity;
    }

    public bool JumpTo(float jumpForce, ref int currentJumpCount)
    {
        if (currentJumpCount > 0)
        {
            velocity.y = jumpForce;
            currentJumpCount--;

            return true;
        }

        return false;
    }

    public void MoveToHorizontal(float moveSpeed)
    {
        if (characterType == CharacterType.Player)
        {
            if (characterDig.IsDigStop)
                return;
        }
        

        //if (horizontalSpeed == 0 && IsSand)
        //{
        //    velocity.x *= 0.98f;
        //    return;
        //}
        if (moveSpeed == 0)
        {
            if (velocity.y == 0)
            {
                velocity.x = 0;
                return;
            }

            if (velocity.x > 0)
                velocity.x -= Time.deltaTime * 15f;
            else if (velocity.x < 0)
                velocity.x += Time.deltaTime * 15f;

            if (Mathf.Abs(velocity.x) <= Time.deltaTime * 15f)
                velocity.x = 0;
            return;
        }

        velocity.x += horizontalSpeed * moveSpeed * (Time.deltaTime * 5f);

        if (((horizontalSpeed < 0 && velocity.x > 0) || (horizontalSpeed > 0 && velocity.x < 0))
            && velocity.y == 0)
        {
            velocity.x = 0;
        }


        if ((horizontalSpeed < 0 && velocity.x < horizontalSpeed * moveSpeed)
            || (horizontalSpeed > 0 && velocity.x > horizontalSpeed * moveSpeed))
        {
            velocity.x = horizontalSpeed * moveSpeed;
        }
    }
    public void MoveToVertical(float moveSpeed)
    {
        //if (verticalSpeed == 0 && IsSand)
        //{
        //    velocity.y *= 0.98f;
        //    return;
        //}
        velocity.y += verticalSpeed * moveSpeed * (Time.deltaTime * 5f);

        if (verticalSpeed < 0 && velocity.y < verticalSpeed * moveSpeed)
        {
            velocity.y = verticalSpeed * moveSpeed;
        }
        if (verticalSpeed > 0 && velocity.y > verticalSpeed * moveSpeed)
        {
            velocity.y = verticalSpeed * moveSpeed;
        }
    }
    #region Raycast
    private void RaycastHorizontal(ref Vector3 velocity)
    {
        float direction = Mathf.Sign(velocity.x);
        float distance = Mathf.Abs(velocity.x) + skinwidth;
        Vector2 rayPosition = Vector2.zero;
        RaycastHit2D hit;

        for (int i = 0; i < horizontalRayCount; ++i)
        {
            rayPosition = direction == 1 ? colliderCorner2D.bottomRight : colliderCorner2D.bottomLeft;
            rayPosition += Vector2.up * horizontalRaySpacing * i;

            //±¤¼± ¹ß»ç
            hit = Physics2D.Raycast(rayPosition, Vector2.right * direction, distance, collisionLayer);

            //±¤¼±¿¡ ºÎµúÈû
            if (hit)
            {
                if (hit.collider.gameObject.layer == 10)
                {
                    return;
                }

                float slopeAngle = Vector2.Angle(hit.normal, Vector2.up);
                if (i == 0 && slopeAngle <= maxClimbAngle)
                {
                    if (collisionChecker2D.descendingSlope)
                    {
                        collisionChecker2D.descendingSlope = false;
                        velocity = collisionChecker2D.velocityOld;
                    }
                    float distanceToSlopeSstart = 0f;
                    if (slopeAngle != collisionChecker2D.slopeAngleOld)
                    {
                        distanceToSlopeSstart = hit.distance - skinwidth;
                        velocity.x -= distanceToSlopeSstart * direction;
                    }
                    ClimbSlope(ref velocity, slopeAngle);
                    velocity.x += distanceToSlopeSstart * direction;
                }

                if (!collisionChecker2D.climbingSlope || slopeAngle > maxClimbAngle)
                {
                    velocity.x = (hit.distance - skinwidth) * direction;
                    distance = hit.distance;

                    if (collisionChecker2D.climbingSlope)
                    {
                        velocity.y = Mathf.Tan(collisionChecker2D.slopeAngle * Mathf.Deg2Rad) * Mathf.Abs(velocity.x);
                    }

                    //ÇöÀç ÀÌµ¿ ¹æÇâ¿¡ µû¸¥ 1, -1
                    collisionChecker2D.left = direction == -1;
                    collisionChecker2D.right = direction == 1;
                }
            }

            Debug.DrawLine(rayPosition, rayPosition + Vector2.right * direction * distance, Color.yellow);
        }
    }
    private void ClimbSlope(ref Vector3 velocity, float slopeAngle)
    {
        float moveDistance = Mathf.Abs(velocity.x);
        float climbVelocityY = Mathf.Sin(slopeAngle * Mathf.Deg2Rad) * moveDistance;

        if (velocity.y <= climbVelocityY)
        {
            velocity.y = climbVelocityY;
            velocity.x = Mathf.Cos(slopeAngle * Mathf.Deg2Rad) * moveDistance * Mathf.Sign(velocity.x);
            collisionChecker2D.down = true;
            collisionChecker2D.climbingSlope = true;
            collisionChecker2D.slopeAngle = slopeAngle;
            currentSlopeAngle = slopeAngle;
        }
    }
    private void DescendSlope(ref Vector3 velocity)
    {
        float directionX = Mathf.Sign(velocity.x);
        Vector2 rayOrigin = (directionX == -1) ? colliderCorner2D.bottomRight : colliderCorner2D.bottomLeft;
        RaycastHit2D slopeHit = Physics2D.Raycast(rayOrigin, -Vector2.up, Mathf.Infinity, collisionLayer);

        if (slopeHit)
        {
            float slopeAngle = Vector2.Angle(slopeHit.normal, Vector2.up);
            if (slopeAngle != 0 && slopeAngle <= maxDescendAngle)
            {
                if (Mathf.Sign(slopeHit.normal.x) == directionX)
                {
                    if (slopeHit.distance - skinwidth <= Mathf.Tan(slopeAngle * Mathf.Deg2Rad) * Mathf.Abs(velocity.x))
                    {
                        float moveDistance = Mathf.Abs(velocity.x);
                        float descendVelocityY = Mathf.Sin(slopeAngle * Mathf.Deg2Rad) * moveDistance;
                        velocity.x = Mathf.Cos(slopeAngle * Mathf.Deg2Rad) * moveDistance * Mathf.Sign(velocity.x);
                        velocity.y -= descendVelocityY;

                        collisionChecker2D.slopeAngle = slopeAngle;
                        collisionChecker2D.descendingSlope = true;
                        collisionChecker2D.down = true;
                        currentSlopeAngle = -slopeAngle;
                    }
                }
            }
        }

    }
    private void RaycastCliff(ref Vector3 velocity)
    {
        float direction = Mathf.Sign(velocity.x);
        float distance = Mathf.Abs(velocity.x) + cliffDistance;
        Vector2 rayPosition = Vector2.zero;
        RaycastHit2D hit;

        rayPosition = direction == 1 ? new Vector2(colliderCorner2D.bottomRight.x + (skinwidth * 2f), colliderCorner2D.bottomRight.y) 
            : new Vector2(colliderCorner2D.bottomLeft.x - (skinwidth * 2f), colliderCorner2D.bottomLeft.y);
        //±¤¼± ¹ß»ç
        hit = Physics2D.Raycast(rayPosition, Vector2.down, distance, collisionLayer);

        //±¤¼±¿¡ ºÎµúÈû
        if (hit.collider == null)
        {
            //º® Ã¼Å© ¿¹½Ã
            //velocity.x = (hit.distance - skinwidth) * direction;
            //distance = hit.distance;

            //ÇöÀç ÀÌµ¿ ¹æÇâ¿¡ µû¸¥ 1, -1
            collisionChecker2D.leftCliff = direction == -1;
            collisionChecker2D.rightCliff = direction == 1;
        }

        Debug.DrawLine(rayPosition, rayPosition + Vector2.down * distance, Color.yellow);
    }
    private void RaycastVertical(ref Vector3 velocity)
    {
        float direction = Mathf.Sign(velocity.y);
        float distance = Mathf.Abs(velocity.y) + skinwidth;
        Vector2 rayPosition = Vector2.zero;
        RaycastHit2D hit;

        if (characterType == CharacterType.Player)
        {
            this.gameObject.transform.parent = null;
        }
        
        for (int i = 0; i < verticalRayCount; ++i)
        {
            rayPosition = direction == 1 ? colliderCorner2D.topLeft : colliderCorner2D.bottomLeft;
            rayPosition += Vector2.right * (verticalRaySpacing * i + velocity.x);

            //±¤¼± ¹ß»ç
            hit = Physics2D.Raycast(rayPosition, Vector2.up * direction, distance, collisionLayer);

            //±¤¼±¿¡ ºÎµúÈû
            if (hit)
            {
                if ((hit.collider.gameObject.layer == 10 || hit.collider.gameObject.layer == 16)
                    && direction == 1)
                {
                    return;
                }

                if (collisionChecker2D.climbingSlope)
                {
                    velocity.x = velocity.y / Mathf.Tan(collisionChecker2D.slopeAngle * Mathf.Deg2Rad) * Mathf.Sign(velocity.x);
                }


                velocity.y = (hit.distance - skinwidth) * direction;
                distance = hit.distance;

                collisionChecker2D.down = direction == -1;
                collisionChecker2D.up = direction == 1;

                if ((hit.collider.gameObject.layer == 10 || hit.collider.gameObject.layer == 16) 
                    && direction == -1)
                {
                    IsOneWayPlatform = true;
                }

                if (hit.collider.gameObject.layer == 13
                    || hit.collider.gameObject.layer == 16)
                {
                    IsMovingPlatform = true;
                }
            }

            if (collisionChecker2D.climbingSlope)
            {
                float directionX = Mathf.Sign(velocity.x);
                distance = Mathf.Abs(velocity.x) + skinwidth;
                Vector2 rayOrigin = (directionX == -1 ? colliderCorner2D.bottomLeft : colliderCorner2D.bottomRight) + Vector2.up * velocity.y;
                RaycastHit2D slopeHit = Physics2D.Raycast(rayOrigin, Vector2.right * directionX, distance, collisionLayer);

                if (slopeHit)
                {
                    float slopeAngle = Vector2.Angle(slopeHit.normal, Vector2.up);
                    if (slopeAngle != collisionChecker2D.slopeAngle)
                    {
                        velocity.x = (slopeHit.distance - skinwidth) * directionX;
                        collisionChecker2D.slopeAngle = slopeAngle;
                    }
                }
            }
            Debug.DrawLine(rayPosition, rayPosition + Vector2.up * direction * distance, Color.yellow);
        }
    }
    public void RaycastVerticalPauseEvent()
    {
        IsRaycastVerticalPause = true;
        StartCoroutine(RaycastVerticalPauseStart());
    }
    private IEnumerator RaycastVerticalPauseStart()
    {
        yield return new WaitForSeconds(oneWayPlatformBuffer);
        IsRaycastVerticalPause = false;
    }
    private void CalculateRaySpacing()
    {
        Bounds bounds = physicsCollider.bounds;
        bounds.Expand(skinwidth * -2);

        horizontalRaySpacing = bounds.size.y / (horizontalRayCount - 1);
        verticalRaySpacing = bounds.size.x / (verticalRayCount - 1);
    }
    private void UpdateColliderCorner2D()
    {
        Bounds bounds = physicsCollider.bounds;
        bounds.Expand(skinwidth * -2);

        colliderCorner2D.topLeft = new Vector2(bounds.min.x, bounds.max.y);
        colliderCorner2D.bottomLeft = new Vector2(bounds.min.x, bounds.min.y);
        colliderCorner2D.bottomRight = new Vector2(bounds.max.x, bounds.min.y);
    }

    public struct ColliderCorner2D
    {
        public Vector2 topLeft;
        public Vector2 bottomLeft;
        public Vector2 bottomRight;
    }

    private struct CollisionChecker2D
    {
        public void Reset()
        {
            left = false;
            right = false;
            up = false;
            down = false;
            leftCliff = false;
            rightCliff = false;
            climbingSlope = false;
            descendingSlope = false;

            slopeAngleOld = slopeAngle;
            slopeAngle = 0f;
        }

        public bool climbingSlope;
        public bool descendingSlope;

        public float slopeAngle, slopeAngleOld;
        public Vector3 velocityOld;

        public bool left;
        public bool right;
        public bool up;
        public bool down;

        public bool leftCliff;
        public bool rightCliff;
    }
    #endregion
    private void OnTriggerStay2D(Collider2D collision)
    {
        if (characterType != CharacterType.Player)
            return;

        GameObject sandObject = collision.gameObject;
        if (sandObject.layer != 7)
            return;
        if (characterCondition == CharacterCondition.Stunned)
            return;

        IsSand = true;
    }
    private void OnTriggerExit2D(Collider2D collision)
    {
        if (characterType != CharacterType.Player)
            return;

        GameObject sandObject = collision.gameObject;
        if (sandObject.layer != 7)
            return;

        IsSand = false;
        characterDig.StopAbility();
    }
}
