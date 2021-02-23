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
            SteuerungLogic.Instance.Start(); //f�hrt dann den konstruktor aus
        }

        [TestMethod]
        public void AnkleideStandard()
        {
            try
            {
                Initialize();


                bool zuhause = SteuerungLogic.Instance.JemandZuhause.Status;
            bool debug = SteuerungLogic.Instance.IsDebug ;
            }
            catch (Exception ex)
            {
                Console.WriteLine("Fehler bei TestMethod1", ex);
                //throw;
            }
        }
    }
}
