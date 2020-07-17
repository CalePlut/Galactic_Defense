using UnityEngine;

public class DestinationSelect : MonoBehaviour
{
    //public PlayerController player;

    //public Button reactor, sensor, engine;

    //// Start is called before the first frame update
    //void Start()
    //{
    //}

    //// Update is called once per frame
    //void Update()
    //{
    //}

    //public void destinationSetup()
    //{
    //    selectBonuses();
    //}

    //void selectBonuses()
    //{
    //    var sensorValue = player.sensors;

    //    //Disables buttons
    //    disableAll();

    //    //Shows a number of buttons equal to the sensors value - 1 (at sensor 1, 0 bonus. at sensor 4, all 3 can be chosen)
    //    var howMany = sensorValue - 1;

    //    //Debug.Log("Showing " + howMany + "random bonuses due to sensors value of " + sensorValue);

    //    //Creates list and populates with the buttons
    //    var statList = new List<Button>();
    //    statList.Add(reactor);
    //    statList.Add(sensor);
    //    statList.Add(engine);

    //    //Activates howMany randomly selected buttons
    //    for(int i = 0; i<howMany; i++)
    //    {
    //        var whichStat = Random.Range(0, statList.Count);
    //        statList[whichStat].interactable = true;
    //        statList.RemoveAt(whichStat);
    //    }

    //}

    //void disableAll()
    //{
    //    reactor.interactable = false;
    //    sensor.interactable = false;
    //    engine.interactable = false;
    //}
}