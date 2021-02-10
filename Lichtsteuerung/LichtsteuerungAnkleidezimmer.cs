using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Diagnostics;


using JusiBase;
//Update-Package

namespace Lichtsteuerung
{
    //https://www.tutorialsteacher.com/csharp/csharp-event

    public class LichtsteuerungAnkleidezimmer
    {

        static readonly object logikLock = new object();

        //für die state machine
        public StateMachineLogic StateMachine;

        //objekte
        //public Bewegungsmelder Ankleide; //theoretisch nicht nötig
        public SensorBool AnkleideBewegung;
        public SensorHelligkeit AnkleideHelligkeit;

        public SensorBool AnkleideTuer;

        public Schalter LichtAnkleide;

        

        public LichtsteuerungAnkleidezimmer()
        {
            AnkleideBewegung = new SensorBool("zigbee.0.00158d00063a6d54.occupancy"); //zigbee.0.00158d00063a6d54.occupancy 0_userdata.0.debugBool
            AnkleideBewegung.MinLaufzeitMinutes = 4;
            AnkleideHelligkeit = new SensorHelligkeit("zigbee.0.00158d00063a6d54.illuminance", 50);

            //Ankleide = new Bewegungsmelder();
            //Ankleide.Bewegung = AnkleideBewegung;
            //Ankleide.Helligkeit = AnkleideHelligkeit;

            AnkleideTuer = new SensorBool("zigbee.0.00158d00025d978b.contact"); //zigbee.0.00158d00025d978b.contact 0_userdata.0.debugBool2

            LichtAnkleide = new Schalter("shelly.0.SHSW-25#D8BFC01A2B2A#1.Relay0.Switch");           
                


            //StateMachine init
            StateMachine = new StateMachineLogic();
            StateMachine.CurrentState = State.Aus;

            StateMachine._transitions.Add(new StatesTransition(State.Deaktiviert, Signal.GotoAus, GotoStateAus, State.Aus));

            StateMachine._transitions.Add(new StatesTransition(State.Aus, Signal.GotoDeaktiviert, GotoStateDeaktiviert,State.Deaktiviert));
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
            AnkleideBewegung.Update();
            AnkleideHelligkeit.Update();
            AnkleideTuer.Update();
            LichtAnkleide.Update();

            AnkleideBewegung.DataChange += DoDataChange;
            AnkleideHelligkeit.DataChange += DoDataChange;
            AnkleideTuer.DataChange += DoDataChange;
            SteuerungLogic.Instance.JemandZuhause.DataChange += DoDataChange;

            Console.WriteLine("updates der anlage geholt");

            

            LichtsteuerungLogik();

        }

        private void GotoStateAus()
        {
            Console.WriteLine("Executed: GotoStateAus");
            if (LichtAnkleide.Status == true)
            {
                LichtAnkleide.ZielStatus = false;
            }

        }

        private void GotoStateDeaktiviert()
        {
            Console.WriteLine("Executed: GotoStateDeaktiviert");
            if (LichtAnkleide.Status == true)
            {
                LichtAnkleide.ZielStatus = false;
            }
                    
        }

        private void GotoStateAction()
        {
            Console.WriteLine("Executed: GotoStateAction");
            if (LichtAnkleide.Status == false)
            {
                LichtAnkleide.ZielStatus = true;
            }
        }

        private void GotoStateReadyForAction()
        {
            Console.WriteLine("Executed: GotoStateReadyForAction");           
            if (LichtAnkleide.Status == true)
            {
                LichtAnkleide.ZielStatus = false;
            }
        }

        //todo: status des lichts wird nicht geprüft fragt
        //bug: helligkeit event kommt sofort nach tür und schliesst wieder weil der bewegungssensor noch nicht da ist.
        private void LichtsteuerungLogik()          
        {
            lock (logikLock)
            {
                Console.WriteLine("Ankleide Lichtsteuerung abarbeiten, aktueller Status: {0}, Zuhause: {1}, Bewegung: {2}, helligkeit: {3}, Tür: {4}, Licht Status: {5}", StateMachine.CurrentState, SteuerungLogic.Instance.JemandZuhause.Status, AnkleideBewegung.Status, AnkleideHelligkeit.Helligkeit, AnkleideTuer.Status, LichtAnkleide.Status);
                if (SteuerungLogic.Instance.JemandZuhause.Status == false && StateMachine.CurrentState != State.Deaktiviert)
                {
                    StateMachine.ExecuteAction(Signal.GotoDeaktiviert);
                    return;
                }
                else if (SteuerungLogic.Instance.JemandZuhause.Status == true && StateMachine.CurrentState == State.Deaktiviert)
                {
                    StateMachine.ExecuteAction(Signal.GotoAus);
                }

                Console.WriteLine("Helligkeit überprüfen");
                if (StateMachine.CurrentState == State.Aus && AnkleideHelligkeit.Helligkeit < AnkleideHelligkeit.Abschaltlevel)
                {
                    StateMachine.ExecuteAction(Signal.GotoReadyForAction);
                }
                else if (AnkleideHelligkeit.Helligkeit > AnkleideHelligkeit.Abschaltlevel)
                {
                    StateMachine.ExecuteAction(Signal.GotoAus);
                    return;
                }



                Console.WriteLine("Bewegungszustand überprüfen");
                if (AnkleideBewegung.Status == true && StateMachine.CurrentState == State.ReadyForAction)
                {
                    StateMachine.ExecuteAction(Signal.GotoAction);
                    return;
                }
                else if (AnkleideBewegung.Status == false && StateMachine.CurrentState == State.Action)
                {
                    //erst nach Ablauf der Restlaufzeit gehen
                    if (AnkleideBewegung.HasRestlaufzeit(AnkleideBewegung.LastChangeTrue) == false)
                    {
                        Console.WriteLine("Licht kann wieder ausgeschaltet werden, keine Restlaufzeit");
                        StateMachine.ExecuteAction(Signal.GotoReadyForAction);
                    }
                    else
                    {
                        Console.WriteLine("Licht kann später ausgeschaltet werden, Restlaufzeit: {0}", AnkleideBewegung.RestlaufzeitMinutes(AnkleideBewegung.LastChangeTrue));
                        Task.Delay(TimeSpan.FromMinutes(AnkleideBewegung.RestlaufzeitMinutes(AnkleideBewegung.LastChangeTrue))).ContinueWith(t => LichtsteuerungLogik());
                        Console.WriteLine("späteres ausschalten getriggert");

                    }
                    return;
                    //https://stackoverflow.com/questions/545533/delayed-function-calls



                }


                Console.WriteLine("Tür überprüfen");
                if (AnkleideTuer.Status == false && StateMachine.CurrentState == State.ReadyForAction)
                {
                    StateMachine.ExecuteAction(Signal.GotoAction);
                    return;
                }
                else if (AnkleideTuer.Status == true && AnkleideBewegung.Status == false && StateMachine.CurrentState == State.Action)
                {
                    Console.WriteLine("Tür war nur kurz offen");
                    StateMachine.ExecuteAction(Signal.GotoReadyForAction);
                    return;
                }

                Console.WriteLine("Ankleide lichtsteuerung abgearbeitet ohne return, Status: {0}", StateMachine.CurrentState);
            }
        }



        private void DoDataChange(object sender, Objekt source)
        {
            Console.WriteLine("DataChange für ankleidezimmer : {0}", source);

            LichtsteuerungLogik();

           

        }


    }
}
