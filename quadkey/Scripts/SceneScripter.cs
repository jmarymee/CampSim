using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public enum SceneScenario {  None, Cyclades, SuncorDozers }

public class SceneScripter : MonoBehaviour
{
    public SceneScenario scenario = SceneScenario.None;
    public bool started = false;
    public bool fattracks = false;
    VehicleTrackMan vtm;
    bool running = false;
    // Start is called before the first frame update
    void Start()
    {
        
    }

    public void Init(SceneScenario scenario, VehicleTrackMan vtm )
    {
        this.scenario = scenario;
        this.vtm = vtm;
    }

    bool tracksLoaded = false;
    public void SetScale()
    {
        if (fattracks)
        {
            vtm.trackScale = 200;
            vtm.consolidationDistance = 3 * vtm.trackScale;
        }
        else
        {
            vtm.trackScale = 10;
            vtm.consolidationDistance = 3 * vtm.trackScale;

        }
        Debug.Log("Set cd in SceneScenario.SetScale:" + vtm.consolidationDistance);
    }

    public IEnumerator LoadTracks()
    {
        if (!tracksLoaded)
        {
            SetScale();
            tracksLoaded = true;
            foreach (var trk in vtm.vehicleTracks)
            {
                SimpleDf.SdfConsistencyLevel = SdfConsistencyLevel.none;
                trk.activate = true;
                trk.ActivateTrack();
                yield return null;
            }
        }
    }
    public IEnumerator RefreshTracks()
    {
        Debug.Log("RefreshTracks");
        foreach (var trk in vtm.vehicleTracks)
        {
            trk.DeleteTrack();
            trk.PlotStaticTrack();
            yield return new WaitForSeconds(1);
            break;
        }
    }

    bool lastStarted = false;
    bool lastFatTracks = false;

    void Update()
    {
        if (lastStarted!=started)
        {
            running = started;
            if (!tracksLoaded)
            {
                StartCoroutine(LoadTracks());
            }
        }
        if (fattracks!=lastFatTracks)
        {
            SetScale();
            StartCoroutine(RefreshTracks());
            lastFatTracks = fattracks;
        }
    }
}
