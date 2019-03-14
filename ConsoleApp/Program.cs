using System;
using Hl7.Fhir.Rest;
using Hl7.Fhir.Model;
using System.Linq;
using System.Collections.Generic;

namespace ConsoleApp
{
    class Program
    {
        const string Endpoint = "https://ontoserver.csiro.au/stu3-latest";
        //const string ValueSetURL = "http://snomed.info/sct?fhir_vs=refset/1072351000168102";
        //const string ValueSetURL = "url=http%3A%2F%2Fsnomed.info%2Fsct%3Ffhir_vs%3Decl%2F%3C762951001";

        const string ValueSetURL = "http://snomed.info/sct?fhir_vs=ecl/<762951001";
        

        static void Main(string[] args)
        {
            Console.WriteLine("Hello World!");

            var client = new FhirClient(Endpoint);
            //var filter = new FhirString("inr");
            var url = new FhirUri(ValueSetURL);
            var result = client.ExpandValueSet(url);
            Console.WriteLine(result.Expansion.Total + " results total");
            Console.WriteLine(result.Expansion.Contains.FirstOrDefault().Display);

            var dict = VStoDictionary(result);

            var trans = client.TranslateConcept("2442011000036104",)

            Console.ReadKey();
        }

        public static Dictionary<string, string> VStoDictionary(ValueSet vs)
        {
            Dictionary<string, string> codeList = new Dictionary<string, string>();

            foreach (var e in vs.Expansion.Contains)
            {
                Console.WriteLine(e.Code + " | " + e.Display + " |");
                codeList.Add(e.Code, e.Display);
            }

            return codeList;
        }
    }



}
