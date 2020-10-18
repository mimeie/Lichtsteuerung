using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

using JusiBase;
//Update-Package

namespace Lichtsteuerung
{
    public sealed class SteuerungLogic
    {
        //für die state machine
        public StateMachineLogic StateMachine;

        private static volatile SteuerungLogic _instance;
        private static object _syncRoot = new object();

        public Multisensor Garderobe;
        public SensorIntToBool GarderobeBewegung;
        public SensorHelligkeit GarderobeHelligkeit;

        public LampeHelligkeit LampeGarderobe;
      
        private SteuerungLogic()
        {

            //StateMachine init
            StateMachine = new StateMachineLogic();
            StateMachine.CurrentState = State.Aus;


            //StateMachine._transitions.Add(new StatesTransition(State.Aus, Signal.GotoWaitForEntfeuchten, GotoStateWaitForEntfeuchten, State.WaitForEntfeuchten));

           
        }

        public static SteuerungLogic Instance
        {
            get
            {
                if (_instance == null)
                {
                    lock (_syncRoot)
                    {
                        if (_instance == null)
                        {
                            _instance = new SteuerungLogic();
                        }
                    }
                }

                return _instance;
            }
        }


        public void Start()
        {
            Console.WriteLine("Steuerung starten");
            GarderobeBewegung = new SensorIntToBool("zwave2.0.Node_030.Basic.currentValue");
            GarderobeHelligkeit = new SensorHelligkeit("zwave2.0.Node_030.Multilevel_Sensor.illuminance");
            Garderobe = new Multisensor();
            Garderobe.Bewegung = GarderobeBewegung;
            Garderobe.Helligkeit = GarderobeHelligkeit;

            LampeGarderobe = new LampeHelligkeit(null, null, "zwave2.0.Node_034.Multilevel_Switch.currentValue", "zwave2.0.Node_034.Multilevel_Switch.targetValue");
            LampeGarderobe.ZielHelligkeit = 55;

            Console.WriteLine("JobManager wurde initialisiert");
            Console.WriteLine("Steuerung gestartet");

        }

        public void Stop()
        {
        }



    }
}
