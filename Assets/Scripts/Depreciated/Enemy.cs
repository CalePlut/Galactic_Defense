//using System.Collections;
//using System.Collections.Generic;
//using UnityEngine;
//using UnityEngine.UI;
//using TMPro;

//public class Enemy : BasicShip
//{
//    [Header("References")]
//    public Slider healthBar;
//    public TextMeshProUGUI text;
//    public TextMeshProUGUI movePlaceholder;
//    public ParticleSystem explode;
//    Animator anim;
//    //public GameObject lockOnReticle;
//    public GameObject missilePrefab;
//    //public GameObject overShield;
//    public GameObject mesh;
//    public healthBarAnimator healthController;
//    //public reticleUI reticle;

//    public LineRenderer laserBeam;

//    [Header("Values")]
//    public int maxHealth = 2;
//    public int damage = 1;
//    int health;
//   // public int lockOn = 0;
//    //public bool shielded = false;
//    //List<int> comboDamage = new List<int> { 0, 1, 2, 4, 6 };

//    int consecutiveAttacks = 0;

//    //GameManager gameManager;
//    PlayerController player;

//    // Start is called before the first frame update
//    void Start()
//    {
//        health = maxHealth;
//        healthController.Refresh(maxHealth, health);
//        anim = GetComponent<Animator>();

//        //updateBar();
//    }

//    public void pickMove() //Selects random move and exectures it
//    {
//        if (health > 0)
//        {
//            movePlaceholder.gameObject.SetActive(true);
//            var myMove = (move)Random.Range(0, 3); //Random move that can be overridden

//            //AI interrupts for special cases go here
//            if (player.lockOn == 3)
//            {
//                myMove = move.Attack;
//            }
//            if (shielded)
//            {
//                if (Random.value < 0.5)
//                {
//                    myMove = move.Attack;
//                }
//                else
//                {
//                    myMove = move.missileLock;
//                }

//            }
//            if (consecutiveAttacks > 3) { myMove = move.Defend; }

//            //Executes move based on selection
//            if (myMove == move.missileLock)
//            {
//                lockMissiles();
//            }
//            if (myMove == move.Defend)
//            {
//                defend();
//            }
//            if (myMove == move.Attack)
//            {
//                attack();
//            }
//        }
//    }

//    void attack()
//    {
//        //player.clearMissileLock();
//        movePlaceholder.text = "Fire weapons";
//        player.takeFire(damage);
//        //clearSingleLock();
//        laser();
//        if (player.lockOn > 0) { missile(); }
//    }

//    void defend()
//    {
//        movePlaceholder.text = "Charge shields";
//        shieldPower();
//    }

//    void lockMissiles()
//    {
//        movePlaceholder.text = ("Missile lock");
//        player.missileLock();
//    }
//    public void hideMove()
//    {
//        movePlaceholder.gameObject.SetActive(false);
//    }

//    //void shieldPower()
//    //{
//    //    shielded = true;
//    //    overShield.SetActive(true);
//    //}
//    //void shieldRemove()
//    //{
//    //    shielded = false;
//    //    overShield.SetActive(false);
//    //}

//    void processDamage(int _dam)
//    {
//        health -= _dam;
//        healthController.takeDamage(_dam);

//    }

//    public void takeDamage(int damage)
//    {
//        consecutiveAttacks++;

//    //    var toTake = damageEquation(damage, lockOn, shielded);
//   //     var toLose = lockOnLoss(lockOn);
//    //    player.decreaseLock(toLose);
//        //Removes overshield on being hit and processes damage
//        //processDamage(toTake);
//      //  damageText.takeDamage(toTake);

//     //   clearMissileLock();
//       // shieldRemove();

//        if (health <= 0)
//        {
//            StartCoroutine(die());
//        }
//    }

//    //public override void missileLock()
//    //{
//    //    base.missileLock();
//    //    consecutiveAttacks = 0;
//    //    var music = gameManager.gameObject.GetComponent<MusicManager>();
//    //    music.changeEnemyLockOn(lockOn);
//    //}

//    public void setGM(GameManager _manager)
//    {
//        gameManager = _manager;
//    }
//    public void setPlayer(PlayerController _player)
//    {
//        player = _player;
//    }

//    public void setStrength(int _pursuit)
//    {
//        maxHealth = _pursuit*2;
//        health = maxHealth;
//        damage = _pursuit;
//    }

//    public void missile()
//    {
//        var newMissile = Instantiate(missilePrefab, transform.position, Quaternion.identity, this.transform);
//        newMissile.GetComponent<MissileControl>().Launch(player.gameObject);
//    }

//    public void laser()
//    {
//        laserBeam.gameObject.SetActive(true);
//        StartCoroutine(enemyLaser(laserBeam));
//        laserBeam.SetPosition(1, transform.InverseTransformPoint(player.transform.position));
//    }

//    IEnumerator enemyLaser(LineRenderer lase)
//    {
//        var col = lase.material.color;
//        col.a = 0.0f;
//        while (col.a < 3.5f)
//        {
//            col.a += 4.0f * Time.deltaTime;
//            lase.material.color = col;
//            yield return null;
//        }
//        yield return new WaitForSeconds(1.0f);
//        laserBeam.gameObject.SetActive(false);
//    }

//    IEnumerator die()
//    {
//        yield return new WaitForSeconds(1.0f);
//        mesh.SetActive(false);
//        explode.Play();
//        yield return new WaitForSeconds(1.0f);
//        gameManager.winCombat();
//        GameObject.Destroy(this.gameObject);
//    }
//}