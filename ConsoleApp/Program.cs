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

            //var dict = VStoDictionary(result);


            Console.WriteLine("Let's try translate!");
            #region TRANSLATE
            var revsertTranslateParameters = new Parameters
            {
                Parameter = new List<Parameters.ParameterComponent>
               {
                  new Parameters.ParameterComponent
                  {
                    Name = "url",
                    Value = new FhirUri("http://snomed.info/sct?fhir_cm=281000036105")
                  },
                  new Parameters.ParameterComponent
                  {
                    Name = "system",
                    Value = new FhirUri("http://snomed.info/sct")
                  },
                  new Parameters.ParameterComponent
                  {
                    Name = "code",
                    Value = new FhirString("419442005")
                  },
                  new Parameters.ParameterComponent
                  {
                    Name = "target",
                    Value = new FhirUri("http://snomed.info/sct")
                  },
                  new Parameters.ParameterComponent
                  {
                    Name = "reverse",
                    Value = new FhirBoolean(true)
                  }
                 }
            };

            var transResult = (Parameters)client.TypeOperation<ConceptMap>("translate", revsertTranslateParameters);

            //
            Console.WriteLine(transResult.Parameter.First().Name + " : " + transResult.Parameter.First().Value.ToString());

            foreach (var match in transResult.Parameter.Where<Parameters.ParameterComponent>(e => e.Name=="match"))
            {
                //var coding = match.Part.Where<Parameters.ParameterComponent>(e => e.Name == "concept");
                //This workds
                //var coding1 = match.Part[1].Value;
                var valueCoding = match.Part.Where<Parameters.ParameterComponent>(p => p.Name.Equals("concept")).FirstOrDefault();

                var coding = (Coding)valueCoding.Value;
                
                Console.WriteLine(coding.Code + " " + coding.Display);

            }
            #endregion TRANSLATE

            #region search
            var substanceECL = "http://snomed.info/sct?fhir_vs=ecl/<<105590001";
            var resultSizeLimit = 5;
            var searchFilter = "morphi";

            
            var Newresult = client.ExpandValueSet(new FhirUri(substanceECL), new FhirString(searchFilter));

            var x = Newresult.Expansion.Contains;
            foreach (var item in x)
            {
                Console.WriteLine(item.Code + " | " + item.Display);
            }


            #endregion search

            Console.WriteLine("Done");




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
