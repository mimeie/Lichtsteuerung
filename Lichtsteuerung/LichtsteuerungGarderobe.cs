using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Diagnostics;


using JusiBase;

namespace Lichtsteuerung
{
    

    public class LichtsteuerungGarderobe 
    {

        static readonly object logikLock = new object();        

        //für die state machine
        public StateMachineLogic StateMachine;

        //objekte
        //public Multisensor Garderobe;
        public SensorBool GarderobeBewegung;
        public SensorHelligkeit GarderobeHelligkeit;

        public SensorIntToBool HaustuerBewegung;

        public LampeHelligkeit LichtGarderobe;

        public SensorBool GarageTuer;



        public LichtsteuerungGarderobe()
        {
            if (SteuerungLogic.Instance.IsDebug == false)
            {
                GarderobeBewegung = new SensorBool("zigbee.0.00158d00057f826b.occupancy");
                GarderobeBewegung.MinLaufzeitMinutes = 5;
                GarderobeHelligkeit = new SensorHelligkeit("zigbee.0.00158d00057f826b.illuminance", 70);
                HaustuerBewegung = new SensorIntToBool("zwave2.0.Node_023.Basic.currentValue");

                GarageTuer = new SensorBool("zigbee.0.00158d00045b0885.contact");
            }
            else
            {
                GarderobeBewegung = new SensorBool("0_userdata.0.DebugLichtsteuerung.Bewegung");
                GarderobeBewegung.MinLaufzeitMinutes = 4;
                GarderobeHelligkeit = new SensorHelligkeit("0_userdata.0.DebugLichtsteuerung.Helligkeit", 50);
                HaustuerBewegung = new SensorIntToBool("0_userdata.0.DebugLichtsteuerung.BewegungInt");

                GarageTuer = new SensorBool("0_userdata.0.DebugLichtsteuerung.TuerKontakt");
            }

            //Garderobe = new Multisensor();
            //Garderobe.Bewegung = GarderobeBewegung;
            //Garderobe.Helligkeit = GarderobeHelligkeit;




            LichtGarderobe = new LampeHelligkeit("", "", "zwave2.0.Node_034.Multilevel_Switch.targetValue", "zwave2.0.Node_034.Multilevel_Switch.targetValue");


            //StateMachine init
            StateMachine = new StateMachineLogic();
            StateMachine.CurrentState = State.Aus;

            StateMachine._transitions.Add(new StatesTransition(State.Deaktiviert, Signal.GotoAus, GotoStateAus, State.Aus));

            StateMachine._transitions.Add(new StatesTransition(State.Aus, Signal.GotoDeaktiviert, GotoStateDeaktiviert, State.Deaktiviert));
            StateMachine._transitions.Add(new StatesTransition(State.Aus, Signal.GotoReadyForAction, GotoStateReadyForAction, State.ReadyForAction));

            StateMachine._transitions.Add(new StatesTransition(State.ReadyForAction, Signal.GotoAus, GotoStateAus, State.Aus));
            StateMachine._transitions.Add(new StatesTransition(State.ReadyForAction, Signal.GotoAction, GotoStateAction, State.Action));
            StateMachine._transitions.Add(new StatesTransition(State.ReadyForAction, Signal.GotoDeaktiviert, GotoStateDeaktiviert, State.Deaktiviert));

            StateMachine._transitions.Add(new StatesTransition(State.Action, Signal.GotoReadyForAction, GotoStateReadyForAction, State.ReadyForAction));
            StateMachine._transitions.Add(new StatesTransition(State.Action, Signal.GotoAus, GotoStateAus, State.Aus));
            StateMachine._transitions.Add(new StatesTransition(State.Action, Signal.GotoDeaktiviert, GotoStateDeaktiviert, State.Deaktiviert));



           
            
        }

        public void Initialize()
        {

            Console.WriteLine("updates der anlage holen");
            GarderobeBewegung.Update();
            GarderobeHelligkeit.Update();
            HaustuerBewegung.Update();
            LichtGarderobe.UpdateHelligkeit();

            GarageTuer.Update();

            GarderobeBewegung.DataChange += DoDataChange;
            GarderobeHelligkeit.DataChange += DoDataChange;
            HaustuerBewegung.DataChange += DoDataChange;
            GarageTuer.DataChange += DoDataChange;
            SteuerungLogic.Instance.JemandZuhause.DataChange += DoDataChange;

            Console.WriteLine("updates der anlage geholt");

            LichtsteuerungLogikAllOthers(SteuerungLogic.Instance.JemandZuhause);

        }

        private void GotoStateAus()
        {
            Console.WriteLine("Executed: GotoStateAus");
            if (LichtGarderobe.Helligkeit > 0)
            {
                LichtGarderobe.ZielHelligkeit = 0;
            }

        }

        private void GotoStateDeaktiviert()
        {
            Console.WriteLine("Executed: GotoStateDeaktiviert");
            if (LichtGarderobe.Helligkeit > 0)
            {
                LichtGarderobe.ZielHelligkeit = 0;
            }

        }

        private void GotoStateAction()
        {
            Console.WriteLine("Executed: GotoStateAction");
            if (LichtGarderobe.Helligkeit == 0)
            {
                LichtGarderobe.ZielHelligkeit = 80;
            }
        }

        private void GotoStateReadyForAction()
        {
            Console.WriteLine("Executed: GotoStateReadyForAction");
            if (LichtGarderobe.Helligkeit > 0)
            {
                LichtGarderobe.ZielHelligkeit = 0;
            }
        }

        public void LichtsteuerungLogikAllOthers(Objekt source)
        {
            if (source != SteuerungLogic.Instance.JemandZuhause)
            {
                LichtsteuerungLogik(SteuerungLogic.Instance.JemandZuhause);
            }
            if (source != GarderobeHelligkeit)
            {
                LichtsteuerungLogik(GarderobeHelligkeit);
            }
            if (source != GarderobeBewegung)
            {
                LichtsteuerungLogik(GarderobeBewegung);
            }
            if (source != HaustuerBewegung)
            {
                LichtsteuerungLogik(HaustuerBewegung);
            }
            if (source != GarageTuer)
            {
                LichtsteuerungLogik(GarageTuer);
            }

        }

        private void LichtsteuerungLogik(Objekt source)
        {
            lock (logikLock)
            {
                Console.WriteLine("Garderobe Lichtsteuerung abarbeiten, aktueller Status: {0}", StateMachine.CurrentState);

                if (source == SteuerungLogic.Instance.JemandZuhause)
                {
                    if (SteuerungLogic.Instance.JemandZuhause.Status == false && StateMachine.CurrentState != State.Deaktiviert)
                    {
                        StateMachine.ExecuteAction(Signal.GotoDeaktiviert);
                    }
                    else if (SteuerungLogic.Instance.JemandZuhause.Status == true && StateMachine.CurrentState == State.Deaktiviert)
                    {
                        StateMachine.ExecuteAction(Signal.GotoAus);

                    }
                    LichtsteuerungLogikAllOthers(source);
                }


                Console.WriteLine("Helligkeit überprüfen");
                if (source == GarderobeHelligkeit)
                {
                    if (StateMachine.CurrentState == State.Aus && GarderobeHelligkeit.Helligkeit < GarderobeHelligkeit.Abschaltlevel)
                    {
                        StateMachine.ExecuteAction(Signal.GotoReadyForAction);
                       
                    }
                    else if (GarderobeHelligkeit.Helligkeit > GarderobeHelligkeit.Abschaltlevel)
                    {
                        StateMachine.ExecuteAction(Signal.GotoAus);
                        Console.WriteLine("Licht aus weil es zu Hell wird");
                    }
                }

                if (source == GarderobeBewegung)
                {
                    Console.WriteLine("Bewegungszustand bei Garderobe selbst überprüfen");
                    if (GarderobeBewegung.Status == true && StateMachine.CurrentState == State.ReadyForAction)
                    {
                        StateMachine.ExecuteAction(Signal.GotoAction);                        
                    }
                    else if (GarderobeBewegung.Status == false && StateMachine.CurrentState == State.Action)
                    {
                        //erst nach Ablauf der Restlaufzeit gehen
                        if (GarderobeBewegung.HasRestlaufzeit(GarderobeBewegung.LastChangeTrue) == false)
                        {
                            Console.WriteLine("Licht kann wieder ausgeschaltet werden, keine Restlaufzeit");
                            StateMachine.ExecuteAction(Signal.GotoReadyForAction);
                        }
                        else
                        {
                            Console.WriteLine("Licht kann später ausgeschaltet werden, Restlaufzeit: {0}", GarderobeBewegung.RestlaufzeitMinutes(GarderobeBewegung.LastChangeTrue));
                            Task.Delay(TimeSpan.FromMinutes(GarderobeBewegung.RestlaufzeitMinutes(GarderobeBewegung.LastChangeTrue))).ContinueWith(t => LichtsteuerungLogik(GarderobeBewegung));
                            Console.WriteLine("späteres ausschalten getriggert");
                        }                        
                    }
                }

                if (source == GarageTuer)
                {
                    Console.WriteLine("Tür überprüfen");
                    if (GarageTuer.Status == false && StateMachine.CurrentState == State.ReadyForAction)
                    {
                        StateMachine.ExecuteAction(Signal.GotoAction);

                    }
                    else if (GarageTuer.Status == true && GarderobeBewegung.Status == false && StateMachine.CurrentState == State.Action)
                    {
                        Console.WriteLine("Tür war nur kurz offen");
                        StateMachine.ExecuteAction(Signal.GotoReadyForAction);

                    }
                }

                if (source == HaustuerBewegung)
                {
                    Console.WriteLine("Bewegungszustand bei Haustür selbst überprüfen");
                    if (HaustuerBewegung.Status == true && StateMachine.CurrentState == State.ReadyForAction)
                    {
                        StateMachine.ExecuteAction(Signal.GotoAction);

                    }
                    else if (HaustuerBewegung.Status == false && GarderobeBewegung.Status == false && StateMachine.CurrentState == State.Action)
                    {
                        StateMachine.ExecuteAction(Signal.GotoReadyForAction);
                    }
                }


                Console.WriteLine("Garderobe lichtsteuerung abgearbeitet,  Status: {0}", StateMachine.CurrentState);
            }
        }


      

        private void DoDataChange(object sender, Objekt source)
        {
            Console.WriteLine("DataChange für garderobe: {0}", source);           

            LichtsteuerungLogik(source);



        }


    }
}
