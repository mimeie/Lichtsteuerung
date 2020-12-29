using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Diagnostics;


using JusiBase;

namespace Lichtsteuerung
{
    //https://www.tutorialsteacher.com/csharp/csharp-event

    public class LichtsteuerungAnkleidezimmer
    {
      

        //für die state machine
        public StateMachineLogic StateMachine;

        //objekte
        public Bewegungsmelder Ankleide; //theoretisch nicht nötig
        public SensorBool AnkleideBewegung;
        public SensorHelligkeit AnkleideHelligkeit;

        public Schalter LichtAnkleide;

        

        public LichtsteuerungAnkleidezimmer()
        {
            AnkleideBewegung = new SensorBool("zigbee.0.00158d00063a6d54.occupancy");
            AnkleideHelligkeit = new SensorHelligkeit("zigbee.0.00158d00063a6d54.illuminance", 50);

            Ankleide = new Bewegungsmelder();
            Ankleide.Bewegung = AnkleideBewegung;
            Ankleide.Helligkeit = AnkleideHelligkeit;

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
            LichtAnkleide.Update();

            AnkleideBewegung.DataChange += DoDataChange;
            AnkleideBewegung.DataChange += DoDataChange;
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

        private void LichtsteuerungLogik()
        {
            Console.WriteLine("Ankleide lichtsteuerung abarbeiten, aktueller Status: {0}, Zuhause: {1}, Bewegung: {2}, helligkeit: {3}", StateMachine.CurrentState,SteuerungLogic.Instance.JemandZuhause.Status, AnkleideBewegung.Status, AnkleideHelligkeit.Helligkeit);
            if (SteuerungLogic.Instance.JemandZuhause.Status == false && StateMachine.CurrentState != State.Deaktiviert)
            {
                StateMachine.ExecuteAction(Signal.GotoDeaktiviert);
                return;
            }
            else if  (SteuerungLogic.Instance.JemandZuhause.Status == true && StateMachine.CurrentState == State.Deaktiviert)
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
            }

            Console.WriteLine("Bewegungszustand überprüfen");
            if (AnkleideBewegung.Status == true && StateMachine.CurrentState == State.ReadyForAction)
            {
                StateMachine.ExecuteAction(Signal.GotoAction);
            }
            else if (AnkleideBewegung.Status == false && StateMachine.CurrentState == State.Action)
            {
                StateMachine.ExecuteAction(Signal.GotoReadyForAction);
            }            
            Console.WriteLine("Ankleide lichtsteuerung abgearbeitet, neuer Status: {0}", StateMachine.CurrentState);
        }



        private void DoDataChange(object sender, Objekt source)
        {
            Console.WriteLine("DataChange für ankleidezimmer : {0}", source);

            LichtsteuerungLogik();

           

        }


    }
}
