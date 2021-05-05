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

        public SensorBool ZimmerBewegung; //ist ein inttobool, dass die aotec sensoren gleich auch mitgenutzt werden können

        public Schalter ZimmerLicht;

        private string _zimmerName;

        public LichtsteuerungAutoAus(string zimmerName, string bewegungId, string schalterId, double minLaufzeit)
        {
            _zimmerName=zimmerName; 
            if (SteuerungLogic.Instance.IsDebug == false)
            {
                ZimmerBewegung = new SensorBool(bewegungId);
                ZimmerBewegung.MinLaufzeitMinutes = minLaufzeit;

                ZimmerLicht = new Schalter(schalterId);
                //LichtSpielzimmer.MinLaufzeitMinutes = SpielzimmerBewegung.MinLaufzeitMinutes;
            }
            else
            {
                ZimmerBewegung = new SensorBool("0_userdata.0.DebugLichtsteuerung.Bewegung");
                ZimmerBewegung.MinLaufzeitMinutes = 0.5;

                ZimmerLicht = new Schalter("0_userdata.0.DebugLichtsteuerung.LichtSchalter");
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
            ZimmerBewegung.Update();
            ZimmerLicht.Update();

            ZimmerBewegung.DataChange += DoDataChange;
            ZimmerLicht.DataChange += DoDataChange;
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
            if (ZimmerLicht.Status == true)
            {
                ZimmerLicht.ZielStatus = false;
            }

        }

        private void GotoStateDeaktiviert()
        {
            Console.WriteLine("Executed: GotoStateDeaktiviert");
            if (ZimmerLicht.Status == true)
            {
                ZimmerLicht.ZielStatus = false;
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
            if (source != ZimmerBewegung)
            {
                LichtsteuerungLogik(ZimmerBewegung);
            }
            if (source != ZimmerLicht)
            {
                LichtsteuerungLogik(ZimmerLicht);
            }

        }

        private void LichtsteuerungLogik(Objekt source)
        {
            lock (logikLock)
            {
                Console.WriteLine("{0} abarbeiten, aktueller Status: {1}, Zuhause: {2}, Bewegung: {3}, Licht Status: {4}", _zimmerName, StateMachine.CurrentState, SteuerungLogic.Instance.JemandZuhause.Status,ZimmerBewegung.Status, ZimmerLicht.Status);

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

                if (source == ZimmerLicht)
                {
                    Console.WriteLine("Licht überprüfen");
                    if (ZimmerLicht.Status == true && StateMachine.CurrentState != State.Deaktiviert)
                    {
                        StateMachine.ExecuteAction(Signal.GotoAction);
                        //falls es nie eine bewegung gibt, licht mit maximaler dauer laufen lassen
                        ZimmerBewegung.LastChangeTrue = DateTime.Now;
                        Console.WriteLine("laufzeit manuell gesetzt beim starten");
                    }
                    else if (ZimmerLicht.Status == false && StateMachine.CurrentState != State.Deaktiviert)
                    {
                        Console.WriteLine("licht wurde wieder ausgeschaltet");
                        StateMachine.ExecuteAction(Signal.GotoAus);

                    }
                }

                if (source == ZimmerBewegung)
                {
                    Console.WriteLine("Bewegungszustand überprüfen");
                    if (ZimmerBewegung.Status == false && StateMachine.CurrentState == State.Action)
                    {
                        //erst nach Ablauf der Restlaufzeit gehen
                        if (ZimmerBewegung.HasRestlaufzeit(ZimmerBewegung.LastChangeTrue) == false)
                        {
                            Console.WriteLine("Licht kann wieder ausgeschaltet werden, keine Restlaufzeit");
                            StateMachine.ExecuteAction(Signal.GotoAus);
                        }
                        else
                        {
                            //https://stackoverflow.com/questions/545533/delayed-function-calls
                            Console.WriteLine("Licht kann später ausgeschaltet werden, Restlaufzeit: {0}", ZimmerBewegung.RestlaufzeitMinutes(ZimmerBewegung.LastChangeTrue));
                            Task.Delay(TimeSpan.FromMinutes(ZimmerBewegung.RestlaufzeitMinutes(ZimmerBewegung.LastChangeTrue))).ContinueWith(t => LichtsteuerungLogik(ZimmerBewegung));
                            Console.WriteLine("späteres ausschalten getriggert");

                        }

                    }
                }
            }
            Console.WriteLine("Zimmer lichtsteuerung abgearbeitet, Status: {0}", StateMachine.CurrentState);

        }


            }
}
