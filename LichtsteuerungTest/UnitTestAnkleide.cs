using Microsoft.VisualStudio.TestTools.UnitTesting;
using Lichtsteuerung;
using System;

namespace LichtsteuerungTest
{
    [TestClass]
    public class UnitTestAnkleide
    {

        private void Initialize()
        {
            SteuerungLogic.Instance.IsDebug = true; //kann ganz am anfang aktiviert werden
            SteuerungLogic.Instance.Start(); //führt dann den konstruktor aus
        }

        [TestMethod]
        public void AnkleideStandard()
        {
            try
            {
                Initialize();


                bool bewegung = SteuerungLogic.Instance.LichtsteuerungAnkleidezimmer.AnkleideBewegung.Status;
                string debug = SteuerungLogic.Instance.LichtsteuerungAnkleidezimmer.LichtAnkleide.ObjektId;

                bool zuhause = SteuerungLogic.Instance.JemandZuhause.Status;
           
            }
            catch (Exception ex)
            {
                Console.WriteLine("Fehler bei TestMethod1", ex);
                //throw;
            }
        }
    }
}
