using System.Collections.Generic;
using UnityEngine;

public class TargetManager : MonoBehaviour
{
    public List<EnemyBase> ships;
    public GameObject leftReticle, centreReticle, rightReticle;
    public static EnemyBase target;
    private int index;

    public void setShips(List<EnemyBase> _ships)
    {
        ships = new List<EnemyBase>(_ships);
        index = 1;
        target = _ships[index];
        centreReticle.SetActive(true);
    }

    public void advanceTarget()
    {
        index++;
        if (index >= ships.Count)
        {
            index = 0;
        }
        target = ships[index];
        setReticle();
    }

    private void setReticle()
    {
        if (target.pos == position.left)
        {
            leftReticle.SetActive(true);
            centreReticle.SetActive(false);
            rightReticle.SetActive(false);
        }
        else if (target.pos == position.centre)
        {
            leftReticle.SetActive(false);
            centreReticle.SetActive(true);
            rightReticle.SetActive(false);
        }
        else if (target.pos == position.right)
        {
            leftReticle.SetActive(false);
            centreReticle.SetActive(false);
            rightReticle.SetActive(true);
        }
    }

    public void removeShip(EnemyBase ship)
    {
        if (ships.Contains(ship))
        {
            ships.Remove(ship);
        }
    }

    public void addShip(EnemyBase ship)
    {
        ships.Add(ship);
    }
}