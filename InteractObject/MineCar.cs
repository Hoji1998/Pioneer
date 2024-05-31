using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MineCar : FinishAttackPoint
{
    [Header("MineCar Component")]
    [SerializeField] private float speed = 5f;
    [SerializeField] private MoveDirection moveDirection;
    [SerializeField] private GameObject model;

    [Header("Particle")]
    [SerializeField] private GameObject frictionParticle;
    [SerializeField] private GameObject frictionParticlePos;
    [SerializeField] private GameObject destroyFeedback;

    [HideInInspector] public MoveDirection mineCarMoveDrection { get => moveDirection; }
    public enum MoveDirection { Right, Left }
    private Character character;
    private Coroutine coroutine;
    private LevelManager levelManager;
    private bool IsRide = false;
    private bool IsMove = false;
    private float directionValue = 0f;
    private Vector3 initialTr = Vector3.zero;
    private MoveDirection initialMoveDirection;
    private SpriteRenderer modelSprite;
    public override void Start()
    {
        base.Start();
    }
    public override void OnDrawGizmos()
    {
        base.OnDrawGizmos();
        Gizmos.color = Color.green;
        Gizmos.DrawWireSphere(transform.position, 0.5f);
    }
    public override void FinishObjectFeedback()
    {
        RideCar();
        MoveStart();
    }
    private void OnEnable()
    {
        Initialized();
        transform.position = initialTr;
    }
    public void Initialized()
    {
        StopAllCoroutines();
        if (character == null)
        {
            character = GetComponent<Character>();
        }
        if (modelSprite == null)
        {
            modelSprite = model.GetComponent<SpriteRenderer>();
        }
        if (levelManager == null)
        {
            levelManager = LevelManager.Instance;
        }
        if (initialTr == Vector3.zero)
        {
            initialTr = transform.position;
            initialMoveDirection = moveDirection;
            return;
        }
        transform.rotation = Quaternion.identity;
        moveDirection = initialMoveDirection;

        character.velocity = Vector2.zero;
        IsMove = false;

        modelSprite.color = Color.white;
    }
    private void RideCar()
    {
        if (base.playerCharacter == null)
        {
            base.playerCharacter = levelManager.playerCharacter.GetComponent<Character>();
        }
        base.StartHitFeedback();
        IsRide = true;
        base.playerCharacter.characterMoveState = Character.CharacterMoveState.Hanging;
        levelManager.ShakeCameraEvent(5f, 0.1f);
    }
    private void MoveStart()
    {
        if (IsMove)
            return;

        IsMove = true;
        MoveDirectionCheck();
        coroutine = StartCoroutine(Move());
    }
    private void MoveDirectionCheck()
    {
        switch (moveDirection)
        {
            case MoveDirection.Left:
                transform.localScale = new Vector3(-1f, 1f, 1f);
                directionValue = -1f;
                break;
            case MoveDirection.Right:
                transform.localScale = Vector3.one;
                directionValue = 1f;
                break;
        }
    }
    private void ChangeMoveDirection()
    {
        moveDirection = moveDirection == MoveDirection.Left ? MoveDirection.Right : MoveDirection.Left;
    }
    private void SetMoveDirection(MoveDirection value)
    {
        moveDirection = value;
    }
    private void MoveComplete()
    {
        character.velocity = Vector2.zero;
        IsMove = false;
        ChangeMoveDirection();
    }
    private void PlayerRideCheck()
    {
        if (base.playerCharacter.characterMoveState != Character.CharacterMoveState.Hanging)
        {
            IsRide = false;
        }

        if (IsRide)
        {
            base.playerCharacter.transform.position = character.transform.position;
        }
    }
    private bool WallCheck()
    {
        if (character.directionCollideState.leftWall ||
            character.directionCollideState.rightWall)
        {
            return true;
        }

        return false;
    }
    private IEnumerator Move()
    {
        var loop = new WaitForFixedUpdate();
        float curTime = 0f;
        int curParticleNum = 0;
        while (true)
        {
            yield return loop;
            if (WallCheck())
            {
                break;
            }

            curTime += Time.deltaTime;
            if (curTime >= 0.1f && character.IsGrounded && curParticleNum <= 10)
            {

                curTime = 0;
                curParticleNum++;
                CreateFrictionParticle();
            }
            if (!character.IsGrounded)
            {
                curParticleNum = 9;
            }

            PlayerRideCheck();

            if (Mathf.Abs(character.velocity.x) >= speed)
            {
                character.velocity.x = speed * directionValue;
                continue;
            }

            character.velocity.x += (Time.deltaTime) * directionValue;
        }

        MoveComplete();
    }
    public void BreakSequance()
    {
        StopCoroutine(coroutine);
        coroutine = StartCoroutine(Break());
    }
    private IEnumerator Break()
    {
        var loop = new WaitForFixedUpdate();
        float curTime = 0f;
        int curParticleNum = 0;
        while (true)
        {
            yield return loop;
            curTime += Time.deltaTime;
            if (curTime >= 0.1f && character.IsGrounded && curParticleNum <= 10)
            {

                curTime = 0;
                curParticleNum++;
                CreateFrictionParticle();
            }
            PlayerRideCheck();

            if (Mathf.Abs(character.velocity.x) <= 0.2f)
            {
                character.velocity = Vector2.zero;
                break;
            }

            character.velocity.x -= (Time.deltaTime * 3f) * directionValue;
        }

        MoveComplete();
    }
    private void CreateFrictionParticle()
    {
        levelManager.ShakeCameraEvent(0.5f, 0.3f);
        GameObject instanceParticle = Instantiate(frictionParticle, frictionParticlePos.transform);
        instanceParticle.transform.parent = null;
    }
    public void DestroyCar()
    {
        Instantiate(destroyFeedback, transform.position, Quaternion.identity);
        modelSprite.color = new Color(0f, 0f, 0f, 0f);
    }
    public void CreateCar(Transform spawnTr, MoveDirection value)
    {
        Initialized();
        transform.position = spawnTr.position;
        SetMoveDirection(value);
    }
}
