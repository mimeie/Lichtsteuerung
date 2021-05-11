using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

using JusiBase;

namespace Lichtsteuerung
{
    public class LichtsteuerungAutoAus
    {
        static readonly object logikLock = new object();

        //für die state machine
        public StateMachineLogic StateMachine;

        public SensorBool RaumBewegung;


        public Schalter RaumLicht;

        private string _RaumName;

        public LichtsteuerungAutoAus(string RaumName, string bewegungId, SourceType bewegungSource, string schalterId, double minLaufzeit)
        {
            _RaumName=RaumName; 
            if (SteuerungLogic.Instance.IsDebug == false)
            {
                RaumBewegung = new SensorBool(bewegungId, bewegungSource);
                RaumBewegung.MinLaufzeitMinutes = minLaufzeit;

                RaumLicht = new Schalter(schalterId);
                //LichtSpielRaum.MinLaufzeitMinutes = SpielRaumBewegung.MinLaufzeitMinutes;
            }
            else
            {
                RaumBewegung = new SensorBool("0_userdata.0.DebugLichtsteuerung.Bewegung");
                RaumBewegung.MinLaufzeitMinutes = 0.5;

                RaumLicht = new Schalter("0_userdata.0.DebugLichtsteuerung.LichtSchalter");
                //LichtSpielRaum.MinLaufzeitMinutes = SpielRaumBewegung.MinLaufzeitMinutes;
            }



            //StateMachine init
            StateMachine = new StateMachineLogic();
            StateMachine.CurrentState = State.Aus;

            StateMachine._transitions.Add(new StatesTransition(State.Deaktiviert, Signal.GotoAus, GotoStateAus, State.Aus));

            StateMachine._transitions.Add(new StatesTransition(State.Aus, Signal.GotoDeaktiviert, GotoStateDeaktiviert, State.Deaktiviert));
            //StateMachine._transitions.Add(new StatesTransition(State.Aus, Signal.GotoReadyForAction, GotoStateReadyForAction, State.ReadyForAction));

            //StateMachine._transitions.Add(new StatesTransition(State.ReadyForAction, Signal.GotoAus, GotoStateAus, State.Aus));
            //StateMachine._transitions.Add(new StatesTransition(State.ReadyForAction, Signal.GotoAction, GotoStateAction, State.Action));
            //StateMachine._transitions.Add(new StatesTransition(State.ReadyForAction, Signal.GotoDeaktiviert, GotoStateDeaktiviert, State.Deaktiviert));

            StateMachine._transitions.Add(new StatesTransition(State.Aus, Signal.GotoAction, GotoStateAction, State.Action));

            //StateMachine._transitions.Add(new StatesTransition(State.Action, Signal.GotoReadyForAction, GotoStateReadyForAction, State.ReadyForAction));
            StateMachine._transitions.Add(new StatesTransition(State.Action, Signal.GotoAus, GotoStateAus, State.Aus));
            StateMachine._transitions.Add(new StatesTransition(State.Action, Signal.GotoDeaktiviert, GotoStateDeaktiviert, State.Deaktiviert));
        }

        public void Initialize()
        {

            Console.WriteLine("updates der anlage holen");
            RaumBewegung.Update();
            RaumLicht.Update();

            RaumBewegung.DataChange += DoDataChange;
            RaumLicht.DataChange += DoDataChange;
            SteuerungLogic.Instance.JemandZuhause.DataChange += DoDataChange;

            Console.WriteLine("updates der anlage geholt");

            LichtsteuerungLogikAllOthers(SteuerungLogic.Instance.JemandZuhause);

        }

        private void DoDataChange(object sender, Objekt source)
        {
            Console.WriteLine("DataChange für spielRaum : {0}", source);

            LichtsteuerungLogik(source);



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
            //if (LichtSpielRaum.Status == false)
            //{
            //    LichtSpielRaum.ZielStatus = true;
            //}
        }

        //private void GotoStateReadyForAction()
        //{
        //    Console.WriteLine("Executed: GotoStateReadyForAction");
        //    if (LichtSpielRaum.Status == true)
        //    {
        //        LichtSpielRaum.ZielStatus = false;
        //    }
        //}

        private void LichtsteuerungLogikAllOthers(Objekt source)
        {
            if (source != SteuerungLogic.Instance.JemandZuhause)
            {
                LichtsteuerungLogik(SteuerungLogic.Instance.JemandZuhause);
            }            
            if (source != RaumBewegung)
            {
                LichtsteuerungLogik(RaumBewegung);
            }
            if (source != RaumLicht)
            {
                LichtsteuerungLogik(RaumLicht);
            }

        }

        private void LichtsteuerungLogik(Objekt source)
        {
            lock (logikLock)
            {
                Console.WriteLine("{0} abarbeiten, aktueller Status: {1}, Zuhause: {2}, Bewegung: {3}, Licht Status: {4}", _RaumName, StateMachine.CurrentState, SteuerungLogic.Instance.JemandZuhause.Status,RaumBewegung.Status, RaumLicht.Status);

                string test = this.GetType().Name;
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

                if (source == RaumLicht)
                {
                    Console.WriteLine("Licht überprüfen");
                    if (RaumLicht.Status == true && StateMachine.CurrentState != State.Deaktiviert)
                    {
                        StateMachine.ExecuteAction(Signal.GotoAction);
                        //falls es nie eine bewegung gibt, licht mit maximaler dauer laufen lassen
                        RaumBewegung.LastChangeTrue = DateTime.Now;
                        Console.WriteLine("laufzeit manuell gesetzt beim starten");
                    }
                    else if (RaumLicht.Status == false && StateMachine.CurrentState != State.Deaktiviert)
                    {
                        Console.WriteLine("licht wurde wieder ausgeschaltet");
                        StateMachine.ExecuteAction(Signal.GotoAus);

                    }
                }

                if (source == RaumBewegung)
                {
                    Console.WriteLine("Bewegungszustand überprüfen");
                    if (RaumBewegung.Status == false && StateMachine.CurrentState == State.Action)
                    {
                        //erst nach Ablauf der Restlaufzeit gehen
                        if (RaumBewegung.HasRestlaufzeit(RaumBewegung.LastChangeTrue) == false)
                        {
                            Console.WriteLine("Licht kann wieder ausgeschaltet werden, keine Restlaufzeit");
                            StateMachine.ExecuteAction(Signal.GotoAus);
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
            }
            Console.WriteLine("Raum lichtsteuerung abgearbeitet, Status: {0}", StateMachine.CurrentState);

        }


            }
}
