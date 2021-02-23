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

            if (SteuerungLogic.Instance.IsDebug == false)
            {
                AnkleideBewegung = new SensorBool("zigbee.0.00158d00063a6d54.occupancy");
                AnkleideBewegung.MinLaufzeitMinutes = 4;
                AnkleideHelligkeit = new SensorHelligkeit("zigbee.0.00158d00063a6d54.illuminance", 50);

                AnkleideTuer = new SensorBool("zigbee.0.00158d00025d978b.contact");

                LichtAnkleide = new Schalter("shelly.0.SHSW-25#D8BFC01A2B2A#1.Relay0.Switch");
            }
            else
            {
                AnkleideBewegung = new SensorBool("0_userdata.0.DebugLichtsteuerung.Bewegung");
                AnkleideBewegung.MinLaufzeitMinutes = 4;
                AnkleideHelligkeit = new SensorHelligkeit("0_userdata.0.DebugLichtsteuerung.Helligkeit", 50);

                AnkleideTuer = new SensorBool("0_userdata.0.DebugLichtsteuerung.TuerKontakt");

                LichtAnkleide = new Schalter("0_userdata.0.DebugLichtsteuerung.LichtSchalter");
            }
          

            //Ankleide = new Bewegungsmelder();
            //Ankleide.Bewegung = AnkleideBewegung;
            //Ankleide.Helligkeit = AnkleideHelligkeit;


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
            LichtAnkleide.DataChange += DoDataChange;
            SteuerungLogic.Instance.JemandZuhause.DataChange += DoDataChange;

            Console.WriteLine("updates der anlage geholt");

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

        private void LichtsteuerungLogikAllOthers(Objekt source)
        {
            if (source != SteuerungLogic.Instance.JemandZuhause)
            { 
                LichtsteuerungLogik(SteuerungLogic.Instance.JemandZuhause);
            }
            if (source != AnkleideHelligkeit)
            {
                LichtsteuerungLogik(AnkleideHelligkeit);
            }
            if (source != AnkleideBewegung)
            {
                LichtsteuerungLogik(AnkleideBewegung);
            }
            if (source != AnkleideTuer)
            {
                LichtsteuerungLogik(AnkleideTuer);
            }
                                   
        }

        //todo: status des lichts wird nicht geprüft fragt
        //bug: helligkeit event kommt sofort nach tür und schliesst wieder weil der bewegungssensor noch nicht da ist.
        private void LichtsteuerungLogik(Objekt source)          
        {
            lock (logikLock)
            {                
                Console.WriteLine("Ankleide Lichtsteuerung abarbeiten, aktueller Status: {0}, Zuhause: {1}, Bewegung: {2}, helligkeit: {3}, Tür: {4}, Licht Status: {5}, source-id: {6}", StateMachine.CurrentState, SteuerungLogic.Instance.JemandZuhause.Status, AnkleideBewegung.Status, AnkleideHelligkeit.Helligkeit, AnkleideTuer.Status, LichtAnkleide.Status, source.ObjektId);

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


                if (source == AnkleideHelligkeit)
                {
                    Console.WriteLine("Helligkeit überprüfen");
                    if (StateMachine.CurrentState == State.Aus && AnkleideHelligkeit.Helligkeit < AnkleideHelligkeit.Abschaltlevel)
                    {
                        StateMachine.ExecuteAction(Signal.GotoReadyForAction);
                    }
                    else if (AnkleideHelligkeit.Helligkeit > AnkleideHelligkeit.Abschaltlevel)
                    {
                        StateMachine.ExecuteAction(Signal.GotoAus);
                        
                        Console.WriteLine("Licht aus weil es zu Hell wird");
                    }
                }


                if (source == AnkleideBewegung)
                {
                    Console.WriteLine("Bewegungszustand überprüfen");
                    if (AnkleideBewegung.Status == true && StateMachine.CurrentState == State.ReadyForAction)
                    {
                        StateMachine.ExecuteAction(Signal.GotoAction);                       
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
                            Task.Delay(TimeSpan.FromMinutes(AnkleideBewegung.RestlaufzeitMinutes(AnkleideBewegung.LastChangeTrue))).ContinueWith(t => LichtsteuerungLogik(AnkleideBewegung));
                            Console.WriteLine("späteres ausschalten getriggert");

                        }                        
                        //https://stackoverflow.com/questions/545533/delayed-function-calls
                    }
                }

                if (source == AnkleideTuer)
                {
                    Console.WriteLine("Tür überprüfen");
                    if (AnkleideTuer.Status == false && StateMachine.CurrentState == State.ReadyForAction)
                    {
                        StateMachine.ExecuteAction(Signal.GotoAction);
                        
                    }
                    else if (AnkleideTuer.Status == true && AnkleideBewegung.Status == false && StateMachine.CurrentState == State.Action)
                    {
                        Console.WriteLine("Tür war nur kurz offen");
                        StateMachine.ExecuteAction(Signal.GotoReadyForAction);
                       
                    }
                }

                Console.WriteLine("Ankleide lichtsteuerung abgearbeitet, Status: {0}", StateMachine.CurrentState);
            }
        }



        private void DoDataChange(object sender, Objekt source)
        {
            Console.WriteLine("DataChange für ankleidezimmer : {0}", source);

            LichtsteuerungLogik(source);

           

        }


    }
}
