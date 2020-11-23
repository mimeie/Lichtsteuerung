using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

using JusiBase;

namespace Lichtsteuerung
{
    //https://www.tutorialsteacher.com/csharp/csharp-event

    public class LichtsteuerungAnkleidezimmer
    {
        public event EventHandler<SensorBool> ProcessCompleted;

        public SensorBool JemandZuhause;

        public LichtsteuerungAnkleidezimmer()
        {
            JemandZuhause = new SensorBool("0_userdata.0.IsAnybodyHome");

            ProcessCompleted += bl_ProcessCompleted;
        }

        public void StartProcess()
        {
           
                Console.WriteLine("Process Started!");


                OnProcessCompleted(JemandZuhause);
            
        }

        protected virtual void OnProcessCompleted(SensorBool sensorBool)
        {
            ProcessCompleted?.Invoke(this, sensorBool);
        }

        public static void bl_ProcessCompleted(object sender, SensorBool sensorBool)
        {
            Console.WriteLine("event kam durch");
        }
    }
}
