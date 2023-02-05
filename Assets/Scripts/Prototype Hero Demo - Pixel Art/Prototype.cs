using Photon.Pun;
using UnityEngine;

public abstract class Prototype : MonoBehaviourPunCallbacks, IPunObservable
{
    [Header("Variables")]
    [SerializeField] private float m_maxSpeed = 4.5f;
    [SerializeField] private float m_jumpForce = 7.5f;

    [Header("Effects")]
    [SerializeField] private GameObject m_RunStopDust;
    [SerializeField] private GameObject m_JumpDust;
    [SerializeField] private GameObject m_LandingDust;

	protected PhotonView	m_photonView;
    private Animator		m_animator;
    private Rigidbody2D		m_rigidbody2d;
	protected SpriteRenderer	m_spriteRenderer;
    private Sensor			m_groundSensor;
    private FighterAudio	m_fighterAudio;
    protected bool			m_grounded = false;
    public bool				m_procession = false;
	protected bool			m_movePath = false;
	protected bool			m_jump = false;
    protected int			m_facingDirection = 1;
    protected float			m_disableMovementTimer = 0.0f;
	protected float			inputX;

    protected virtual void Start()
    {
		m_photonView = GetComponent<PhotonView>();
        m_animator = GetComponent<Animator>();
        m_rigidbody2d = GetComponent<Rigidbody2D>();
		m_spriteRenderer = GetComponent<SpriteRenderer>();
		m_fighterAudio = transform.Find("Audio").GetComponent<FighterAudio>();
        m_groundSensor = transform.Find("GroundSensor").GetComponent<Sensor>();
    }

    protected virtual void Update()
    {
		m_disableMovementTimer -= Time.deltaTime;

		OnGround();

		IsFalling();

		Movement();

		SetAirspeed();

		Jump_And_AnimationState();
    }

	private void OnGround()
	{
		if (!m_grounded && m_groundSensor.State())
		{
			m_grounded = true;
			m_jump = false;
			m_animator.SetBool("Grounded", m_grounded);
			m_animator.SetBool("Jump", m_jump);
		}
	}

	private void IsFalling()
	{
		//Check if character just started falling
		if (m_grounded && !m_groundSensor.State())
		{
			m_grounded = false;
			m_jump = false;
			m_animator.SetBool("Grounded", m_grounded);
			m_animator.SetBool("Jump", m_jump);
		}
	}

	private void Movement()
	{// 오프라인 스크립트 따로 분리
		if (!m_photonView.IsMine) return;

		inputX = 0.0f;

		if (m_disableMovementTimer < 0.0f)
		{
			inputX = Input.GetAxis("Horizontal");
		}
		float inputRaw = Input.GetAxisRaw("Horizontal");

		m_procession = Mathf.Abs(inputRaw) > Mathf.Epsilon && Mathf.Abs(Mathf.Sign(inputRaw)) == 1;
			
		if (inputX > 0f)
		{
			m_spriteRenderer.flipX = false;
		}
		else if (inputX < 0f)
		{
			m_spriteRenderer.flipX = true;
		}

		m_rigidbody2d.velocity = new Vector2(inputX * m_maxSpeed, m_rigidbody2d.velocity.y);
	}

	private void SetAirspeed()
	{
		m_animator.SetFloat("AirSpeedY", m_rigidbody2d.velocity.y);
	}

	private void Jump_And_AnimationState()
	{
		if (Input.GetButtonDown("Jump") && m_grounded && m_disableMovementTimer < 0.0f)
		{
			if (!m_photonView.IsMine) return;

			m_grounded = false;
			m_jump = true;
			m_animator.SetBool("Jump", m_jump);
			m_animator.SetBool("Grounded", m_grounded);
			m_rigidbody2d.velocity = new Vector2(m_rigidbody2d.velocity.x, m_jumpForce);
			m_groundSensor.Disable(0.2f);
		}
		else if (m_jump && m_grounded && m_disableMovementTimer < 0.0f)
		{
			m_grounded = false;
			m_jump = true;
			m_animator.SetBool("Jump", m_jump);
			m_animator.SetBool("Grounded", m_grounded);
			m_rigidbody2d.velocity = new Vector2(m_rigidbody2d.velocity.x, m_jumpForce);
			m_groundSensor.Disable(0.2f);
		}
		else if (m_procession)
		{
			m_animator.SetInteger("AnimState", 1);
		}
		else
		{
			m_animator.SetInteger("AnimState", 0);
			//m_animator.SetBool("Jump", false);
		}
	}

	public abstract void OnPhotonSerializeView(PhotonStream stream, PhotonMessageInfo info);

	private void SpawnDustEffect(GameObject dust, float dustXOffset = 0)
    {
        if (dust != null)
        {
            Vector3 dustSpawnPosition = transform.position + new Vector3(dustXOffset * m_facingDirection, 0.0f, 0.0f);
            GameObject newDust = Instantiate(dust, dustSpawnPosition, Quaternion.identity);
            newDust.transform.localScale = newDust.transform.localScale.x * new Vector3(m_facingDirection, 1, 1);
        }
    }

    void AE_runStop()
    {
        m_fighterAudio.PlaySound("RunStop");
        float dustXOffset = 0.6f;
        SpawnDustEffect(m_RunStopDust, dustXOffset);
    }

    void AE_footstep()
    {
        m_fighterAudio.PlaySound("Footstep");
    }

    void AE_Jump()
    {
        m_fighterAudio.PlaySound("Jump");
        SpawnDustEffect(m_JumpDust);
    }

    void AE_Landing()
    {
        m_fighterAudio.PlaySound("Landing");
        SpawnDustEffect(m_LandingDust);
    }
}
