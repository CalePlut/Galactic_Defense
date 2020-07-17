using TMPro;
using UnityEngine;

public class ResolutionManager : MonoBehaviour
{
    public TextMeshProUGUI playerText, enemyText, logic;

    public Color reactor, sensor, engine;

    public void setMoves(move _player, move _enemy)
    {
        if (_player == move.Defend)
        {
            playerText.color = engine;
            playerText.text = "Engine";
        }
        if (_player == move.Attack)
        {
            playerText.color = reactor;
            playerText.text = "Attack";
        }
        if (_player == move.missileLock)
        {
            playerText.color = sensor;
            playerText.text = "Sensor";
        }

        if (_enemy == move.Defend)
        {
            enemyText.color = engine;
            enemyText.text = "Engine";
        }
        if (_enemy == move.Attack)
        {
            enemyText.color = reactor;
            enemyText.text = "Attack";
        }
        if (_enemy == move.missileLock)
        {
            enemyText.color = sensor;
            enemyText.text = "Sensor";
        }
    }

    public void playerWin()
    {
        logic.color = Color.green;
        logic.text = "beats";
    }

    public void playerLose()
    {
        logic.color = Color.red;
        logic.text = "loses to";
    }

    public void playerTie(int playerLevel, int enemyLevel)
    {
        playerText.text = playerText.text + "(" + playerLevel + ")";
        enemyText.text = enemyText.text + "(" + enemyLevel + ")";

        if (playerLevel > enemyLevel)
        {
            logic.color = Color.green;
            logic.text = ">";
        }
        if (playerLevel < enemyLevel)
        {
            logic.color = Color.red;
            logic.text = "<";
        }
        if (playerLevel == enemyLevel)
        {
            logic.color = Color.yellow;
            logic.text = "=";
        }
    }

    public void disableMenu()
    {
        gameObject.SetActive(false);
    }

    // Start is called before the first frame update
    private void Start()
    {
    }

    // Update is called once per frame
    private void Update()
    {
    }
}