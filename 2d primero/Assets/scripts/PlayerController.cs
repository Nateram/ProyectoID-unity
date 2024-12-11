    using System.Collections;
    using System.Collections.Generic;
    using UnityEngine;
    using UnityEngine.Events;
    using UnityEngine.UI;

    public class PlayerController : MonoBehaviour
    {


        
        [Header("Horizontal Movement Settings:")]
        [SerializeField] private float walkSpeed = 1;


        private bool canMove = true;


        [Header("Vertical Movement Settings")]
        [SerializeField] private float jumpForce = 45f;
        private float jumpBufferCounter = 0;

        private int airJumpCounter = 0;
        [SerializeField] private int maxAirJumps;

        [SerializeField] private float jumpBufferFrames;

        private float coyoteTimeCounter = 0;
        [SerializeField] private float coyoteTime;

        [Header("Ground Check Settings:")]
        [SerializeField] private Transform groundCheckPoint;
        [SerializeField] private float groundCheckY = 0.2f;
        [SerializeField] private float groundCheckX = 0.5f;
        [SerializeField] private LayerMask whatIsGround;



        [Header("Dash Settings")]
        [SerializeField] private float dashSpeed;
        [SerializeField] private float dashTime;
        [SerializeField] private float dashCooldown;
        [Space(5)]

        private float gravity;
        private bool canDash = true;


        [HideInInspector] public PlayerStateList pState;
        private Rigidbody2D rb;
        private float xAxis, yAxis;

        Animator anim;

        [Header("Alas")]

        [SerializeField] private Animator alasAnimator;



        [Header("Attack Settings:")]

        [SerializeField] private LayerMask attackableLayer; //the layer the player can attack and recoil off of

        [SerializeField] private float damage; //the damage the player does to an enemy

        private bool attack = false;
        [SerializeField] private Animator AguijonAnimator;
        [SerializeField] private Animator AguijonArriba;

        [SerializeField] private Animator AguijonAbajo;
        
        [SerializeField] private float timeBetweenAttack;
        private float timeSinceAttack;
        [SerializeField] private Transform SideAttackTransform; //the middle of the side attack area
        [SerializeField] private Vector2 SideAttackArea; //how large the area of side attack is

        [SerializeField] private Transform UpAttackTransform; //the middle of the up attack area
        [SerializeField] private Vector2 UpAttackArea; //how large the area of side attack is

        [SerializeField] private Transform DownAttackTransform; //the middle of the down attack area
        [SerializeField] private Vector2 DownAttackArea; //how large the area of down attack is

        [Header("Recoil Settings:")]
        [SerializeField] private int recoilXSteps = 5; //how many FixedUpdates() the player recoils horizontally for
        [SerializeField] private int recoilYSteps = 5; //how many FixedUpdates() the player recoils vertically for 

        [SerializeField] private float recoilXSpeed = 10; //the speed of horizontal recoil
        [SerializeField] private float recoilYSpeed = 50; //the speed of vertical recoil

        private int stepsXRecoiled, stepsYRecoiled; //the no. of steps recoiled horizontally and verticall 
       
        [Header("Health Settings")]
        public int health;
        public int maxHealth;

        bool restoreTime;
        float restoreTimeSpeed;
        [SerializeField] GameObject particulasgolpe;
        [SerializeField] float hitFlashSpeed;

        float healTimer;
        [SerializeField] float timeToHeal;
        public UnityEvent<int> cambioVida;

        [SerializeField] private Menugameover menuGameover;

        [Header("Knockback Settings")]
        [SerializeField] private float knockbackForceX = 10f;
        [SerializeField] private float knockbackForceY = 5f;
        [SerializeField] private float knockbackDuration = 0.2f;

       
        [Header("Mana Settings")]
        [SerializeField] float mana;
        [SerializeField] float manaDrainSpeed;

        [SerializeField] float manaGain;

        [SerializeField] UnityEngine.UI.Image manaStorage;

    
        [SerializeField] float castOrHealTimer;


        [Header("Spell Settings")]
        //spell stats
        [SerializeField] float manaSpellCost = 0.3f;
        [SerializeField] float timeBetweenCast = 0.5f;
        float timeSinceCast;
        [SerializeField] float spellDamage; //upspellexplosion and downspellfireball
        //spell cast objects
        [SerializeField] GameObject sideSpellFireball;
        [SerializeField] GameObject upSpellExplosion;
        public static PlayerController Instance;

        private SpriteRenderer sr;

        private void Awake()
        {
            if(Instance != null && Instance != this)
            {
                Destroy(gameObject);
            }
            else
            {
                Instance = this;
            }
            DontDestroyOnLoad(gameObject);



        }

        void Start()
        {
            rb = GetComponent<Rigidbody2D>();
            anim = GetComponent<Animator>();
            pState = GetComponent<PlayerStateList>();
            gravity = rb.gravityScale;
            sr = GetComponent<SpriteRenderer>();
            Health  = maxHealth;
            cambioVida.Invoke(Health);
            Mana = mana;
            pState.alive = true; // Set alive to true at start

            manaStorage.fillAmount = Mana;



        }

        void Update()
        {
            if (pState.alive){
            CastSpell();
            GetInputs();
            UpdateJumpVariables();
            if (pState.dashing) return;
            RestoreTimeScale();
            Move();
            Heal();
            if (pState.healing) return;
            Flip();
            Jump();
            StartDash();
            Attack();
            }
            FlashWhileInvincible();


        }

    private void OnTriggerEnter2D(Collider2D _other) //for up and down cast spell
    {
        if(_other.GetComponent<Enemy>() != null && pState.casting)
        {
            _other.GetComponent<Enemy>().EnemyHit(spellDamage, (_other.transform.position - transform.position).normalized, -recoilYSpeed);
        }
    }
    private void FixedUpdate()
    {
        if (pState.dashing || pState.healing ) return;
        Recoil();
    } 

        void GetInputs()
        {
            xAxis = Input.GetAxisRaw("Horizontal");
            yAxis = Input.GetAxisRaw("Vertical");
            attack = Input.GetButtonDown("Attack");
            if (Input.GetButton("Cast/Heal"))
            {
                castOrHealTimer += Time.deltaTime;
            }
            else
            {
                castOrHealTimer = 0;
            }

        }

        void Flip()
        {
            if (xAxis < 0)
            {
                transform.localScale = new Vector2(-Mathf.Abs(transform.localScale.x), transform.localScale.y);
                pState.lookingRight = false; 

            }
            else if (xAxis > 0)
            {
                transform.localScale = new Vector2(Mathf.Abs(transform.localScale.x), transform.localScale.y);
                pState.lookingRight = true; 
            }
        }

        private void Move()
        {
            if (!canMove) return;

            if (pState.healing) rb.velocity = new Vector2(0, 0);
            rb.velocity = new Vector2(walkSpeed * xAxis, rb.velocity.y);
            anim.SetBool("Andando", rb.velocity.x != 0 && Grounded());
        }

        public bool Grounded()
        {
            if (Physics2D.Raycast(groundCheckPoint.position, Vector2.down, groundCheckY, whatIsGround) 
                || Physics2D.Raycast(groundCheckPoint.position + new Vector3(groundCheckX, 0, 0), Vector2.down, groundCheckY, whatIsGround) 
                || Physics2D.Raycast(groundCheckPoint.position + new Vector3(-groundCheckX, 0, 0), Vector2.down, groundCheckY, whatIsGround))
            {
                return true;
            }
            else
            {
                return false;
            }
        }

        void Jump()
        {
            // Cortar el salto cuando sueltas el botón
            if (!pState.jumping && jumpBufferCounter > 0 && coyoteTimeCounter > 0)
            {
                rb.velocity = new Vector2(rb.velocity.x, jumpForce);
                pState.jumping = true;
            }
            if (!Grounded() && airJumpCounter < maxAirJumps && Input.GetButtonDown("Jump"))
            {
                rb.velocity = new Vector2(rb.velocity.x, jumpForce);
                airJumpCounter++;
                pState.jumping = true;
                alasAnimator.SetBool("Alas", !Grounded());
                anim.SetBool("doblesalt", !Grounded());
            }
            if (Input.GetButtonUp("Jump") && rb.velocity.y > 3)
            {
                rb.velocity = new Vector2(rb.velocity.x, 0);

                pState.jumping = false;
            }

        anim.SetBool("Saltando", !Grounded());
        }

        void UpdateJumpVariables()
        {
            if (Grounded())
            {
                coyoteTimeCounter = coyoteTime;
                airJumpCounter = 0; // Reiniciamos el contador de saltos aéreos
                pState.jumping = false;
                anim.SetBool("doblesalt", !Grounded());
                alasAnimator.SetBool("Alas", !Grounded());




            }
            else
            {
                coyoteTimeCounter -= Time.deltaTime;
            }

            if (Input.GetButtonDown("Jump"))
            {
                jumpBufferCounter = jumpBufferFrames;
            }
            else
            {
                jumpBufferCounter -= Time.deltaTime * 10;
            }
        }

        public void OnDoubleJumpAnimationComplete()
        {
        anim.SetBool("doblesalt", false);
        alasAnimator.SetBool("Alas", false);

        }

        IEnumerator Dash() //the dash action the player performs
        {
            canDash = false;
            pState.dashing = true;
            anim.SetBool("doblesalt", false);
            alasAnimator.SetBool("Alas", false);
            anim.SetTrigger("Dashing");
            rb.gravityScale = 0;
            int _dir = pState.lookingRight ? 1 : -1;
            rb.velocity = new Vector2(_dir * dashSpeed, 0);
            yield return new WaitForSeconds(dashTime);
            rb.gravityScale = gravity;
            pState.dashing = false;
            yield return new WaitForSeconds(dashCooldown);
            canDash = true;

        }

        void StartDash()
        {
            if(Input.GetButtonDown("Dash") && canDash)
            {
                StartCoroutine(Dash());
            }
        }

        void Attack()
        {
            timeSinceAttack += Time.deltaTime;
            if(attack && timeSinceAttack >= timeBetweenAttack)
            {
                timeSinceAttack = 0;

                if(yAxis == 0 || yAxis < 0 && Grounded())
                {
                    anim.SetTrigger("Ataque");
                    AguijonAnimator.SetTrigger("Ataque");
                    anim.SetBool("doblesalt", false);
                    alasAnimator.SetBool("Alas", false);
                    Hit(SideAttackTransform, SideAttackArea,ref pState.recoilingX, recoilXSpeed);
                }
                else if(yAxis > 0)
                {
                    AguijonArriba.SetTrigger("Ataque");
                    anim.SetTrigger("AtaqueArriba");
                    anim.SetBool("doblesalt", false);
                    alasAnimator.SetBool("Alas", false);
                    Hit(UpAttackTransform, UpAttackArea,ref pState.recoilingX, recoilXSpeed);
                }
                else if (yAxis < 0 && !Grounded())
                {
                    anim.SetTrigger("AtaqueAbajo");
                    anim.SetBool("doblesalt", false);
                    AguijonAbajo.SetTrigger("Ataque");
                    alasAnimator.SetBool("Alas", false);
                Hit(DownAttackTransform, DownAttackArea, ref pState.recoilingY, recoilYSpeed);
                }
            }
        }
        void Hit(Transform _attackTransform, Vector2 _attackArea , ref bool _recoilDir, float _recoilStrength )
        {
            Collider2D[] objectsToHit = Physics2D.OverlapBoxAll(_attackTransform.position, _attackArea, 0, attackableLayer);
            List<Enemy> hitEnemies = new List<Enemy>();

            if(objectsToHit.Length > 0)
            {
                _recoilDir = true;
            } 
            for(int i = 0; i < objectsToHit.Length; i++)
            {
                Enemy e = objectsToHit[i].GetComponent<Enemy>();
                if(e && !hitEnemies.Contains(e))
                {
                    e.EnemyHit(damage, (transform.position - objectsToHit[i].transform.position).normalized, _recoilStrength);
                    hitEnemies.Add(e);
                    if (objectsToHit[i].CompareTag("Enemy"))
                    {
                        Mana += manaGain;
                    }
                }
            }
        }

        void Recoil()
        {
            if(pState.recoilingX)
            {
                if(pState.lookingRight)
                {
                    rb.velocity = new Vector2(-recoilXSpeed, 0);
                }
                else
                {
                    rb.velocity = new Vector2(recoilXSpeed, 0);
                }
            }

            if(pState.recoilingY)
            {
                rb.gravityScale = 0;
                if (yAxis < 0)
                {                
                    rb.velocity = new Vector2(rb.velocity.x, recoilYSpeed);
                }
                else
                {
                    rb.velocity = new Vector2(rb.velocity.x, -recoilYSpeed);
                }
                airJumpCounter = 0;
            }
            else
            {
                rb.gravityScale = gravity;
            }

            //stop recoil
            if(pState.recoilingX && stepsXRecoiled < recoilXSteps)
            {
                stepsXRecoiled++;
            }
            else
            {
                StopRecoilX();
            }
            if (pState.recoilingY && stepsYRecoiled < recoilYSteps)
            {
                stepsYRecoiled++;
            }
            else
            {
                StopRecoilY();
            }

            if(Grounded())
            {
                StopRecoilY();
            } 
        }
        void StopRecoilX()
        {
            stepsXRecoiled = 0;
            pState.recoilingX = false;
        }
        void StopRecoilY()
        {
            stepsYRecoiled = 0;
            pState.recoilingY = false;
        } 


        private void OnDrawGizmos()
        {
            Gizmos.color = Color.red;
            Gizmos.DrawWireCube(SideAttackTransform.position, SideAttackArea);
            Gizmos.DrawWireCube(UpAttackTransform.position, UpAttackArea);
            Gizmos.DrawWireCube(DownAttackTransform.position, DownAttackArea);
        }

        public void TakeDamage(float _damage, Vector2 attackerPosition)
        {
            // If player is dead, don't process damage
            if (pState.alive){
        
                Health -= Mathf.RoundToInt(_damage);
                cambioVida.Invoke(Health);
            
                if (Health <= 0)
                {
                    Health = 0;
                    StartCoroutine(Death());
                    return;
                }
                else
                {
                // Calculate knockback directionx
                Vector2 knockbackDirection = (Vector2)transform.position - attackerPosition;
                knockbackDirection.Normalize();
            
                // Apply knockback and damage effects
                StartCoroutine(ApplyKnockback(knockbackDirection));
                StartCoroutine(StopTakingDamage());
                }

            }
        }
        IEnumerator StopTakingDamage()
        {
            pState.invincible = true;
            anim.SetTrigger("Dañado");
            particulasgolpe.SetActive(true);
            GameObject _particulasgolpe = Instantiate(particulasgolpe, transform.position, Quaternion.identity);
            Destroy(_particulasgolpe, 1.5f);
            yield return new WaitForSeconds(0.4f);
            pState.invincible = false;
        }
        private IEnumerator ApplyKnockback(Vector2 direction)
        {
            // Desactiva el movimiento del jugador durante el knockback
            pState.dashing = true; // Usar el estado de dash para prevenir movimiento

            // Calcula la dirección X basada en la posición relativa al enemigo
            float directionX = Mathf.Sign(direction.x);
            
            // Aplica la fuerza con mayor control
            rb.velocity = Vector2.zero; // Reset velocidad actual
            rb.AddForce(new Vector2(directionX * knockbackForceX, knockbackForceY), ForceMode2D.Impulse);
            
            yield return new WaitForSeconds(knockbackDuration);
            
            // Restaura el control
            pState.dashing = false;
            rb.velocity = Vector2.zero;
        }
        public int Health
        {
            get { return health; }
            set
            {
                if (health != value)
                {
                    health = Mathf.Clamp(value, 0, maxHealth);
                }
            }
        }

        void RestoreTimeScale()
        {
            if (restoreTime)
            {
                if (Time.timeScale < 1)
                {
                    Time.timeScale += Time.unscaledDeltaTime * restoreTimeSpeed;
                }
                else
                {
                    Time.timeScale = 1;
                    restoreTime = false;
                }
            }
        }
        public void HitStopTime(float _newTimeScale, int _restoreSpeed, float _delay)
        {
            restoreTimeSpeed = _restoreSpeed;
            if (_delay > 0)
            {
                StopCoroutine(StartTimeAgain(_delay));
                StartCoroutine(StartTimeAgain(_delay));
            }
            else
            {
                restoreTime = true;
            }
            Time.timeScale = _newTimeScale;
        }
        IEnumerator StartTimeAgain(float _delay)
        {
            yield return new WaitForSecondsRealtime(_delay);
            Time.timeScale = 1f; // Restauración instantánea
            restoreTime = false; // No usar restauración gradual
        }
        void FlashWhileInvincible()
        {
            if (pState.invincible)
            {
                // Aumentamos la frecuencia multiplicando Time.time por un valor más alto
                sr.enabled = Mathf.FloorToInt(Time.time * 80f) % 2 == 0;
            }
            else
            {
                sr.enabled = true;
            }
        }

        void Heal()
        {
            if (Input.GetButton("Cast/Heal") && castOrHealTimer > 0.15f && Health < maxHealth && Mana > 0 && Grounded() && !pState.dashing)
            {
                pState.healing = true;
                anim.SetBool("Curandose", true);

                //healing
                healTimer += Time.deltaTime;
                if (healTimer >= timeToHeal)
                {
                    Health++;
                    cambioVida.Invoke(Health);

                    healTimer = 0;
                }
                Mana -= Time.deltaTime * manaDrainSpeed;

            }
                else
                {
                    pState.healing = false;
                    anim.SetBool("Curandose", false);
                    healTimer = 0;
                }

        }
        float Mana
        {
            get { return mana; }
            set
            {
                //if mana stats change
                if (mana != value)
                {
                    mana = Mathf.Clamp(value, 0, 1);
                    manaStorage.fillAmount = Mana;

                }
            }
        }


        void CastSpell()
        {
            if (Input.GetButtonUp("Cast/Heal")  && castOrHealTimer <= 0.15f && timeSinceCast >= timeBetweenCast && Mana >= manaSpellCost)
            {
                pState.casting = true;
                timeSinceCast = 0;
                StartCoroutine(CastCoroutine());
            }
            else
            {
                timeSinceCast += Time.deltaTime;
            }
        }

        IEnumerator CastCoroutine()
        {
            anim.SetBool("Casting", true);
            canMove = false; // Bloquear movimiento

            yield return new WaitForSeconds(0.15f);

            //side cast
            if (yAxis == 0 || (yAxis < 0 && Grounded()))
            {
                GameObject _fireBall = Instantiate(sideSpellFireball, SideAttackTransform.position, Quaternion.identity);

                //flip fireball
                if(pState.lookingRight)
                {
                    _fireBall.transform.eulerAngles = Vector3.zero; // if facing right, fireball continues as per normal
                }
                else
                {
                    _fireBall.transform.eulerAngles = new Vector2(_fireBall.transform.eulerAngles.x, 180); 
                    //if not facing right, rotate the fireball 180 deg
                }
                recoilXSpeed = 5;
                pState.recoilingX = true;
            }

            //up cast
            else if( yAxis > 0)
            {
                Instantiate(upSpellExplosion, transform);
                rb.velocity = Vector2.zero;
            }

            Mana -= manaSpellCost;
            yield return new WaitForSeconds(0.20f);
            canMove = true; // Bloquear movimiento
            anim.SetBool("Casting", false);
            recoilXSpeed = 10;
            pState.casting = false;
        }
        IEnumerator Death()
        {
            pState.alive = false; // Set this first to prevent other actions
            Time.timeScale = 1f;
            rb.simulated = false;      // Stops all physics interactions
            rb.velocity = Vector2.zero; // Stop any current movement
            GameObject _particulasgolpe = Instantiate(particulasgolpe, transform.position, Quaternion.identity);
            Destroy(_particulasgolpe, 1.5f);
            anim.SetTrigger("Death");
            // Wait in real time (not affected by time scale)
            yield return new WaitForSecondsRealtime(1.5f);
            menuGameover.ShowDeathMenu();
        }

// In PlayerController.cs, add:
public void RestoreMana(float amount)
{
    Mana += amount;
}
        public void Respawned()
        {
            if (!pState.alive)
            {
                pState.alive = true;
                Health = maxHealth;
                cambioVida.Invoke(Health);
                anim.Play("iddle");
                rb.simulated = true;
                rb.velocity = Vector2.zero;
                menuGameover.HideMenu();
            }

        }
    }

