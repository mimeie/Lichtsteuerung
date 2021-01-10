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
        public SensorIntToBool GarderobeBewegung;
        public SensorHelligkeit GarderobeHelligkeit;

        public SensorIntToBool HaustuerBewegung;

        public LampeHelligkeit LichtGarderobe;

      

        public LichtsteuerungGarderobe()
        {
            GarderobeBewegung = new SensorIntToBool("zwave2.0.Node_030.Basic.currentValue");
            GarderobeBewegung.MinLaufzeitMinutes = 5;
            GarderobeHelligkeit = new SensorHelligkeit("zwave2.0.Node_030.Multilevel_Sensor.illuminance", 50);
            HaustuerBewegung = new SensorIntToBool("zwave2.0.Node_023.Basic.currentValue");

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

            GarderobeBewegung.DataChange += DoDataChange;
            GarderobeHelligkeit.DataChange += DoDataChange;
            HaustuerBewegung.DataChange += DoDataChange;
            SteuerungLogic.Instance.JemandZuhause.DataChange += DoDataChange;

            Console.WriteLine("updates der anlage geholt");

            LichtsteuerungLogik();

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

        private void LichtsteuerungLogik()
        {
            lock (logikLock)
            {
                Console.WriteLine("Garderobe Lichtsteuerung abarbeiten, aktueller Status: {0}", StateMachine.CurrentState);
                if (SteuerungLogic.Instance.JemandZuhause.Status == false && StateMachine.CurrentState != State.Deaktiviert)
                {
                    StateMachine.ExecuteAction(Signal.GotoDeaktiviert);
                    return;
                }
                else if (SteuerungLogic.Instance.JemandZuhause.Status == true && StateMachine.CurrentState == State.Deaktiviert)
                {
                    StateMachine.ExecuteAction(Signal.GotoAus);
                    return;
                }

                Console.WriteLine("Helligkeit überprüfen");
                if (StateMachine.CurrentState == State.Aus && GarderobeHelligkeit.Helligkeit < GarderobeHelligkeit.Abschaltlevel)
                {
                    StateMachine.ExecuteAction(Signal.GotoReadyForAction);
                    return;
                }
                else if (GarderobeHelligkeit.Helligkeit > GarderobeHelligkeit.Abschaltlevel)
                {
                    StateMachine.ExecuteAction(Signal.GotoAus);
                    return;
                }

                Console.WriteLine("Bewegungszustand bei Garderobe selbst überprüfen");
                if (GarderobeBewegung.Status == true && StateMachine.CurrentState == State.ReadyForAction)
                {
                    StateMachine.ExecuteAction(Signal.GotoAction);
                    return;
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
                        Task.Delay(TimeSpan.FromMinutes(GarderobeBewegung.RestlaufzeitMinutes(GarderobeBewegung.LastChangeTrue))).ContinueWith(t => LichtsteuerungLogik());
                        Console.WriteLine("späteres ausschalten getriggert");
                    }                   
                    return;
                }

                Console.WriteLine("Bewegungszustand bei Haustür selbst überprüfen");
                if (HaustuerBewegung.Status == true && StateMachine.CurrentState == State.ReadyForAction)
                {
                    StateMachine.ExecuteAction(Signal.GotoAction);
                    return;
                }
                else if (HaustuerBewegung.Status == false && GarderobeBewegung.Status == false && StateMachine.CurrentState == State.Action)
                {
                    StateMachine.ExecuteAction(Signal.GotoReadyForAction);
                    return;
                }


                Console.WriteLine("Garderobe lichtsteuerung abgearbeitet ohne return,  Status: {0}", StateMachine.CurrentState);
            }
        }


      

        private void DoDataChange(object sender, Objekt source)
        {
            Console.WriteLine("DataChange für garderobe: {0}", source);           

            LichtsteuerungLogik();



        }


    }
}
