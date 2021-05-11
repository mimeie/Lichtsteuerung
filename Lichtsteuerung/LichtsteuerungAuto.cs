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

    public class LichtsteuerungAuto
    {

        static readonly object logikLock = new object();

        //für die state machine
        public StateMachineLogic StateMachine;

        //objekte
        //public Bewegungsmelder Raum; //theoretisch nicht nötig
        public SensorBool RaumBewegung;
        public SensorHelligkeit RaumHelligkeit;

        public SensorBool RaumTuer;

        public Schalter RaumLicht;

        private string _RaumName;

        public LichtsteuerungAuto(string raumName, string bewegungId, string schalterId, string helliigkeitId, string tuerId, int helligkeitAbschaltlevel, double minLaufzeit)
        {
            _RaumName = raumName;
            if (SteuerungLogic.Instance.IsDebug == false)
            {
                RaumBewegung = new SensorBool(bewegungId);
                RaumBewegung.MinLaufzeitMinutes = minLaufzeit;
                RaumHelligkeit = new SensorHelligkeit(helliigkeitId, helligkeitAbschaltlevel);

                RaumTuer = new SensorBool(tuerId);

                RaumLicht = new Schalter(schalterId);
            }
            else
            {
                RaumBewegung = new SensorBool("0_userdata.0.DebugLichtsteuerung.Bewegung");
                RaumBewegung.MinLaufzeitMinutes = 4;
                RaumHelligkeit = new SensorHelligkeit("0_userdata.0.DebugLichtsteuerung.Helligkeit", 50);

                RaumTuer = new SensorBool("0_userdata.0.DebugLichtsteuerung.TuerKontakt");

                RaumLicht = new Schalter("0_userdata.0.DebugLichtsteuerung.LichtSchalter");
            }
          

            //Raum = new Bewegungsmelder();
            //Raum.Bewegung = RaumBewegung;
            //Raum.Helligkeit = RaumHelligkeit;


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
            RaumBewegung.Update();
            RaumHelligkeit.Update();
            RaumTuer.Update();
            RaumLicht.Update();

            RaumBewegung.DataChange += DoDataChange;
            RaumHelligkeit.DataChange += DoDataChange;
            RaumTuer.DataChange += DoDataChange;
            RaumLicht.DataChange += DoDataChange;
            SteuerungLogic.Instance.JemandZuhause.DataChange += DoDataChange;

            Console.WriteLine("updates der anlage geholt");

            LichtsteuerungLogikAllOthers(SteuerungLogic.Instance.JemandZuhause);

        }

        private void GotoStateAus()
        {
            Console.WriteLine("Executed: GotoStateAus");
            if (RaumLicht.Status == true)
            {
                RaumLicht.ZielStatus = false;
            }

        }

        private void GotoStateDeaktiviert()
        {
            Console.WriteLine("Executed: GotoStateDeaktiviert");
            if (RaumLicht.Status == true)
            {
                RaumLicht.ZielStatus = false;
            }
                    
        }

        private void GotoStateAction()
        {
            Console.WriteLine("Executed: GotoStateAction");
            if (RaumLicht.Status == false)
            {
                RaumLicht.ZielStatus = true;
            }
        }

        private void GotoStateReadyForAction()
        {
            Console.WriteLine("Executed: GotoStateReadyForAction");           
            if (RaumLicht.Status == true)
            {
                RaumLicht.ZielStatus = false;
            }
        }

        private void LichtsteuerungLogikAllOthers(Objekt source)
        {
            if (source != SteuerungLogic.Instance.JemandZuhause)
            { 
                LichtsteuerungLogik(SteuerungLogic.Instance.JemandZuhause);
            }
            if (source != RaumHelligkeit)
            {
                LichtsteuerungLogik(RaumHelligkeit);
            }
            if (source != RaumBewegung)
            {
                LichtsteuerungLogik(RaumBewegung);
            }
            if (source != RaumTuer)
            {
                LichtsteuerungLogik(RaumTuer);
            }
                                   
        }

        
        private void LichtsteuerungLogik(Objekt source)          
        {
            lock (logikLock)
            {                
                Console.WriteLine("{0} Lichtsteuerung abarbeiten, aktueller Status: {1}, Zuhause: {2}, Bewegung: {3}, helligkeit: {4}, Tür: {5}, Licht Status: {6}, source-id: {7}", _RaumName, StateMachine.CurrentState, SteuerungLogic.Instance.JemandZuhause.Status, RaumBewegung.Status, RaumHelligkeit.Helligkeit, RaumTuer.Status, RaumLicht.Status, source.ObjektId);

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


                if (source == RaumHelligkeit)
                {
                    Console.WriteLine("Helligkeit überprüfen");
                    if (StateMachine.CurrentState == State.Aus && RaumHelligkeit.Helligkeit < RaumHelligkeit.Abschaltlevel)
                    {
                        StateMachine.ExecuteAction(Signal.GotoReadyForAction);
                    }
                    else if (RaumHelligkeit.Helligkeit > RaumHelligkeit.Abschaltlevel)
                    {
                        StateMachine.ExecuteAction(Signal.GotoAus);
                        
                        Console.WriteLine("Licht aus weil es zu Hell wird");
                    }
                }


                if (source == RaumBewegung)
                {
                    Console.WriteLine("Bewegungszustand überprüfen");
                    if (RaumBewegung.Status == true && StateMachine.CurrentState == State.ReadyForAction)
                    {
                        StateMachine.ExecuteAction(Signal.GotoAction);                       
                    }
                    else if (RaumBewegung.Status == false && StateMachine.CurrentState == State.Action)
                    {
                        //erst nach Ablauf der Restlaufzeit gehen
                        if (RaumBewegung.HasRestlaufzeit(RaumBewegung.LastChangeTrue) == false)
                        {
                            Console.WriteLine("Licht kann wieder ausgeschaltet werden, keine Restlaufzeit");
                            StateMachine.ExecuteAction(Signal.GotoReadyForAction);
                        }
                        else
                        {
                            //https://stackoverflow.com/questions/545533/delayed-function-calls
                            Console.WriteLine("Licht kann später ausgeschaltet werden, Restlaufzeit: {0}", RaumBewegung.RestlaufzeitMinutes(RaumBewegung.LastChangeTrue));
                            Task.Delay(TimeSpan.FromMinutes(RaumBewegung.RestlaufzeitMinutes(RaumBewegung.LastChangeTrue))).ContinueWith(t => LichtsteuerungLogik(RaumBewegung));
                            Console.WriteLine("späteres ausschalten getriggert");

                        }                        
                        
                    }
                }

                if (source == RaumTuer)
                {
                    Console.WriteLine("Tür überprüfen");
                    if (RaumTuer.Status == false && StateMachine.CurrentState == State.ReadyForAction)
                    {
                        StateMachine.ExecuteAction(Signal.GotoAction);
                        
                    }
                    else if (RaumTuer.Status == true && RaumBewegung.Status == false && StateMachine.CurrentState == State.Action)
                    {
                        Console.WriteLine("Tür war nur kurz offen");
                        StateMachine.ExecuteAction(Signal.GotoReadyForAction);
                       
                    }
                }

                Console.WriteLine("Raum lichtsteuerung abgearbeitet, Status: {0}", StateMachine.CurrentState);
            }
        }



        private void DoDataChange(object sender, Objekt source)
        {
            Console.WriteLine("DataChange für Raumzimmer : {0}", source);

            LichtsteuerungLogik(source);

           

        }


    }
}
