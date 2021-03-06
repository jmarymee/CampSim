﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using GraphAlgos;

namespace CampusSimulator
{

    public class Leg
    {
        public string snode;
        public string enode;
        public BirdFormE form;
        public LcCapType capneed;
        public string formname;
        public Vehicle vehicle;
        public float vel;
        public float dist;
        public int distcount;
        public int destindex;
        override public string ToString()
        {
            return snode + " to " + enode + " "+" "+formname+" " + capneed + "  dist:" + dist.ToString("F1")+" cnt:"+distcount;
        }
    }

    public enum JourneyStatE { NotStarted, WaitingToStart, Started, AlmostFinished, Finished, Failed }

  
    public class JourneyMan : MonoBehaviour
    {

        private List<Journey> Journeys = new List<Journey>();
        public int njnys = 0;
        public int nlegs = 0;
        public SceneMan sman;
        private LinkCloudMan linkctrl;
        public int curjidx = 0;
        public float velfak = 1;
        public float pctInterBuildingJourneys = 0.5f;
        public GarageMan gm;
        public BuildingMan bm;
        public PersonMan pm;
        public ZoneMan zm;
        public string preferedJourneyBuildingName = "";
        public float preferedJourneyBuildingBias = 0.5f;
        public bool journeyMessages = false;
        public bool logJourneys = false;
        public string jnyLogFileName = "";
        public float startJnyTime = 0;
        public int journeysLogged = 0;

        public void AddJ(Journey jny)
        {
            Journeys.Add(jny);
            UpdateLegCount();
        }

        public void StartLoggingJourneys()
        {
            startJnyTime = Time.time;
            var scenename = sman.curregion.ToString();
            var dtnow = System.DateTime.UtcNow.ToString("yyyyMMdd-HHmmss");
            var jlogdir = "./jlog/";
            try
            {
                System.IO.Directory.CreateDirectory(jlogdir);
                jnyLogFileName = jlogdir + "Journeys-" + scenename + "-" + dtnow + ".csv";
            }
            catch(System.Exception ex)
            {
                logJourneys = false;
                var msg = "Error creating journey log directory";
                Debug.LogError(msg + ex.ToString());
            }
        }

        public void LogJourney(Journey jny,BldRoom b1,BldRoom b2)
        {
            if (!logJourneys) return;
            try
            {
                if (journeysLogged == 0)
                {
                    StartLoggingJourneys();
                    var headline = "njny,pname,aname,vid,vtype,roomfr,roomto,time,desc\n";
                    System.IO.File.AppendAllText(jnyLogFileName, headline);
                }
                jny.jnyTime = Time.time - startJnyTime;
                var pname = (jny.person ? jny.person.name : "");
                var aname = (jny.person ? jny.person.avatarName : "");
                var vname = (jny.vehicle ? jny.vehicle.name : "");
                var fname = (jny.vehicle ? jny.vehicle.formName : "");
                var r1name = (b1 ? b1.roomFullName : "");
                var r2name = (b2 ? b2.roomFullName : "");
                var line = journeysLogged + "," + pname + "," + aname + "," + vname + "," + fname + "," + r1name + "," + r2name + "," + jny.jnyTime.ToString("f3") + "," + jny.description + "\n";
                System.IO.File.AppendAllText(jnyLogFileName, line);
                journeysLogged++;
            }
            catch (System.Exception ex)
            {
                logJourneys = false;
                var msg = "Error logging journey";
                Debug.LogError(msg + ex.ToString());
            }
        }


        public List<Journey> GetJourneys()
        {
            return Journeys;
        }

        static int personcount = 0;
        static int girlcount = 0;
        const int maxgirl = 10;
        static int mancount = 1;
        const int maxman = 10;

        public float lvelfak = 1;
        string GetPersonFormname()
        {
            string rv = "";
            int diceroll = personcount;
            int gidx = (girlcount % maxgirl) + 1;
            int midx = (mancount % maxman) + 1;
            if (randomize)
            {
                diceroll = ranman.Next(0, 128);
                gidx = (ranman.Next(0, 128) % maxgirl) + 1;
                midx = (ranman.Next(0, 128) % maxman) + 1;
                // Debug.Log("personform dice:" + diceroll + " gidx:" + gidx + " midx:" + midx);
            }
            if (diceroll % 4 == 0)
            {
                rv = "girl" + gidx.ToString("D3");
                girlcount++;
            }
            else
            {
                rv = "man" + midx.ToString("D3");
                mancount++;
            }
            personcount++;
            return rv;
        }

        bool NodeExists(string nodename)
        {
            if (nodename==null) return false;
            return this.sman.linkcloudman.IsNodeName(nodename);
        }

        public Journey AddInterBuildingJourney(Person pers, BldRoom broom,int roomslot)
        {
            //Debug.Log("Adding Interbuilding Journey for " + pers.personName + " to " + broom.name);
            CheckFastMode();
            var frbroom = pers.GetCurrentRoom();
            if (!frbroom)
            {
                Debug.LogWarning(pers.personName + " is not in a room so can't do interbuilding journey - placeRoom:" + pers.placeRoom);
                return null;
            }
            if (broom == frbroom)
            {
                if (journeyMessages)
                {
                    Debug.LogWarning(pers.personName + " starting journey to same room - no journey - room:" + broom.roomFullName);
                }
                broom.UnReserveRoomSlot(roomslot); // same room - no journey
                return null;
            }
            //else
            //{
            //    Debug.Log("Clearly room " + broom.name + " is not the same as " + frbroom.name);
            //}


            var bld1 = frbroom.bld;
            var bld2 = broom.bld;
            if (!(bld1.name == bld2.name))
            {
                Debug.LogWarning("Not the same building - no journey frbroom:"+frbroom.roomFullName+"  broom:"+broom.roomFullName);
                broom.UnReserveRoomSlot(roomslot);// not in same building
                return null;
            }

            var perform = pers.avatarName;
            try
            {
                var jsnode = pers.placeNode;

                Leg leg = new Leg
                {
                    snode = jsnode,
                    enode = "",
                    form = BirdFormE.person,
                    capneed = LcCapType.walk,
                    formname = perform,
                    vel = 2 * lvelfak
                };
                var spos = linkctrl.GetNode(jsnode).pt;

                leg.enode = broom.roomNodeName; 
                if (!NodeExists(leg.enode)) return null;

                pers.PersonStateStartWaitingToTravel();
                var msg = (pers.personName+" interbuiding leg.snode:" + leg.snode + " enode:" + leg.enode );
                if (journeyMessages)
                {
                    Debug.Log(msg);
                }
                var stardelay = GraphAlgos.GraphUtil.GetRanFloat(0.5f, 5f);
                var jgo = new GameObject();
                var jny = jgo.AddComponent<Journey>();
                var findelaysecs = 2f;
                var stardelaysecs = GraphAlgos.GraphUtil.GetRanFloat(0, 15);
                jny.InitJourney(this, pers, null,frbroom,broom, msg, findelaysecs, stardelaysecs,jorg:"interbld");
                jny.AddLeg(leg);
                AddJ(jny);
                return jny;
            }
            catch (UnityException ex)
            {
                Debug.LogWarning("Could not add interbuilding journey for "+pers.personName +" from" + frbroom.name + " to "+broom.name+" " + ex.Message);
                broom.UnReserveRoomSlot(roomslot);
                return null;
            }
        }
        public Journey AddBldBldJourney(string fr_node, string tu_node, string pathname)
        {
            if (!NodeExists(fr_node)) return null;
            if (!NodeExists(tu_node)) return null;
            bool reservedCar = false;
            bool reservedSlot = false;
            GarageSlot sm1=null;
            GarageSlot sm2=null;
            try
            {
                var jsnode = fr_node;
                var jenode = tu_node;
                var gm = GameObject.FindObjectOfType<GarageMan>();

                var perform = GetPersonFormname();
           

                Leg leg1 = new Leg {
                    snode = jsnode,
                    enode = "",
                    form = BirdFormE.person,
                    capneed = LcCapType.walk,
                    formname = perform,
                    vel = 2 * lvelfak
                };
                var spos = linkctrl.GetNode(jsnode).pt;
                sm1 = gm.GetClosestFreeCar(spos);
                if (sm1 == null)
                {
                    if (journeyMessages)
                    {
                        Debug.LogWarning("Could not create journey from " + pathname + "- no free cars at start point");
                    }
                    return null;
                }
                sm1.ReserveCar();
                reservedCar = true;
                leg1.enode = sm1.nodename;
                if (!NodeExists(leg1.enode)) return null;
                //Debug.Log("leg1.enode:" + leg1.enode + " pername:" + perform + " carname:" + sm1.carname);

                var carform = BirdFormE.car;
                if (gm.slotform.Get() == SlotFormE.Node)
                {
                    carform = BirdFormE.sphere;
                }
                Leg leg2 = new Leg {
                    snode = leg1.enode,
                    enode = "",
                    form = carform,
                    vehicle = sm1.vehicle,
                    capneed = LcCapType.drive,
                    formname = sm1.carformname,
                    vel = 20 * lvelfak
                };
                var epos = linkctrl.GetNode(jenode).pt;
                sm2 = gm.GetClosestAvailableSlot(epos);
                if (sm2 == null)
                {
                    if (journeyMessages)
                    {
                        if (reservedCar)
                        {
                            sm1.UnReserveCar();
                        }
                        Debug.LogWarning("Could not create journey from " + pathname + " - no parking slots available at destination");
                    }
                    return null;
                }
                leg2.enode = sm2.nodename;
                if (!NodeExists(leg2.enode)) return null;

                //Debug.Log("leg2.enode:" + leg2.enode);
                sm2.ReserveSlot();
                reservedSlot = true;

                Leg leg3 = new Leg {
                    snode = leg2.enode,
                    enode = jenode,
                    form = BirdFormE.person,
                    capneed = LcCapType.walk,
                    formname = perform,
                    vel = 2 * lvelfak
                };
                var description = " - " + pathname + " " + perform + " " + sm1.carformname;
                var jgo = new GameObject();
                var jny = jgo.AddComponent<Journey>();
                jny.InitJourney(this, null,null,null,null, description,jorg: "ephemeral");
                jny.AddLeg(leg1);
                jny.AddLeg(leg2);
                jny.AddLeg(leg3);
                AddJ(jny);
                return jny;
            }
            catch (UnityException ex)
            {
                Debug.LogWarning("Could not add journey from " + fr_node + " to " + tu_node + " " + ex.Message);
                if (reservedCar)
                {
                    sm1.UnReserveCar();
                }
                if (reservedSlot)
                {
                    sm1.UnReserveSlot();
                }
                return null;
            }
        }

        public Journey AddPersonBldroomJourney(Person person, BldRoom broom,int roomslot)
        {
            if (!person)
            {
                Debug.LogWarning("Null person in AddPersonBldroomJourney");
                return null;
            }
            var fr_node = person.placeNode;
            var tu_node = broom.roomNodeName;
            if (!NodeExists(fr_node)) return null;
            if (!NodeExists(tu_node)) return null;
            GarageSlot sm1 = null;
            GarageSlot sm2 = null;
            try
            {
                var jsnode = fr_node;
                var jenode = tu_node;
                var gm = GameObject.FindObjectOfType<GarageMan>();

                var perform = person.avatarName;
                var frroom = bm.GetBroom(person.placeRoom);

                Leg leg1 = new Leg
                {
                    snode = jsnode,
                    enode = "",
                    form = BirdFormE.person,
                    capneed = LcCapType.walk,
                    formname = perform,
                    vel = 2 * lvelfak
                };
                var spos = linkctrl.GetNode(jsnode).pt;
                sm1 = gm.GetClosestFreeCar(spos);
                if (sm1 == null)
                {
                    if (journeyMessages)
                    {
                        Debug.LogWarning("Could not create journey for " + person.personName + "- no free cars at start point");
                    }
                    broom.UnReserveRoomSlot(roomslot);
                    return null;
                }
                sm1.ReserveCar();
                leg1.enode = sm1.nodename;
                if (!NodeExists(leg1.enode)) return null;
                //Debug.Log("leg1.enode:" + leg1.enode + " pername:" + perform + " carname:" + sm1.carname);

                var carform = BirdFormE.car;
                if (gm.slotform.Get() == SlotFormE.Node)
                {
                    carform = BirdFormE.sphere;
                }
                Leg leg2 = new Leg
                {
                    snode = leg1.enode,
                    enode = "",
                    form = carform,
                    capneed = LcCapType.drive,
                    vehicle = sm1.vehicle,
                    formname = sm1.carformname,
                    vel = 20 * lvelfak
                };

                var epos = linkctrl.GetNode(jenode).pt;
                sm2 = gm.GetClosestAvailableSlot(epos);
                if (sm2 == null)
                {
                    if (journeyMessages)
                    {
                        Debug.LogWarning("Could not create journey for "+person.personName +"  to  " + broom.name + " - no parking slots at dest");
                    }
                    if (sm1)
                    {
                        sm1.UnReserveCar();
                    }
                    broom.UnReserveRoomSlot(roomslot);
                    return null;
                }
                leg2.enode = sm2.nodename;
                if (!NodeExists(leg2.enode)) return null;

                //Debug.Log("leg2.enode:" + leg2.enode);
                var gslotAvail = sm2.ReserveSlot();
                if (!gslotAvail)
                {
                    if (journeyMessages)
                    {
                        Debug.LogWarning("Could not create journey for " + person.personName + "  to  " + broom.name + " - cound not reserve parking slot at dest");
                    }
                    broom.UnReserveRoomSlot(roomslot);
                    if (sm1)
                    {
                        sm1.UnReserveCar();
                    }
                    if (sm2)
                    {
                        sm2.UnReserveSlot();
                    }
                    return null;
                }

                Leg leg3 = new Leg
                {
                    snode = leg2.enode,
                    enode = jenode,
                    form = BirdFormE.person,
                    capneed = LcCapType.walk,
                    formname = perform,
                    vel = 2 * lvelfak
                };
                var msg = person.personName + " traveling to " + broom.name + " with a " + sm1.carformname;
                person.PersonStateStartWaitingToTravel();
                var stardelay = GraphAlgos.GraphUtil.GetRanFloat(0.5f, 5f);
                var jgo = new GameObject();
                var jny = jgo.AddComponent<Journey>();
                // jny.InitJourney(this, null, msg);
                jny.InitJourney(this, person, sm1.vehicle,frroom,broom, msg, 5, stardelay,jorg:"pers,bld,rm");
                jny.AddLeg(leg1);
                jny.AddLeg(leg2);
                jny.AddLeg(leg3);
                AddJ(jny);
                return jny;
            }
            catch (UnityException ex)
            {
                Debug.LogWarning("Could not add journey for "+person.personName+ " from " + fr_node + " to " + tu_node + " " + ex.Message);
                return null;
            }
        }

        public Journey AddEvacJourney(Person person, string pathname)
        {
            var fr_node = person.placeNode;
            var perform = person.avatarName;
            if (!NodeExists(fr_node)) return null;
            try
            {
                var jsnode = fr_node;

                Leg leg = new Leg
                {
                    snode = jsnode,
                    enode = "",
                    form = BirdFormE.person,
                    capneed = LcCapType.walk,
                    formname = perform,
                    vel = 2 * lvelfak
                };
                var spos = linkctrl.GetNode(jsnode).pt;
                var zslt = zm.GetClosestAvailableSlot(spos);
                if (zslt == null)
                {
                    if (journeyMessages)
                    {
                        Debug.LogWarning("Could not create evac journey for " + person.personName + " at " + person.placeNode + "- no available slot in an evac zone");
                    }
                    return null;
                }
                zslt.Reserve();
                //Debug.Log("Reserved slot " + sm1.name + " for " + pers.personName);
                leg.enode = zslt.nodename;
                if (!NodeExists(leg.enode)) return null;
                person.PersonStateStartWaitingToTravel();
                var msg = ("Evac leg.snode:" + leg.snode + " enode:" + leg.enode + " pername:" + person.personName);
                if (journeyMessages)
                {
                    Debug.Log(msg);
                }
                var stardelay = GraphAlgos.GraphUtil.GetRanFloat(0.5f, 5f);
                var jgo = new GameObject();
                var jny = jgo.AddComponent<Journey>();
                var frroom = bm.GetBroom(person.placeRoom);
                jny.InitJourney(this, person, null,frroom,null, msg, 0, stardelay,jorg:"evac");
                jny.AddLeg(leg);
                AddJ(jny);
                return jny;
            }
            catch (UnityException ex)
            {
                Debug.LogWarning("Could not add journey from " + fr_node + " to evac zone " + ex.Message);
                return null;
            }
        }



        public Journey AddPersonToRoomJourney(Person pers, string roomname, string pathname, float startdelaysecs = -1)
        {
            CheckFastMode();
            var fr_node = pers.GetWaitNodeName();
            var perform = pers.avatarName;
            if (!NodeExists(fr_node)) return null;
            try
            {
                var jsnode = fr_node;
                var froom = bm.GetBroom(pers.placeRoom);
                var broom = bm.GetBroom(roomname);

                Leg leg = new Leg
                {
                    snode = jsnode,
                    enode = "",
                    form = BirdFormE.person,
                    capneed = LcCapType.walk,
                    formname = perform,
                    vel = 2 * lvelfak
                };
                var spos = linkctrl.GetNode(jsnode).pt;

                leg.enode = roomname;
                if (!NodeExists(leg.enode)) return null;

                pers.PersonStateStartWaitingToTravel();
                var msg = ("Perstorom leg.snode:" + leg.snode + " enode:" + leg.enode + " pername:" + pers.personName);
                if (journeyMessages)
                {
                    Debug.Log(msg);
                }
                var jgo = new GameObject();
                var jny = jgo.AddComponent<Journey>();
                var findelaysecs = 2f;
                if (startdelaysecs < 0)
                { 
                    startdelaysecs = GraphAlgos.GraphUtil.GetRanFloat(0, 15);
                }
                jny.InitJourney(this, pers, null,froom,broom, msg, findelaysecs, startdelaysecs,jorg:"fixedpers,room");
                jny.AddLeg(leg);
                AddJ(jny);
                return jny;
            }
            catch (UnityException ex)
            {
                Debug.LogWarning("Could not add journey from " + fr_node + " to evac zone " + ex.Message);
                return null;
            }
        }

        public Journey AddJourneyBackHome(Person pers, string pathname, float startdelaysecs = -1) // this could probably be subsituted with the above
        {
            CheckFastMode();
            var fr_node = pers.placeNode;
            var perform = pers.avatarName;
            if (!NodeExists(fr_node)) return null;
            try
            {
                var jsnode = fr_node;

                Leg leg = new Leg
                {
                    snode = jsnode,
                    enode = "",
                    form = BirdFormE.person,
                    capneed = LcCapType.walk,
                    formname = perform,
                    vel = 2 * lvelfak
                };
                var spos = linkctrl.GetNode(jsnode).pt;

                leg.enode = pers.homeNode;  // just go home, hope it isn't full already :)
                if (!NodeExists(leg.enode)) return null;

                pers.PersonStateStartWaitingToTravel();
                var msg = ("Evac leg.snode:" + leg.snode + " enode:" + leg.enode + " pername:" + pers.personName);
                if (journeyMessages)
                {
                    Debug.Log(msg);
                }
                var stardelay = GraphAlgos.GraphUtil.GetRanFloat(0.5f, 5f,"jnygen");
                var jgo = new GameObject();
                var jny = jgo.AddComponent<Journey>();
                var findelaysecs = 2f;
                if (startdelaysecs < 0)
                {
                    startdelaysecs = GraphAlgos.GraphUtil.GetRanFloat(0, 15);
                }
                var froom = bm.GetBroom(pers.placeRoom,expectFailure:true);
                var broom = bm.GetBroom(pers.homeRoom);
                jny.InitJourney(this, pers,null,froom,broom, msg, findelaysecs, startdelaysecs,jorg:"backhome");
                jny.AddLeg(leg);
                AddJ(jny);
                return jny;
            }
            catch (UnityException ex)
            {
                Debug.LogWarning("Could not add journey from " + fr_node + " to evac zone " + ex.Message);
                return null;
            }
        }

        public Journey AddBldBldJourneyWithEphemeralPeople(string b1, string b2)
        {
            CheckFastMode();
            var bc1 = bm.GetBuilding(b1);
            if (bc1 == null)
            {
                return null;
            }
            var bc2 = bm.GetBuilding(b2);
            if (bc1 == null)
            {
                return null;
            }
            var pathname = bc1.shortname + " to " + bc2.shortname;
            if (journeyMessages)
            {
                Debug.Log("Spawning journey " + pathname);
            }
            var bdest1 = bc1.GetRandomDest("jnygen");
            var bdest2 = bc2.GetRandomDest("jnygen");
            var jny = AddBldBldJourney(bdest1,bdest2, pathname);
            return jny;
        }
        public void CheckFastMode()
        {
            if (sman.fastMode)
            {
                lvelfak = 2.5f;
            }
            else
            {
                lvelfak = 1.0f;
            }
        }
        public Journey AddPersonJourney(Person person)
        {
            CheckFastMode();
            var bc1 = person.GetBuilding();
            if (bc1 == null) return null;
            var bc2 = bm.GetRandomBld("jnygen");
            if (bc2 == null) return null;
            var room = bc2.GetRandomRoom("jnygen");
            var roomslot = room.ReserveRoomSlot();
            if (journeyMessages)
            {
                Debug.Log("Spawning journey for " + person.personName + "from " + person.placeRoom + " to " + room.roomFullName);
            }
            return AddPersonBldroomJourney(person, room, roomslot);
        }
        public Journey AddPersonBldRoomJourney(string b1, string b2)
        {
            CheckFastMode();
            var bc1 = bm.GetBuilding(b1);
            if (bc1 == null)
            {
                Debug.LogWarning("AddPersonBldRoomJourney failed building b1:" + b1 + " does not exist");
                return null;
            }
            var person = bc1.GetRandomFreeToTravelPerson("jnygen");
            if (person == null)
            {
                Debug.LogWarning("AddPersonBldRoomJourney failed building b1:" + b1 + " GetRandomFreeToTravel Person returned null");
                return null;
            }
            bool interBuilding = GraphAlgos.GraphUtil.FlipBiasedCoin(pctInterBuildingJourneys);
            var bc2 = bm.GetBuilding(b2);
            if (interBuilding)
            {
                bc2 = bc1;
            }
            if (bc2 == null)
            {
                Debug.LogWarning("AddPersonBldRoomJourney failed building b2:" + b2 + " does not exist");
                return null;
            }
            var room = bc2.GetRandomRoom("jnygen");
            var roomslot = room.ReserveRoomSlot();
            if (journeyMessages)
            {
                Debug.Log("Spawning journey for " + person.personName+" from "+person.placeRoom + " to "+room.roomFullName);
            }
            Journey jny;
            if (interBuilding || bc1 == bc2) // 2nd clause might be true by accident
            {
                jny = AddInterBuildingJourney(person, room, roomslot);
            }
            else
            {
                jny = AddPersonBldroomJourney(person, room, roomslot);
            }
            return jny;
        }

        public void LaunchArnie()
        {
            var pers = sman.psman.GetPerson("Arnie Schwarzwald");
            var room = "b19-f01-rm1031";
            var msg = "Go get em";
            //pers.AddNewCamera("Arnie Launcher");
            pers.GrabMainCamera();
            if (sman.curregion == SceneSelE.Eb12 )
            {
                room = "eb12-12-lob";
            }
            if (pers.IsInHomeRoom())
            {
                AddPersonToRoomJourney(pers, room, msg,startdelaysecs:1);
            }
            else
            {
                msg = "get back to work Arnie";
                AddJourneyBackHome(pers, msg, startdelaysecs: 2);
            }
            Debug.Log("Arnie Launched to " + room + " - " + msg);
        }

        public void BatchEvacJourneys(string b1, int n)
        {
            var bld = bm.GetBuilding(b1);
            if (bld == null)
            {
                return;
            }
            var perlist = bld.GetAllPeopleInRooms();
            if (n > perlist.Count) n = perlist.Count;

            for (int i= 0; i<n; i++)
            {
                var pers = perlist[i];
                var pathname = "Spawning evacuation of " + pers.personName + " from building " + bld.shortname + " to an evacuation zone";
                if (journeyMessages)
                {
                  Debug.Log(pathname);
                }
                AddEvacJourney(pers, pathname);
            }
        }
        public void BatchAllClearJourneys(string z1, int n)
        {
            var zone = zm.GetZone(z1);
            if (zone == null)
            {
                return;
            }
            var perlist = zone.GetFreePeopleInZone();
            if (n > perlist.Count) n = perlist.Count;

            for (int i = 0; i < n; i++)
            {
                var pers = perlist[i];
                var pathname = "Spawning unevacuation of " + pers.personName + " from zone " + zone.name + " to homeroom:"+pers.homeNode;
                if (journeyMessages)
                {
                    Debug.Log(pathname);
                }
                AddJourneyBackHome(pers, pathname);
            }
        }
        bool doStreamJourneys = false;
        string streamZone = "";
        Building streamBld = null;
        float streamGapMax = 4.0f;
        float streamGapMin = 2.5f;
        float streamGapCur = 0;
        float streamLastTime = 0;
        int nstreamers = 0;
        public void CreateStreamJourneys(string zonename, Building bld)
        {
            Debug.Log("CreateStreamJourneys for " + zonename + " Bld:" + bld.name);
            var zone = zm.GetZone(zonename);
            if (!zone)
            {
                Debug.LogWarning("Zone " + zonename + " not found");
                return;
            }
            var pers = pm.MakeRandomPerson(1)[0];


            pers.homeBld = bld.name;
            pers.homeNode = bld.GetRandomDest();

            if (nstreamers % 6 == 3) // every 6 times a new one comes, but the first one comes after 3
            {
                pers.empStatus = PersonMan.empStatusE.Unknown;
                if (sman.curregion== SceneSelE.MsftB19focused)
                {
                    if (bld.name=="Bld19")
                    {
                        pers.homeNode = "b19-f01-rm1031";// send them all to the same room
                    }
                }
            }
            else
            {
                if (pers.empStatus== PersonMan.empStatusE.Unknown)// squash rouge unknowns
                {
                    pers.empStatus = PersonMan.empStatusE.FullTimeEmp;
                }
            }


            var zslt = zone.GetClosestAvailableSlot(Vector3.zero);
            pers.placeBld = zone.name;
            pers.placeNode = zslt.nodename;
            var pathname = "Spawning streamjourney of " + pers.empStatus + " from zone " + zone.name + " to homeroom:" + pers.homeNode;
            Debug.Log(pathname);
            Debug.Log("journeymessages:" + journeyMessages);
            if (journeyMessages)
            {
                Debug.Log(pathname);
            }
            AddJourneyBackHome(pers, pathname);
            nstreamers++;
        }


        public void spawnStreamJourneys()
        {
            var now = Time.time;
            if ((now-streamLastTime)>streamGapCur)
            {
                CreateStreamJourneys(streamZone,streamBld);
                streamGapCur = GraphAlgos.GraphUtil.GetRanFloat(streamGapMin, streamGapMax, "spawnstreaming");
                streamLastTime = now;
            }
        }

        public void StartStreamingJourneys(string zonename, Building bld)
        {
            doStreamJourneys = !doStreamJourneys;
            Debug.Log("doStreamJourneys:" + doStreamJourneys);
            streamZone = zonename;
            streamBld = bld;
            streamLastTime = Time.time;
        }
        // Use this for initialization
        void Start()
        {
            sman = FindObjectOfType<SceneMan>();
            linkctrl = sman.linkcloudman;
            gm = sman.gaman;
            bm = sman.bdman;
            pm = sman.psman;
            zm = sman.znman;
        }
        void UpdateLegCount()
        {
            njnys = Journeys.Count;
            int legsum = 0;
            foreach (Journey jny1 in Journeys) legsum += jny1.nlegs;
            nlegs = legsum;
        }
        public void DeleteJourney(Journey jny)
        {
            if (jny.person)
            {
                jny.person.journey = null;
            }
            jny.birdctrl.DeleteBird();
            // what about the animator?
            Destroy(jny.birdctrl);
            Destroy(jny.pathctrl);
            Journeys.Remove(jny);
            UpdateLegCount();
        }
        public bool spawnjourneys = false;
        public bool randomize = false;
        public bool allowdupdests = false;
        public float spawninterval = 4f; // each journey takes around 75 secs
        public int nspawnbatch = 10;
        public float lastspawn = 0;
        public int nspawntries = 0;
        public int nspawned = 0;
        public int nspawnfails = 0;
        System.Random ranman = new System.Random();

        public void DeleteAllJourneys()
        {
            foreach (var jny in Journeys)
            {
                jny.birdctrl.DeleteBird();
                // what about the animator?
                Destroy(jny);
            }
            Journeys = new List<Journey>();
            UpdateLegCount();
            spawnjourneys = false;
        }

        public void SetScene(SceneSelE newregion)
        {
            DeleteAllJourneys();
        }
        string ObjDetClassify(string resname)
        {
            resname = resname.ToLower();
            string rv = "car";
            if (resname=="")
            {
                rv = "";
            }
            else if (resname.StartsWith("girl") || resname.StartsWith("man") || resname.StartsWith("businessman") || resname.StartsWith("businesswoman"))
            {
                rv = "person";
            }
            //Debug.Log(resname + " classified as " + rv);
            return rv;
        }
        void SpawnJourneysByBuilding()
        {
            var nbldcnt = bm.GetBuildingCount();
            if (nbldcnt == 0)
            {
                nspawnfails++;
                Debug.LogWarning("Cannot spawn jouneys withoug buildings nbldcnt:" + nbldcnt + " tries:" + nspawntries + " fails:" + nspawnfails);
                return; // no buildings so no journeys
            }
            if (nspawntries == 0 || (Time.time - lastspawn) > spawninterval)
            {
                for (int i = 0; i < nspawnbatch; i++)
                {
                    lvelfak = 1;
                    if (sman.fastMode)
                    {
                        lvelfak = 2.5f;
                    }
                    //Debug.Log("spawning nbldcnt:" + nbldcnt + " tries:" + nspawntries + " fails:" + nspawnfails);
                    Journey jny;
                    var b1 = bm.GetRandomBldName("");
                    if (preferedJourneyBuildingName != "")
                    {
                        if (GraphAlgos.GraphUtil.FlipBiasedCoin(preferedJourneyBuildingBias))
                        {
                            b1 = preferedJourneyBuildingName;
                        }
                    }
                    var donotchoose = (allowdupdests ? "" : b1);
                    var b2 = bm.GetRandomBldName(donotchoose);
                    if (preferedJourneyBuildingName != "")
                    {
                        if (GraphAlgos.GraphUtil.FlipBiasedCoin(preferedJourneyBuildingBias))
                        {
                            b2 = preferedJourneyBuildingName;
                        }
                    }
                    var movepeople = true;
                    if (movepeople)
                    {
                        jny = AddPersonBldRoomJourney(b1, b2);
                    }
                    else
                    {
                        jny = AddBldBldJourneyWithEphemeralPeople(b1, b2);
                    }
                    if (jny == null)
                    {
                        nspawnfails++;
                    }
                    else
                    {
                        nspawned++;
                    }
                    nspawntries++;
                    lastspawn = Time.time;
                }
            }
        }



        public void ModifyJourneySpeeds()
        {
            bool frontjnyhack = true;
            var njnys = Journeys.Count;
            for (var i = 0; i < njnys; i++)
            {
                var jny = Journeys[i];
                var setit = false;
                if (frontjnyhack && jny.frontjny && jny.frontjny.birdctrl.wegguid!=jny.birdctrl.wegguid)
                {
                    var bc1 = jny.birdctrl;
                    var bc2 = jny.frontjny.birdctrl;
                    if (bc1 && bc2)
                    {
                        var pp1 = bc1.pathpos;
                        var pp2 = bc2.pathpos;
                        if (pp1 != null && pp2 != null)
                        {
                            var dist = Vector3.Distance(bc1.pathpos.pt, bc2.pathpos.pt);
                            jny.frontdist = dist;
                            setit = true;
                        }
                    }
                    //Debug.Log(jny.person.personName+" frontdist preinited to " + dist);
                } 
                if (!setit)
                {
                    jny.frontjny = null;
                    jny.frontdist = 9e9f;
                }
                jny.backjny = null;
                jny.backdist = 9e9f;
                jny.njnysOnWeg = 0;
            }

            for (var i1=0; i1<njnys; i1++)
            {
                var jny1 = Journeys[i1];
                var bc1 = jny1.birdctrl;
                for (var i2 = 0; i2<njnys; i2++)
                {
                    if (i1 == i2) continue;
                    var jny2 = Journeys[i2];
                    var bc2 = jny2.birdctrl;
                    if (bc1.wegguid==bc2.wegguid)
                    {
                        jny1.njnysOnWeg++;
                        jny2.njnysOnWeg++;
                        var dist = (bc1.wegdistance - bc2.wegdistance);
                        // Checks out...
                        //var dist2 = Vector3.Distance(bc1.pathpos.pt, bc2.pathpos.pt);
                        //var pctdif = (dist - dist2) / Mathf.Max(0.001f, dist + dist2);
                        //if (pctdif>=0.0f)
                        //{
                        //    Debug.Log("Pctdif:" + pctdif);
                        //}

                        var jf = jny1;
                        var jb = jny2;
                        if (dist<0)
                        {
                            jb = jny1;
                            jf = jny2;
                            dist = -dist;
                        }
                        if (dist<jf.backdist)
                        {
                            jf.backdist = dist;
                            jf.backjny = jb;
                        }
                        if (dist < jb.frontdist)
                        {
                            jb.frontdist = dist;
                            jb.frontjny = jf;
                        }
                    }
                }
            }
            for (var i = 0; i < njnys; i++)
            {
                var jny = Journeys[i];
                var fak = 1;
                if (jny.currentleg!=null)
                {
                    if (jny.currentleg.capneed == LcCapType.drive) fak = 6;
                }
                if (jny.frontdist<0.1f*fak)
                {
                    jny.birdctrl.BirdSpeedFactor = GraphAlgos.GraphUtil.GetRanFloat(0.05f, 0.15f,"jngyen")*fak;
                }
                else if (jny.frontdist < 1*fak)
                {
                    jny.birdctrl.BirdSpeedFactor = jny.frontdist*fak;
                }
                else if (jny.backdist < 1*fak)
                {
                   jny.birdctrl.BirdSpeedFactor = 2.0f-jny.backdist;
                }
                else
                {
                    jny.birdctrl.BirdSpeedFactor = 1.0f;
                }
            }
        }
        int updateCount = 0;
        // Update is called once per frame
        float lastSpawnTime = 0;
        void Update() 
        {
            updateCount++;
            if (updateCount<=8) // no journeys for the first 8 updates
            {
                return;
            }

            // Journey housekeeping
            var nstarted = 0;
            var ndeleted = 0;
            var deletelist = new List<Journey>();
            if ((Time.time - lastSpawnTime) > 0.125f)
            {
                foreach (var jny in Journeys)
                {
                    if (nstarted > 16) break;
                    switch (jny.status)
                    {
                        case JourneyStatE.WaitingToStart:
                            if ((Time.time - jny.createtime) > jny.startSecsToDelay)
                            {
                                jny.StartJourney();
                                nstarted++;
                            }
                            break;
                        case JourneyStatE.Started:
                            if (jny.birdctrl.BirdState == BirdStateE.atgoal)
                            {
                                jny.NextLeg();
                            }
                            break;
                        case JourneyStatE.Failed:
                            if ((Time.time - jny.failedtime) > jny.failedSecsToDestroy)
                            {
                                deletelist.Add(jny);
                            }
                            break;
                        case JourneyStatE.AlmostFinished:
                            if ((Time.time - jny.finishedtime) > jny.finishSecsToDestroy)
                            {
                                jny.FinishJourney();
                                deletelist.Add(jny);
                            }
                            break;
                    }
                }
                lastSpawnTime = Time.time;
            }
            foreach ( var jny in deletelist)
            {
                DeleteJourney(jny);
                Destroy(jny.gameObject);
                ndeleted++;
            }
            // Journey spawning
            if (spawnjourneys)
            {
                SpawnJourneysByBuilding();
            }
            if (doStreamJourneys)
            {
                spawnStreamJourneys();
            }
            ModifyJourneySpeeds();
            //Debug.Log("Updatecount:" + updateCount + " started:" + nstarted + " deleted:" + ndeleted);
        }
    }
}