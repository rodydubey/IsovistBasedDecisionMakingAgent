using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using DefaultNamespace;
using UnityEngine;
using Random = UnityEngine.Random;

public class CrowdAgentSpawner : MonoBehaviour {
    [SerializeField] private GameObject person;
    [SerializeField] private Transform exitLocation;

    [Header("Spawn Settings Settings")] 
    [SerializeField] private int maxAgents = 15;
    [Range(0f, 100f)] 
    [SerializeField] private float spawningFrequency; // Agents/sec
    public List<LocationWeightPair> locationWeightList = new List<LocationWeightPair>();

    private float lastSpawn;
    private float totalRelativeFrequency;
    [SerializeField] private int numAgents;

    private List<GameObject> agents = new List<GameObject>();

    private uint nextID = 1;
    private string dirPath;
    
    [Serializable]
    public struct LocationWeightPair {
        public Transform location;
        public float relativeWeight;

        public LocationWeightPair(Transform location, float relativeWeight) {
            this.location = location;
            this.relativeWeight = relativeWeight;
        }
    }

    private void Start() {
        totalRelativeFrequency = locationWeightList.Sum(x => x.relativeWeight);
        dirPath = Application.dataPath + "/Data/Paths/" +
                  DateTime.Now.Month + "-" + DateTime.Now.Day + "-" + DateTime.Now.Year + " " +
                  DateTime.Now.TimeOfDay.Hours + "-" + DateTime.Now.TimeOfDay.Minutes + "-" +
                  DateTime.Now.TimeOfDay.Seconds + "/";
        Directory.CreateDirectory(dirPath);
    }

    private bool newWave = true;
    private float rt = 0f;
    public int waves;

    public void Update() {
        if (newWave && waves <= 500/maxAgents) {
            rt = 0;
            waves++;
            for (var i = 0; i < maxAgents; i++) {
                Transform spawn  = RndFromList();

                var newAgent = Instantiate(person, spawn.position, spawn.rotation);
                agents.Add(newAgent);
                newAgent.GetComponent<FieldOfView>().SetIsovist(ScriptableObject.CreateInstance<Isovist>());
                newAgent.transform.parent = transform;
                newAgent.GetComponent<SmartAgentController>().id = nextID++;
                newAgent.GetComponent<SmartAgentController>().rootPath = dirPath;
            }

            newWave = false;
        } else {
            rt += Time.deltaTime;
            if (rt > 90) {
                KillAll();
                newWave = true;
            }
        }
        
        /**if (!(Time.time - lastSpawn >= 1 / spawningFrequency) || numAgents >= maxAgents) return;
        lastSpawn = Time.time;
            
        var dest = RndFromList();
        Transform spawn;
        do {
            spawn = RndFromList();
        } while (dest.Equals(spawn));
            
        var newAgent = Instantiate(person, spawn.position, spawn.rotation);
        newAgent.GetComponent<FieldOfView>().SetIsovist(ScriptableObject.CreateInstance<Isovist>());
        newAgent.transform.parent = transform;
        var f = Random.Range(0, 500);
        var l = Random.Range(0, 500);
        newAgent.GetComponent<SmartAgentController>().id = nextID++;
        newAgent.GetComponent<SmartAgentController>().rootPath = dirPath;
        //newAgent.name = Names.firstNames[f] + " " + Names.lastNames[l];
        //var mt = newAgent.GetComponent<MoveTo>();
        //mt.goal = dest;
        //mt.spawner = this;

        numAgents++;**/
    }

    private void KillAll() {
        foreach (var agent in agents) {
            Destroy(agent);
        }
        agents.Clear();
    }

    public Transform RndFromList() {
        var i = Random.Range(0, totalRelativeFrequency);
        for(var j = 0; j < locationWeightList.Count; j++) {
            if ((i -= locationWeightList[j].relativeWeight) >= 0) continue;
            return locationWeightList[j].location;
        }

        return transform;
    }

    public void AgentDestroyed() {
        numAgents--;
    }
}
