using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Diagnostics;

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

        public Multisensor Ankleide;
        public SensorIntToBool AnkleideBewegung;
        public SensorHelligkeit AnkleideHelligkeit;

        public Schalter LichtAnkleide;

        public SensorBool JemandZuhause;
      
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
            AnkleideBewegung = new SensorIntToBool("zwave2.0.Node_006.Basic.currentValue");
            AnkleideHelligkeit = new SensorHelligkeit("zwave2.0.Node_006.Multilevel_Sensor.illuminance");

            Ankleide = new Multisensor();
            Ankleide.Bewegung = AnkleideBewegung;
            Ankleide.Helligkeit = AnkleideHelligkeit;

            JemandZuhause = new SensorBool("0_userdata.0.IsAnybodyHome");

            LichtAnkleide = new Schalter("shelly.0.SHSW-25#D8BFC01A2B2A#1.Relay0.Switch");
         

            Console.WriteLine("JobManager wurde initialisiert");
            Console.WriteLine("Steuerung gestartet");

            //zum testen           
            //Update();
        }


        //parallel: https://stackoverrun.com/de/q/11724611 oder https://docs.microsoft.com/de-de/dotnet/standard/parallel-programming/how-to-use-parallel-invoke-to-execute-parallel-operations
        public void Update()
        {
            Stopwatch sw = new Stopwatch();
            sw.Start();
            //Console.WriteLine("update gestartet");

            Console.WriteLine("updates der anlage holen");

            #region ParallelTasks
            // Perform three tasks in parallel on the source array
            Parallel.Invoke(() =>
                    {
                        AnkleideBewegung.Update();
                        Console.WriteLine("AnkleideBewegung update fertig ausgeführt, dauer: {0}, wert ist: {1}", sw.ElapsedMilliseconds, AnkleideBewegung.Status);
                    },  // close first Action

                             () =>
                             {
                                 AnkleideHelligkeit.Update();
                                 Console.WriteLine("AnkleideHelligkeit update fertig ausgeführt, dauer: {0}, helligkeit ist: {1}", sw.ElapsedMilliseconds, AnkleideHelligkeit.Helligkeit);
                             }, //close second Action

                             () =>
                             {
                                 LichtAnkleide.Update();
                                 Console.WriteLine("LichtAnkleide update fertig ausgeführt, dauer: {0}, wert ist: {1}", sw.ElapsedMilliseconds, LichtAnkleide.Status);
                             }, //close third Action


                             () =>
                             {
                JemandZuhause.Update();
                Console.WriteLine("JemandZuhause update fertig ausgeführt, dauer: {0}, wert ist: {1}", sw.ElapsedMilliseconds, JemandZuhause.Status);
            } //close third Action
                         ); //close parallel.invoke

            Console.WriteLine("Returned from Parallel.Invoke, dauer {0}", sw.ElapsedMilliseconds);
            #endregion

            //bevor es in die state machine geht mal alles hier drin machen
            
            
            

            

           

            //string temp = sw.ElapsedMilliseconds.ToString();

            //https://www.nuget.org/packages/CoordinateSharp/
            if (AnkleideBewegung.Status == true && AnkleideHelligkeit.Helligkeit < 15 && JemandZuhause.Status == true)
            {
                if (LichtAnkleide.Status == false)
                {
                    LichtAnkleide.ZielStatus = true;
                    Console.WriteLine("LichtAnkleide ZielStatus an fertig ausgeführt, dauer: {0}", sw.ElapsedMilliseconds);
                }
                else
                {
                    Console.WriteLine("LichtAnkleide ist schon an");
                }

            }
            else
            {
                if (LichtAnkleide.Status == true)
                {
                    LichtAnkleide.ZielStatus = false;
                    Console.WriteLine("LichtAnkleide ZielStatus aus fertig ausgeführt, dauer: {0}", sw.ElapsedMilliseconds);
                }
                else
                {
                    Console.WriteLine("LichtAnkleide ist schon aus");
                }
            }


            Console.WriteLine("Gesamtupdate fertig ausgeführt, dauer: {0}", sw.ElapsedMilliseconds);
            sw.Stop();
        }

        public void Stop()
        {
        }



    }
}
