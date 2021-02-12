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
            SteuerungLogic.Instance.Start();
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
