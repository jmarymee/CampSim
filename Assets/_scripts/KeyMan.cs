using System.Collections;
using System.Collections.Generic;
using UnityEngine;
#if USE_KEYWORDRECOGNIZER
using UnityEngine.Windows.Speech;
#endif
using System.Linq;
using GraphAlgos;

namespace CampusSimulator
{

#if USE_KEYWORDRECOGNIZER
    public class KeywordMan
    {
        RegionMan sman = null;

        KeywordRecognizer kR1 = null;
        KeywordRecognizer kR2 = null;

        public LinkedList<string> kwhistory = new LinkedList<string>();


        Dictionary<string, System.Action> keywords1 = null;
        Dictionary<string, System.Action> keywords2 = null;

        public KeywordMan(RegionMan sman)
        {
            this.sman = sman;
            initKeywords();
        }
        public int totalKeywordCount()
        {
            int ncnt = 0;
            if (keywords1 != null) ncnt += keywords1.Count;
            if (keywords2 != null) ncnt += keywords2.Count;
            return ncnt;
        }
        public int totalKeys1()
        {
            int ncnt = 0;
            if (keywords1 != null) ncnt += keywords1.Count;
            return ncnt;
        }
        public int totalKeys2()
        {
            int ncnt = 0;
            if (keywords2 != null) ncnt += keywords2.Count;
            return ncnt;
        }
        public string getlastwordused()
        {
            var rval = "";
            if (kwhistory != null)
            {
                if (kwhistory.Count > 0)
                {
                    rval = kwhistory.Last.Value;
                }
            }
            return rval;
        }
        void addStandardKeywords(Dictionary<string, System.Action> keywords)
        {
            keywords.Add("generate", () => { sman.CreateLinkCloud(); });
            keywords.Add("regen", () => { sman.CreateLinkCloud(); }); // why does this one not work
            keywords.Add("start router", () => { sman.CreateLinkCloud(); });
            keywords.Add("hide route", () => { sman.HideRoute(); });
            keywords.Add("show route", () => { sman.ShowRoute(); });
            keywords.Add("reset", () => { sman.ResetCalled(); });
            keywords.Add("begin", () => { sman.ResetCalled(); });
            keywords.Add("pause", () => { sman.PauseBird(); });
            keywords.Add("unpause", () => { sman.UnPauseBird(); });
            keywords.Add("resume", () => { sman.UnPauseBird(); });
            keywords.Add("random room", () => { sman.SetRandomEndNode(); });
            keywords.Add("change", () => { sman.NextBirdForm(); });
            keywords.Add("higher", () => { sman.FlyBirdHigher(); });
            keywords.Add("lower", () => { sman.FlyBirdLower(); });
            keywords.Add("stop", () => { sman.StopBird(); });
            keywords.Add("go fast", () => { sman.SetSpeed(4); });
            keywords.Add("faster", () => { sman.FasterBird(); });
            keywords.Add("slower", () => { sman.SlowerBird(); });
            keywords.Add("go slow", () => { sman.SetSpeed(0.5f); });
            keywords.Add("go", () => { sman.StartBird(); });
            keywords.Add("start", () => { sman.StartBird(); });
            keywords.Add("continue", () => { sman.StartBird(); });
            keywords.Add("reverse", () => { sman.ReversePath(); });

            keywords.Add("black balls", () => { sman.SetBallColor("black"); });
            keywords.Add("blue balls", () => { sman.SetBallColor("blue"); });
            keywords.Add("steel blue balls", () => { sman.SetBallColor("steelblue"); });
            keywords.Add("black pipes", () => { sman.SetPipeColor("black"); });
            keywords.Add("yellow pipes", () => { sman.SetPipeColor("yellow"); });
            keywords.Add("blue pipes", () => { sman.SetPipeColor("blue"); });
            keywords.Add("steel blue pipes", () => { sman.SetPipeColor("steelblue"); });

            keywords.Add("next bird form", () => { sman.NextBirdForm(); });

            keywords.Add("field of view 10", () => { sman.SetFov(10); });
            keywords.Add("field of view 20", () => { sman.SetFov(20); });
            keywords.Add("field of view 30", () => { sman.SetFov(30); });
            keywords.Add("field of view 40", () => { sman.SetFov(30); });
            keywords.Add("field of view 50", () => { sman.SetFov(30); });
            keywords.Add("field of view 60", () => { sman.SetFov(60); });
            keywords.Add("field of view 70", () => { sman.SetFov(60); });
            keywords.Add("field of view 80", () => { sman.SetFov(60); });
            keywords.Add("field of view 90", () => { sman.SetFov(90); });

            keywords.Add("grid on", () => { sman.GridOn(); });
            keywords.Add("grid off", () => { sman.GridOn(); });
            keywords.Add("grid bigger", () => { sman.GridBigger(); });
            keywords.Add("grid smaller", () => { sman.GridSmaller(); });


            keywords.Add("inc inc", () => { sman.IncInc(); });
            keywords.Add("dec inc", () => { sman.DecInc(); });

            keywords.Add("grow", () => { sman.Grow(); });
            keywords.Add("shrink", () => { sman.Shrink(); });

            keywords.Add("translate up", () => { sman.TranslateUp(); });
            keywords.Add("translate down", () => { sman.TranslateDown(); });

            keywords.Add("translate left", () => { sman.TranslateLeft(); });
            keywords.Add("translate right", () => { sman.TranslateRight(); });

            keywords.Add("translate forward", () => { sman.TranslateForward(); });
            keywords.Add("translate backward", () => { sman.TranslateBack(); });
            keywords.Add("translate back", () => { sman.TranslateBack(); });

            keywords.Add("rotate clockwise", () => { sman.RotateCw(); });
            keywords.Add("rotate counter clockwise", () => { sman.RotateCcw(); });

            keywords.Add("rotate right", () => { sman.RotateCw(); });
            keywords.Add("rotate left", () => { sman.RotateCcw(); });

            keywords.Add("grow 10", () => { sman.Grow10(); });
            keywords.Add("grow 50", () => { sman.Grow50(); });

            keywords.Add("shrink 10", () => { sman.Shrink10(); });
            keywords.Add("shrink 50", () => { sman.Shrink50(); });
            keywords.Add("shrink 5", () => { sman.Shrink50(); });

            keywords.Add("gen Redwest B 3", () => { sman.GenRedwb3(); });
            keywords.Add("gen Redwest Simple", () => { sman.GenRedwb3simple(); });
            keywords.Add("gen 43 1", () => { sman.Gen43_1(); });
            keywords.Add("gen 43 2", () => { sman.Gen431p2(); });
            keywords.Add("john Redwest B 3", () => { sman.GenRedwb3(); });
            keywords.Add("john Redwest Simple", () => { sman.GenRedwb3simple(); });
            keywords.Add("John 43 1", () => { sman.Gen43_1(); });
            keywords.Add("john 43 2", () => { sman.Gen431p2(); });
            keywords.Add("gen sphere", () => { sman.GenSphere(); });

            keywords.Add("garnish", () => { sman.NextGarnish(); });
            keywords.Add("correct position", () => { sman.CorrectPosition(); });
            keywords.Add("correct angle", () => { sman.CorrectAngle(); });
            keywords.Add("correct both", () => { sman.CorrectPositionAndAngle(); });

            keywords.Add("home", () => { sman.SetRoomAction(RegionMan.RoomActionE.makeHome); });
            keywords.Add("make start", () => { sman.SetRoomAction(RegionMan.RoomActionE.makeStart); });
            keywords.Add("orient", () => { sman.SetRoomAction(RegionMan.RoomActionE.orientOn); });
            keywords.Add("destination", () => { sman.SetRoomAction(RegionMan.RoomActionE.makeDestination); });

            keywords.Add("hide links", () => { sman.HideLinks(); });
            keywords.Add("show links", () => { sman.ShowLinks(); });

            keywords.Add("revert to home", () => { sman.RevertToHome(); });

            keywords.Add("scroll up", () => { sman.ScrollStatus(-1); });
            keywords.Add("scroll down", () => { sman.ScrollStatus(1); });
            keywords.Add("page up", () => { sman.ScrollPage(-1); });
            keywords.Add("page down", () => { sman.ScrollPage(1); });
            keywords.Add("top", () => { sman.ScrollStatus(-100000); });
            keywords.Add("bottom", () => { sman.ScrollStatus(100000); });

            keywords.Add("hide status", () => { sman.SetStatusInfoMode(StatusCtrl.outModeE.none); });
            keywords.Add("show status", () => { sman.SetStatusInfoMode(StatusCtrl.outModeE.geninfo); });

            keywords.Add("help", () => { sman.SetStatusInfoMode(StatusCtrl.outModeE.help); });
            keywords.Add("status trace", () => { sman.SetStatusInfoMode(StatusCtrl.outModeE.trace); });
            keywords.Add("status voice", () => { sman.SetStatusInfoMode(StatusCtrl.outModeE.voiceCmdHistory); });
            keywords.Add("status info", () => { sman.SetStatusInfoMode(StatusCtrl.outModeE.geninfo); });

            keywords.Add("load all rooms", () => { sman.SetStatusInfoMode(StatusCtrl.outModeE.geninfo); });

            keywords.Add("inc keyword limit", () => { sman.IncKeyLimit(); });
            keywords.Add("dec keyword limit", () => { sman.DecKeyLimit(); });

            keywords.Add("enable spatial mapping", () => { sman.EnableSpatialMapping(); });
            keywords.Add("disable spatial mapping", () => { sman.DisableSpatialMapping(); });
            keywords.Add("increase spatial extent", () => { sman.IncSpatialExtent(); });
            keywords.Add("decrease spatial extent", () => { sman.DecSpatialExtent(); });
            keywords.Add("more spatial detail", () => { sman.IncSpatialDetail(); });
            keywords.Add("less spatial detail", () => { sman.DecSpatialDetail(); });

            keywords.Add("toggle move camera", () => { sman.ToggleMoveCamera(); });
            keywords.Add("toggle drop error markers", () => { sman.ToggleDropErrorMarkers(); });
            keywords.Add("correct error markers", () => { sman.CorrectOnErrorMarkers(); });
            keywords.Add("start error marking", () => { sman.StartErrorMarking(); });
            keywords.Add("finish error marking", () => { sman.FinishErrorMarking(); });
            keywords.Add("toggle floor plan", () => { sman.ToggleFloorPlan(); });
            keywords.Add("Error corect on", () => { sman.SetErrorCorrect(true); });
            keywords.Add("Error correct off", () => { sman.SetErrorCorrect(false); });
        }

        // Use this for initialization
        void initKeywords()
        {
            keywords1 = new Dictionary<string, System.Action>();
            addStandardKeywords(keywords1);
            kR1 = new KeywordRecognizer(keywords1.Keys.ToArray());
            kR1.OnPhraseRecognized += kROnPhraseRecognized;
            kR1.Start();
            RegionMan.Log("keywords1 started num keys:" + keywords1.Count);
        }

        public void initKeywordsWithRooms()
        {
            keywords2 = new Dictionary<string, System.Action>();
            var keylist = sman.linkcloudctrl.GetKeywordKeys();
            int nadded = 0;
            foreach (var key in keylist)
            {
                var val = sman.linkcloudctrl.GetKeywordValue(key);
                keywords2.Add(key, () => { sman.NodeAction(val); });
                nadded += 1;
                RegionMan.Log("Adding key:" + key + "  Val:" + val);
            }
            if (kR2 != null)
            {
                kR2.Stop();
                kR2.Dispose();
                kR2 = null;
            }
            if (keywords2.Count > 0)
            {
                kR2 = new KeywordRecognizer(keywords2.Keys.ToArray());
                kR2.OnPhraseRecognized += kROnPhraseRecognized2;
                kR2.Start();
            }
            RegionMan.Log("keywords rooms num keys:" + keywords2.Count);
        }

        void kROnPhraseRecognized(PhraseRecognizedEventArgs args)
        {
            System.Action kwAction;
            if (keywords1.TryGetValue(args.text, out kwAction))
            {
                RegionMan.Log("You just said " + args.text);
                kwhistory.AddLast(args.text);
                kwAction.Invoke();
            }
            else
            {
                RegionMan.Log("Could not find " + args.text);
            }
        }
        void kROnPhraseRecognized2(PhraseRecognizedEventArgs args)
        {
            System.Action kwAction;
            if (keywords2.TryGetValue(args.text, out kwAction))
            {
                RegionMan.Log("You just said " + args.text);
                kwhistory.AddLast(args.text);
                kwAction.Invoke();
            }
            else
            {
                RegionMan.Log("Could not find " + args.text);
            }
        }
    }
#endif
}