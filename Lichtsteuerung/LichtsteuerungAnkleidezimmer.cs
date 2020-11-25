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
        public event EventHandler<string> DataChange;

        //für die state machine
        public StateMachineLogic StateMachine;

        //objekte
        public Multisensor Ankleide;
        public SensorIntToBool AnkleideBewegung;
        public SensorHelligkeit AnkleideHelligkeit;

        public Schalter LichtAnkleide;

        public SensorBool JemandZuhause;

        public LichtsteuerungAnkleidezimmer()
        {
            AnkleideBewegung = new SensorIntToBool("zwave2.0.Node_006.Basic.currentValue");
            AnkleideHelligkeit = new SensorHelligkeit("zwave2.0.Node_006.Multilevel_Sensor.illuminance",50);

            Ankleide = new Multisensor();
            Ankleide.Bewegung = AnkleideBewegung;
            Ankleide.Helligkeit = AnkleideHelligkeit;

            JemandZuhause = new SensorBool("0_userdata.0.IsAnybodyHome");

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



            DataChange += DoDataChange;
        }

        public void Initialize()
        {

            Console.WriteLine("updates der anlage holen");
            AnkleideBewegung.Update();
            AnkleideHelligkeit.Update();
            LichtAnkleide.Update();
            JemandZuhause.Update();
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
            Console.WriteLine("lichtsteuerung abarbeiten, aktueller Status: {0}", StateMachine.CurrentState);
            if (JemandZuhause.Status == false && StateMachine.CurrentState != State.Deaktiviert)
            {
                StateMachine.ExecuteAction(Signal.GotoDeaktiviert);
                return;
            }
            else if  (JemandZuhause.Status == true && StateMachine.CurrentState == State.Deaktiviert)
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
            Console.WriteLine("lichtsteuerung abgearbeitet, neuer Status: {0}", StateMachine.CurrentState);
        }


        public void RaiseDataChange(string source)
        //protected virtual void OnProcessCompleted(SensorBool sensorBool)
        {
            DataChange?.Invoke(this, source);
        }

        private void DoDataChange(object sender, string source)
        {
            Console.WriteLine("DataChange: {0}", source);

            switch (source)
            {
                case nameof(JemandZuhause):
                    JemandZuhause.Update();               
                    break;                
                case nameof(AnkleideHelligkeit):
                    AnkleideHelligkeit.Update();
                    break;
                case nameof(AnkleideBewegung):
                    AnkleideBewegung.Update();           
                    break;
                //case nameof(LichtAnkleide):
                //    LichtAnkleide.Update();
                //    break;
            }

            LichtsteuerungLogik();

           

        }


    }
}
