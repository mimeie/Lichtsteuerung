using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

using JusiBase;

namespace Lichtsteuerung
{
    public class LichtsteuerungSpielzimmer
    {
        static readonly object logikLock = new object();

        //für die state machine
        public StateMachineLogic StateMachine;

        public SensorBool SpielzimmerBewegung;

        public Schalter LichtSpielzimmer;

        public LichtsteuerungSpielzimmer()
        {
            if (SteuerungLogic.Instance.IsDebug == false)
            {
                SpielzimmerBewegung = new SensorBool("zigbee.0.00158d0004abd3aa.occupancy");
                SpielzimmerBewegung.MinLaufzeitMinutes = 8;

                LichtSpielzimmer = new Schalter("shelly.0.SHSW-25#D8BFC01A2B2A#1.Relay1.Switch");
                //LichtSpielzimmer.MinLaufzeitMinutes = SpielzimmerBewegung.MinLaufzeitMinutes;
            }
            else
            {
                SpielzimmerBewegung = new SensorBool("0_userdata.0.DebugLichtsteuerung.Bewegung");
                SpielzimmerBewegung.MinLaufzeitMinutes = 0.5;

                LichtSpielzimmer = new Schalter("0_userdata.0.DebugLichtsteuerung.LichtSchalter");
                //LichtSpielzimmer.MinLaufzeitMinutes = SpielzimmerBewegung.MinLaufzeitMinutes;
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
            SpielzimmerBewegung.Update();         
            LichtSpielzimmer.Update();

            SpielzimmerBewegung.DataChange += DoDataChange;
            LichtSpielzimmer.DataChange += DoDataChange;
            SteuerungLogic.Instance.JemandZuhause.DataChange += DoDataChange;

            Console.WriteLine("updates der anlage geholt");

            LichtsteuerungLogikAllOthers(SteuerungLogic.Instance.JemandZuhause);

        }

        private void DoDataChange(object sender, Objekt source)
        {
            Console.WriteLine("DataChange für spielzimmer : {0}", source);

            LichtsteuerungLogik(source);



        }

        private void GotoStateAus()
        {
            Console.WriteLine("Executed: GotoStateAus");
            if (LichtSpielzimmer.Status == true)
            {
                LichtSpielzimmer.ZielStatus = false;
            }

        }

        private void GotoStateDeaktiviert()
        {
            Console.WriteLine("Executed: GotoStateDeaktiviert");
            if (LichtSpielzimmer.Status == true)
            {
                LichtSpielzimmer.ZielStatus = false;
            }

        }

        private void GotoStateAction()
        {
            Console.WriteLine("Executed: GotoStateAction");
            //if (LichtSpielzimmer.Status == false)
            //{
            //    LichtSpielzimmer.ZielStatus = true;
            //}
        }

        //private void GotoStateReadyForAction()
        //{
        //    Console.WriteLine("Executed: GotoStateReadyForAction");
        //    if (LichtSpielzimmer.Status == true)
        //    {
        //        LichtSpielzimmer.ZielStatus = false;
        //    }
        //}

        private void LichtsteuerungLogikAllOthers(Objekt source)
        {
            if (source != SteuerungLogic.Instance.JemandZuhause)
            {
                LichtsteuerungLogik(SteuerungLogic.Instance.JemandZuhause);
            }            
            if (source != SpielzimmerBewegung)
            {
                LichtsteuerungLogik(SpielzimmerBewegung);
            }
            if (source != LichtSpielzimmer)
            {
                LichtsteuerungLogik(LichtSpielzimmer);
            }

        }

        private void LichtsteuerungLogik(Objekt source)
        {
            lock (logikLock)
            {
                Console.WriteLine("Spielzimmer Lichtsteuerung abarbeiten, aktueller Status: {0}, Zuhause: {1}, Bewegung: {2}, Licht Status: {3}", StateMachine.CurrentState, SteuerungLogic.Instance.JemandZuhause.Status, SpielzimmerBewegung.Status, LichtSpielzimmer.Status);

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

                if (source == LichtSpielzimmer)
                {
                    Console.WriteLine("Licht überprüfen");
                    if (LichtSpielzimmer.Status == true && StateMachine.CurrentState != State.Deaktiviert)
                    {
                        StateMachine.ExecuteAction(Signal.GotoAction);
                        //falls es nie eine bewegung gibt, licht mit maximaler dauer laufen lassen
                        SpielzimmerBewegung.LastChangeTrue = DateTime.Now;
                        Console.WriteLine("laufzeit manuell gesetzt beim starten");
                    }
                    else if (LichtSpielzimmer.Status == false && StateMachine.CurrentState != State.Deaktiviert)
                    {
                        Console.WriteLine("licht wurde wieder ausgeschaltet");
                        StateMachine.ExecuteAction(Signal.GotoAus);

                    }
                }

                if (source == SpielzimmerBewegung)
                {
                    Console.WriteLine("Bewegungszustand überprüfen");
                    if (SpielzimmerBewegung.Status == false && StateMachine.CurrentState == State.Action)
                    {
                        //erst nach Ablauf der Restlaufzeit gehen
                        if (SpielzimmerBewegung.HasRestlaufzeit(SpielzimmerBewegung.LastChangeTrue) == false)
                        {
                            Console.WriteLine("Licht kann wieder ausgeschaltet werden, keine Restlaufzeit");
                            StateMachine.ExecuteAction(Signal.GotoAus);
                        }
                        else
                        {
                            //https://stackoverflow.com/questions/545533/delayed-function-calls
                            Console.WriteLine("Licht kann später ausgeschaltet werden, Restlaufzeit: {0}", SpielzimmerBewegung.RestlaufzeitMinutes(SpielzimmerBewegung.LastChangeTrue));
                            Task.Delay(TimeSpan.FromMinutes(SpielzimmerBewegung.RestlaufzeitMinutes(SpielzimmerBewegung.LastChangeTrue))).ContinueWith(t => LichtsteuerungLogik(SpielzimmerBewegung));
                            Console.WriteLine("späteres ausschalten getriggert");

                        }

                    }
                }
            }
            Console.WriteLine("Spielzimmer lichtsteuerung abgearbeitet, Status: {0}", StateMachine.CurrentState);

        }


            }
}
