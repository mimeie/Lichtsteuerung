using Microsoft.VisualStudio.TestTools.UnitTesting;
using Lichtsteuerung;
using System;

namespace LichtsteuerungTest
{
    [TestClass]
    public class UnitTest1
    {
        [TestMethod]
        public void TestMethod1()
        {
            try
            {
                SteuerungLogic.Instance.IsDebug = true; //kann ganz am anfang aktiviert werden
                SteuerungLogic.Instance.Start(); //führt dann den konstruktor aus
                

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
