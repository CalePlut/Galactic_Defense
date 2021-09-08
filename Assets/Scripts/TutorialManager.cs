using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UI;

public class TutorialManager : MonoBehaviour
{
    public GameObject firstSlide;
    private GameManager manager;
    private SceneControl scene;
    private GameObject currentSlide;
    private PlayerShip player;
    private EnemyShip enemy;

    public List<Button> hotbar;

    private void Start()
    {
        currentSlide = firstSlide;
        var playerObj = GameObject.Find("Player Ship");
        player = playerObj.GetComponent<PlayerShip>();

        var managerObj = GameObject.Find("MainCamera");
        manager = managerObj.GetComponent<GameManager>();
        scene = managerObj.GetComponent<SceneControl>();
    }

    public void StartTutorial()
    {

        firstSlide.SetActive(true);

        foreach (Button button in hotbar)
        {
            button.interactable = false;
        }
    }

    public void SetTutorialEnemy(EnemyShip _enemy)
    {
        enemy = _enemy;
    }

    public void advanceSlide(GameObject next)
    {
        currentSlide.SetActive(false);
        next.SetActive(true);
        currentSlide = next;
    }

    public void StartAttack(GameObject next)
    {
        currentSlide.SetActive(false);
        StartCoroutine(SlideWait(2.5f, next));
        currentSlide = next;
        player.AttackToggle();
    }

    public void HeavyAttack(GameObject next)
    {
        currentSlide.SetActive(false);
        StartCoroutine(SlideWait(3.5f, next));
        currentSlide = next;
        enemy.ShieldsDown();
        player.HeavyAttackTrigger();
    }

    public void ParryPractice(GameObject next)
    {
        currentSlide.SetActive(false);
        enemy.HeavyAttackTrigger();

        StartCoroutine(ParryWait(2.75f, next));
        currentSlide = next;
    }

    public void ParrySuccess(GameObject next)
    {
        currentSlide.SetActive(false);
        player.AbsorbTrigger();
        manager.resumeGame();
        StartCoroutine(SlideWait(1.8f, next));
        currentSlide = next;
    }

    public void HealPractice(GameObject next)
    {
        currentSlide.SetActive(false);
        player.HealTrigger();
        StartCoroutine(SlideWait(3.0f, next));
        currentSlide = next;
    }

    public void StartGame()
    {
        GameManager.tutorial = false;
        scene.restart();
    }

    private IEnumerator SlideWait(float delay, GameObject toActivate)
    {
        yield return new WaitForSeconds(delay);
        toActivate.SetActive(true);
    }

    private IEnumerator ParryWait(float delay, GameObject toActivate)
    {
        yield return new WaitForSeconds(delay);
        manager.pauseGame();
        toActivate.SetActive(true);
    }

    public void HarmPlayer()
    {
        player.ShieldsDown();
        player.TakeDamage(20.0f);
        player.ShieldsUp();
    }
}