using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using System.Diagnostics;

using JusiBase;

namespace Lichtsteuerung
{
    [ApiController]
    [Route("api/[controller]")]
    public class LichtsteuerungController : ControllerBase
    {
       

        private readonly ILogger<LichtsteuerungController> _logger;

        public LichtsteuerungController(ILogger<LichtsteuerungController> logger)
        {
            _logger = logger;
        }

        //[HttpGet("{id}", Name = "Get")]
        //public ResponseTrigger Get(string id)
        //{

        //    try
        //    {
        //        Stopwatch sw = new Stopwatch();
        //        sw.Start();

        //        Console.WriteLine("getter Aufruf {0}",id);

        //        SteuerungLogic.Instance.Update();


        //        Console.WriteLine("getter fertig ausgeführt, dauer: {0}", sw.ElapsedMilliseconds);
        //        sw.Stop();

        //        return new ResponseTrigger
        //        {
        //            ReturnCode = 0,
        //            ReturnState = SteuerungLogic.Instance.StateMachine.CurrentState.ToString()
        //        };
        //    }
        //    catch (Exception ex)
        //    {
        //        Console.WriteLine("Fehler bei LichtsteuerungAufruf: " + ex);

        //        return new ResponseTrigger
        //        {
        //            ReturnCode = 0,
        //            ReturnState = "Fehler"
        //        };

        //    }


        //}


        // api/lichtsteuerung/lichtankleide
        // /api/lichtsteuerung/allgemein?source=JemandZuhause
        [HttpGet("{id}", Name = "Get")]
        public ResponseTrigger Get(string id, string source)
        {

            try
            {
                Stopwatch sw = new Stopwatch();
                sw.Start();

                
                if (source == null)
                {
                    Console.WriteLine("getter Aufruf mit Zielelement {0} ohne source", id);
                }
                else
                { 
                    Console.WriteLine("getter Aufruf mit Zielelement {0} und source {1}", id, source);
                    switch (id)
                    {
                        case "lichtankleide":
                            switch (source)
                            {
                                case "AnkleideBewegung":
                                    SteuerungLogic.Instance.LichtsteuerungAnkleidezimmer.AnkleideBewegung.RaiseDataChange(true);
                                    break;
                                case "AnkleideHelligkeit":
                                    SteuerungLogic.Instance.LichtsteuerungAnkleidezimmer.AnkleideHelligkeit.RaiseDataChange(true);
                                    break;
                                case "AnkleideTuer":
                                    SteuerungLogic.Instance.LichtsteuerungAnkleidezimmer.AnkleideTuer.RaiseDataChange(true);
                                    break;
                                case "LichtAnkleide":
                                    SteuerungLogic.Instance.LichtsteuerungAnkleidezimmer.LichtAnkleide.RaiseDataChange(true);
                                    break;
                            }                                                   
                            break;
                        case "lichtgarderobe":
                            switch (source)
                            {
                                case "GarderobeBewegung":
                                    SteuerungLogic.Instance.LichtsteuerungGarderobe.GarderobeBewegung.RaiseDataChange(true);
                                    break;
                                case "GarderobeHelligkeit":
                                    SteuerungLogic.Instance.LichtsteuerungGarderobe.GarderobeHelligkeit.RaiseDataChange(true);
                                    break;
                                case "HaustuerBewegung":
                                    SteuerungLogic.Instance.LichtsteuerungGarderobe.HaustuerBewegung.RaiseDataChange(true);
                                    break;
                                case "GarageTuer":
                                    SteuerungLogic.Instance.LichtsteuerungGarderobe.GarageTuer.RaiseDataChange(true);
                                    break;                                   
                            }                            
                            break;
                        case "lichtspielzimmer":
                            switch (source)
                            {
                                case "SpielzimmerBewegung":
                                    SteuerungLogic.Instance.LichtsteuerungSpielzimmer.SpielzimmerBewegung.RaiseDataChange(true);
                                    break;
                                case "LichtSpielzimmer":
                                    SteuerungLogic.Instance.LichtsteuerungSpielzimmer.LichtSpielzimmer.RaiseDataChange(true);
                                    break;     
                            }

                            break;
                        case "allgemein":
                            SteuerungLogic.Instance.JemandZuhause.RaiseDataChange(true);
                            break;

                    }
                }
                                     
                Console.WriteLine("getter fertig ausgeführt, dauer: {0}", sw.ElapsedMilliseconds);
                sw.Stop();

                return new ResponseTrigger
                {
                    ReturnCode = 0,
                    ReturnState = SteuerungLogic.Instance.LichtsteuerungAnkleidezimmer.StateMachine.CurrentState.ToString()
                };
            }
            catch (Exception ex)
            {
                Console.WriteLine("Fehler bei LichtsteuerungAufruf: " + ex);

                return new ResponseTrigger
                {
                    ReturnCode = 0,
                    ReturnState = "Fehler"
                };

            }


        }
    }
}
