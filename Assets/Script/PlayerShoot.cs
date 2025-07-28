using UnityEngine;
using UnityEngine.UI;
using System.Collections;

public class PlayerShoot : MonoBehaviour
{
    [Header("Shooting Setup")]
    public GameObject projectilePrefab;
    public Transform launchPoint;
    public Transform target;

    [Header("Power Settings")]
    public float maxPower = 15f;
    public float chargeSpeed = 10f;

    private float currentPower = 0f;
    [SerializeField]
    private float savedDoubleAttackPower = 0f;

    private bool isCharging = false;
    private bool canShoot = true;
    private bool hasPowerThrow = false;
    private bool hasDoubleAttack = false;
    private bool pendingDoubleAttack = false; // Track second shot for Double Attack

    private float powerThrowDamage = 10f;
    private float doubleAttackDamage = 5f;

    private TurnManager turnManager;
    private CharacterAnimation characterAnimation;

    [Header("UI")]
    public Image powerBar;
    public Image powerBarBackground;

    [Header("Think Timer UI")]
    public Image warningFillImage;  // Assign this in inspector to your warning fill UI image

    private float thinkTimer = 0f;
    private bool isThinking = false;

    [SerializeField]
    private float timeToThink = 10f;   // default, can load from database
    [SerializeField]
    private float timeToWarning = 3f;  // default, can load from database


    [Header("Bot Settings")]
    public bool isBot = false; // True if controlled by AI
    public float botFireDelay = 1.5f; // Delay before bot fires
    public float gravity = 9.8f;      // Gravity used in trajectory calculation
    private struct ShotParams { public float power; public float xOffset; }
    private bool botDoubleAttack;

    private bool botPendingDoubleAttack = false; // Track if bot will fire a second shot
    private float botSavedPower = 0f;            // Power for bot's second shot
    private float botDoubleAttackDamage = 5f;    // Damage for bot's double attack

    private bool botHasPowerThrow = false;
    private float botPowerThrowDamage = 10f;

    [Range(0f, 1f)]
    public float missChance = 0.2f;

    // --- Item Usage Flags ---
    public bool HasUsedPowerThrow { get; set; } = false;
    public bool HasUsedDoubleAttack { get; set; } = false;

    float windNormalActivate, windHardActivate;


    void Awake()
    {
        turnManager = FindObjectOfType<TurnManager>();
        characterAnimation = GetComponent<CharacterAnimation>();

        ItemDatabase db = SOHelper.LoadSOFromPath<ItemDatabase>("Data/ItemDatabase");

        float tempMissChance;
        if (GameSettingsManager.Instance.difficulty == "easy")
        {
            if (float.TryParse(db.data.EnemyHP_Easy.MissedChance, out tempMissChance))
                missChance = tempMissChance / 100f;
        }
        else if (GameSettingsManager.Instance.difficulty == "normal")
        {
            if (float.TryParse(db.data.EnemyHP_Normal.MissedChance, out tempMissChance))
                missChance = tempMissChance / 100f;
        }
        else if (GameSettingsManager.Instance.difficulty == "hard")
        {
            if (float.TryParse(db.data.EnemyHP_Hard.MissedChance, out tempMissChance))
                missChance = tempMissChance / 100f;
        }

        float tempTimeToThink, tempTimeToWarning;
        if (float.TryParse(db.data.TimeToThink.Sec, out tempTimeToThink))
            timeToThink = tempTimeToThink;
        else
            timeToThink = 2f;

        if (float.TryParse(db.data.TimeToWarning.Sec, out tempTimeToWarning))
            timeToWarning = tempTimeToWarning;
        else
            timeToWarning = 0.5f;

        if (warningFillImage != null)
        {
            warningFillImage.fillAmount = 0f;
            warningFillImage.gameObject.SetActive(false);
        }

        if (botDoubleAttack)
        botDoubleAttack = false;
    }

    void OnEnable()
    {
        Projectile.OnAnyProjectileLanded += OnProjectileLanded;
    }

    void OnDisable()
    {
        Projectile.OnAnyProjectileLanded -= OnProjectileLanded;
    }

    void Update()
    {
        if (isCharging)
        {
            currentPower += chargeSpeed * Time.deltaTime;
            currentPower = Mathf.Clamp(currentPower, 0f, maxPower);
            powerBar.fillAmount = currentPower / maxPower;

            if (currentPower >= maxPower)
                Fire();
        }

        if (isThinking && !isBot)
        {
            thinkTimer += Time.deltaTime;

            if (thinkTimer >= timeToThink - timeToWarning)
            {
                float warningElapsed = thinkTimer - (timeToThink - timeToWarning);
                // Decreasing fill from 1 to 0
                float fillAmount = Mathf.Clamp01(1f - (warningElapsed / timeToWarning));

                if (warningFillImage != null)
                    warningFillImage.fillAmount = fillAmount;
            }

            if (thinkTimer >= timeToThink)
            {
                isThinking = false;
                if (warningFillImage != null)
                {
                    warningFillImage.fillAmount = 0f;
                    warningFillImage.gameObject.SetActive(false);
                }

                Debug.Log("Player ran out of time to think!");

                canShoot = false;
                if (turnManager != null)
                    turnManager.EndTurn();
            }
        }

    }

    // ---------------- MANUAL SHOOT ----------------
    void OnMouseDown()
    {
        if (isBot || !canShoot) return;

        EndTurnTimer();  // Stop and reset the think timer when player starts charging

        StartCharging();
    }


    void OnMouseUp()
    {
        if (isBot || !canShoot || !isCharging) return;
        Fire();
    }

    private void StartCharging()
    {
        isCharging = true;
        currentPower = 0f;
        powerBar.fillAmount = 0f;
        powerBar.gameObject.SetActive(true);
        powerBarBackground.gameObject.SetActive(true);

        turnManager?.HideArrows();
        characterAnimation?.PlayShoot();
    }

    // ---------------- FIRE ----------------
    void Fire()
    {
        isCharging = false;
        canShoot = false;

        powerBar.gameObject.SetActive(false);
        powerBarBackground.gameObject.SetActive(false);

        float powerToUse = currentPower;

        if (hasDoubleAttack)
        {
            savedDoubleAttackPower = currentPower; // Store power for second shot
            pendingDoubleAttack = true;
            Debug.Log($"Double Attack: Saved power {savedDoubleAttackPower} for second shot.");
        }

        GameObject proj = Instantiate(projectilePrefab, launchPoint.position, Quaternion.identity);
        Vector2 dir = (target.position - launchPoint.position).normalized;
        dir.y += 1.1f;
        dir.Normalize();

        Projectile projectile = proj.GetComponent<Projectile>();

        // Load damage values from ItemDatabase
        ItemDatabase db = SOHelper.LoadSOFromPath<ItemDatabase>("Data/ItemDatabase");
        float tempnormalDamage, tempsmallDamage;
        if (float.TryParse(db.data.NormalAttack.Damage, out tempnormalDamage))
            projectile.normalDamage = tempnormalDamage;
        if (float.TryParse(db.data.SmallAttack.Damage, out tempsmallDamage))
            projectile.smallDamage = tempsmallDamage;

        if (hasPowerThrow)
        {
            projectile.normalDamage = powerThrowDamage;
            projectile.smallDamage = powerThrowDamage;
            Debug.Log($"Power Throw: Damage {projectile.normalDamage}");
            hasPowerThrow = false;
            HasUsedPowerThrow = true;
        }
        else if (hasDoubleAttack)
        {
            projectile.normalDamage = doubleAttackDamage;
            projectile.smallDamage = doubleAttackDamage;
            HasUsedDoubleAttack = true;
        }

        projectile.Launch(dir, powerToUse);

        currentPower = 0f;
        powerBar.fillAmount = 0f;
    }

    private void FireSecondShot()
    {
        canShoot = false;

        GameObject proj = Instantiate(projectilePrefab, launchPoint.position, Quaternion.identity);
        Vector2 dir = (target.position - launchPoint.position).normalized;
        dir.y += 1.1f;
        dir.Normalize();

        Projectile projectile = proj.GetComponent<Projectile>();

        // Use double attack damage
        projectile.normalDamage = doubleAttackDamage;
        projectile.smallDamage = doubleAttackDamage;

        // Launch using saved power
        projectile.Launch(dir, savedDoubleAttackPower);
        Debug.Log($"Double Attack: Fired second projectile with Power: {savedDoubleAttackPower}, Direction: {dir}");

        savedDoubleAttackPower = 0f; // Reset after firing
    }

    // ---------------- AUTO DOUBLE ATTACK ----------------
    private void OnProjectileLanded()
    {
        if (pendingDoubleAttack)
        {
            pendingDoubleAttack = false;
            hasDoubleAttack = false;

            Debug.Log("Player Double Attack: Launching second shot!");
            FireSecondShot();
        }
        else if (botPendingDoubleAttack && isBot)
        {
            botPendingDoubleAttack = false;

            Debug.Log("Bot Double Attack: Launching second shot!");
            BotFireSecondShot();
        }
    }

    // ---------------- BOT SHOOT ----------------
    public void BotShoot()
    {
        if (!isBot || !canShoot) return;
        StartCoroutine(BotShootRoutine());
    }

    private IEnumerator BotShootRoutine()
    {
        yield return new WaitForSeconds(botFireDelay);

        ShotParams shot = CalculateGuaranteedShot();
        bool shouldMiss = false;
        
        botHasPowerThrow = false;

        // Load damage values from ItemDatabase
        ItemDatabase db = SOHelper.LoadSOFromPath<ItemDatabase>("Data/ItemDatabase");
        float tempnormalDamage, tempsmallDamage, tempWindNormalActivate, tempWindHardActivate;

        
        if (float.TryParse(db.data.NormalEnemyActivate.WindForce, out tempWindNormalActivate))
        {
            windNormalActivate = tempWindNormalActivate;
        }
        if (float.TryParse(db.data.HardEnemyActivate.WindForce, out tempWindHardActivate))
        {
            windHardActivate = tempWindHardActivate;
        }

        // --- Hard Difficulty Power Throw ---
        SpecialItemManager specialItemManager = FindObjectOfType<SpecialItemManager>();
        
        if (GameSettingsManager.Instance.difficulty == "hard" && Mathf.Abs(WindManager.windForce) >= windHardActivate)
        {
            botHasPowerThrow = true;
            if (float.TryParse(db.data.PowerThrow.Damage, out tempnormalDamage))
                botPowerThrowDamage = tempnormalDamage;

            if(specialItemManager != null)
            {
                specialItemManager.BotShowText("hard");
                Debug.Log($"Bot Power Throw triggered! :{botPowerThrowDamage}");
            }
            
        }
        // --- Normal Difficulty Double Attack ---
        else if (GameSettingsManager.Instance.difficulty == "normal" && Mathf.Abs(WindManager.windForce) <= windNormalActivate)
        {
            botDoubleAttack = true;
            botPendingDoubleAttack = true;
            botSavedPower = shot.power;
            if (float.TryParse(db.data.SmallAttack.Damage, out tempsmallDamage))
                botDoubleAttackDamage = tempsmallDamage;
            if (specialItemManager != null)
            {
                specialItemManager.BotShowText("normal");
                Debug.Log($"Bot Double Attack triggered! :{botDoubleAttackDamage}");
            }
                
        }
        else
        {
            float randomCheck = Random.value;
            shouldMiss = randomCheck < missChance;
        }

        // If Power Throw, bot will NOT miss
        if (!botHasPowerThrow && shouldMiss)
        {
            Debug.Log("Bot intentionally missing!");
            shot.power += Random.Range(-10f, 10f);
            shot.power = Mathf.Clamp(shot.power, 1f, maxPower);
        }

        characterAnimation?.PlayShoot();
        BotFire(shot.power, shot.xOffset, shouldMiss);
    }

    private void BotFireSecondShot()
    {
        Debug.Log($"Bot Double Attack: Firing second shot with Power: {botSavedPower}");

        GameObject proj = Instantiate(projectilePrefab, launchPoint.position, Quaternion.identity);
        Vector2 dir = (target.position - launchPoint.position).normalized;
        dir.y += 1.1f;
        dir.Normalize();

        Projectile projectile = proj.GetComponent<Projectile>();
        projectile.normalDamage = botDoubleAttackDamage;
        projectile.smallDamage = botDoubleAttackDamage;

        projectile.Launch(dir, botSavedPower);

        botSavedPower = 0f;
    }

    private void BotFire(float power, float xOffset, bool shouldMiss)
    {
        canShoot = false;

        GameObject proj = Instantiate(projectilePrefab, launchPoint.position, Quaternion.identity);
        Vector2 dir = (target.position - launchPoint.position).normalized;
        dir.x += xOffset;
        dir.y += 1.1f;
        dir.Normalize();

        if (!botHasPowerThrow && shouldMiss)
        {
            dir.x += Random.Range(-0.2f, 0.2f);
            dir.y += Random.Range(-0.2f, 0.2f);
            dir.Normalize();
        }

        Projectile projectile = proj.GetComponent<Projectile>();

        if (botHasPowerThrow)
        {
            projectile.normalDamage = botPowerThrowDamage;
            projectile.smallDamage = botPowerThrowDamage;
        }
        else if (botDoubleAttack)
        {
            projectile.normalDamage = botDoubleAttackDamage;
            projectile.smallDamage = botDoubleAttackDamage;
        }
        else
        {
            ItemDatabase db = SOHelper.LoadSOFromPath<ItemDatabase>("Data/ItemDatabase");
            float tempnormalDamage, tempsmallDamage;
            if (float.TryParse(db.data.NormalAttack.Damage, out tempnormalDamage))
                projectile.normalDamage = tempnormalDamage;
            if (float.TryParse(db.data.SmallAttack.Damage, out tempsmallDamage))
                projectile.smallDamage = tempsmallDamage;
        }

        proj.GetComponent<Projectile>().Launch(dir, power);
    }

    private ShotParams CalculateGuaranteedShot()
    {
        ShotParams bestShot = new ShotParams { power = 1f, xOffset = 0f };
        float minDistance = float.MaxValue;

        for (float power = 1f; power <= maxPower; power += 0.2f)
        {
            for (float xOffset = -0.5f; xOffset <= 0.5f; xOffset += 0.05f)
            {
                float distance = SimulateUnityPhysics(power, xOffset);
                if (distance < minDistance)
                {
                    minDistance = distance;
                    bestShot.power = power;
                    bestShot.xOffset = xOffset;
                }

                if (distance <= 0.2f)
                {
                    return bestShot;
                }
            }
        }
        return bestShot;
    }

    private float SimulateUnityPhysics(float power, float xOffset = 0f)
    {
        Vector2 position = launchPoint.position;
        Vector2 dir = (target.position - launchPoint.position).normalized;
        dir.x += xOffset;
        dir.y += 1.1f;
        dir.Normalize();

        Vector2 velocity = dir * power;
        float gravityForce = Mathf.Abs(Physics2D.gravity.y * projectilePrefab.GetComponent<Rigidbody2D>().gravityScale);
        float wind = WindManager.windForce;

        float simStep = 0.1f;
        Vector2 targetPos = target.position + new Vector3(0.5f, 0f);

        for (float t = 0; t < 5f; t += simStep)
        {
            velocity += new Vector2(wind, -gravityForce) * simStep;
            position += velocity * simStep;

            if (Vector2.Distance(position, targetPos) <= 0.2f)
                return 0f;
        }
        return Vector2.Distance(position, targetPos);
    }

    // ---------------- ENABLE SHOOTING ----------------
    public void EnableShooting(bool enable)
    {
        canShoot = enable;

        if (isBot)
        {
            powerBar.gameObject.SetActive(false);
            powerBarBackground.gameObject.SetActive(false);
        }

        if (isBot && enable)
            BotShoot();
    }

    // ---------------- ITEM ACTIVATIONS ----------------
    public void ActivatePowerThrow(float damage)
    {
        hasPowerThrow = true;
        powerThrowDamage = damage;
        HasUsedPowerThrow = true;
    }

    public void ActivateDoubleAttack(float damage)
    {
        hasDoubleAttack = true;
        doubleAttackDamage = damage;
        HasUsedDoubleAttack = true;
    }

    public void ResetItemUsage()
    {
        HasUsedPowerThrow = false;
        HasUsedDoubleAttack = false;
    }
    public void StartTurnTimer()
    {
        if (isBot) return; // no timer for bot

        thinkTimer = 0f;
        isThinking = true;

        if (warningFillImage != null)
        {
            warningFillImage.fillAmount = 0f;
            warningFillImage.gameObject.SetActive(true);
        }
    }

    // Call this to stop think timer when turn ends
    public void EndTurnTimer()
    {
        isThinking = false;

        if (warningFillImage != null)
        {
            warningFillImage.fillAmount = 0f;
            warningFillImage.gameObject.SetActive(false);
        }
    }
}
