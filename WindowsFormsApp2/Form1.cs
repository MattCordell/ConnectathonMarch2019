using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.IO;
using Hl7.Fhir.Model;
using Hl7.Fhir.Rest;


namespace WindowsFormsApp2
{

    public partial class Form1 : Form
    {
        List<Patient> data = new List<Patient>();

        //string drugFilter = "*";

        public Form1()
        {
            InitializeComponent();

            data = LoadPatientData();
            var dt = new DataTable();



            //var bindingList = new BindingList<Patient>(data);           
            //bindingList.Where<Patient>(p => p.drugId.Equals(1073941000168107));       
            //var source = new BindingSource(bindingList, null);         
            //dataGridView1.DataSource = data.Where<Patient>(p => p.


            dataGridView1.DataSource = data;

            
            


            dataGridView1.Show();
            
            
        }

        public List<Patient> LoadPatientData()
        {
            var dataFile = @"C:\Users\Matthew.Cordell\Desktop\ConnectData.txt";
            var data = File.ReadAllLines(dataFile);
            var d2 = new List<Patient>();

            foreach (var item in data)
            {
                var x = item.Split('\t');

                var patient = new Patient();
                patient.firstName = x[0];
                patient.lastName = x[1];
                patient.sex = x[2];
                patient.dob = x[3];
                patient.drugId = x[4];
                patient.drugName = x[5];
                patient.disorderId = x[6];
                patient.disorderName = x[7];

                d2.Add(patient);
            }
            return d2;
        }


        private void textBox2_TextChanged(object sender, EventArgs e)
        {

        }

        private void textBox1_TextChanged(object sender, EventArgs e)
        {

        }

        private void button1_Click(object sender, EventArgs e)
        {
            var drugFilter = GenerateDrugFilter();
            
            List<Patient> result = data.Where<Patient>(p => drugFilter.Contains(p.drugId)).ToList();
         

            dataGridView1.DataSource = result;
        }

        private List<string> GenerateDrugFilter()
        {
            var ecl = textBox2.Text;
            const string Endpoint = "https://ontoserver.csiro.au/stu3-latest";
            string ValueSetURL = "http://snomed.info/sct?fhir_vs=ecl/"+ ecl;

            //ExpandECL for Box2
            var client = new FhirClient(Endpoint);           
            var url = new FhirUri(ValueSetURL);
            var result = client.ExpandValueSet(url);

            var ids = new List<string>();

            foreach (var item in result.Expansion.Contains)
            {
                ids.Add(item.Code);
            }

            var translations = new List<string>();

            foreach (var id in ids)
            {
                //Reverse Translate
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
                    Value = new FhirString(id)
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
                //if there's a map
                //if (transResult.Parameter[0].Value.Equals("true"))
                //{
                    foreach (var match in transResult.Parameter.Where<Parameters.ParameterComponent>(e => e.Name == "match"))
                    {
                        //var coding = match.Part.Where<Parameters.ParameterComponent>(e => e.Name == "concept");
                        //This workds
                        //var coding1 = match.Part[1].Value;
                        var valueCoding = match.Part.Where<Parameters.ParameterComponent>(p => p.Name.Equals("concept")).FirstOrDefault();

                        var coding = (Coding)valueCoding.Value;

                        translations.Add(coding.Code);                        
                    }
                //}

            }

            //Expand the AMT ingredients
            string AMT_ecl = "http://snomed.info/sct?fhir_vs=ecl/<<30425011000036101:<<762951001=(";


            foreach (var item in translations)
            {
                AMT_ecl = AMT_ecl + item + " OR ";
            }
            //close the expression
            AMT_ecl = AMT_ecl + "123456 OR 123456)";

            var AMT_url = new FhirUri(AMT_ecl);
            var AMTs = client.ExpandValueSet(AMT_url);

            var filtered = new List<string>();
            foreach (var item in AMTs.Expansion.Contains)
            {
                filtered.Add(item.Code);
            }


            return filtered;
        }
    }

    public class Patient
    {
        public string firstName { get; set; }
        public string lastName { get; set; }
        public string sex { get; set; }
        public string dob { get; set; }
        public string drugId { get; set; }
        public string drugName { get; set; }
        public string disorderId { get; set; }
        public string disorderName { get; set; }
    }

}
