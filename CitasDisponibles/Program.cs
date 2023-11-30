using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using Newtonsoft.Json;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;
using System.Xml.XPath;

namespace CitasDisponibles
{
    internal class Program
    {
        List<SkeletorCitas> Citas;
        string diaConsultar;
        static void Main(string[] args)
        {
            Program program = new Program();
            var continuar = "si";
            program.LecturaJson();
            while(continuar == "si")
            {
                Console.WriteLine("Ingrese día para consultar.");
                program.diaConsultar = Console.ReadLine();

                Console.WriteLine(program.CuposDisponibles(program.diaConsultar));
                continuar = Console.ReadLine();
            }
        }
        private void LecturaJson()
        {
            
            string jsonCitas = "https://luegopago.blob.core.windows.net/luegopago-uploads/Pruebas%20LuegoPago/data.json";
            using (HttpClient client = new HttpClient())
            {
                try
                {
                    string jsonContent = client.GetStringAsync(jsonCitas).Result;

                    Citas = JsonConvert.DeserializeObject<List<SkeletorCitas>>(jsonContent);

                }
                catch (Exception ex)
                {
                    Console.WriteLine($"Error al leer el archivo JSON: {ex.Message}");
                }
            }
        }
        private class SkeletorCitas
        {
            // Se crea clase con los atributos necesarios para leer archivo Json
            public string Day { get; set; }
            public string Hour { get; set; }
            public string Duration { get; set; }
        }
        private int CuposDisponibles(string diaCita)
        {
            List<SkeletorCitas> diaConsultadoCitas = Citas.Where(cita => cita.Day.ToLower() == diaCita.ToLower()).ToList();
            DateTime horaComparacion = DateTime.ParseExact("09:00", "HH:mm", null);
            DateTime horaFinalDia = DateTime.ParseExact("17:00", "HH:mm", null);
            int cuposDisponibles = 0;
            foreach (var horariosOcupados in diaConsultadoCitas)
            {
                var minutosDisponibles = (DateTime.ParseExact(horariosOcupados.Hour, "HH:mm", null) - horaComparacion).TotalMinutes;
                cuposDisponibles = minutosDisponibles/30 >1 ? cuposDisponibles+(int)Math.Floor(minutosDisponibles/30) : cuposDisponibles;
                Console.Write(horariosOcupados.Hour + "   -   ");
                Console.WriteLine((Convert.ToDateTime(horariosOcupados.Hour).AddMinutes(Convert.ToUInt16(horariosOcupados.Duration))).ToString("HH:mm"));
                horaComparacion = (DateTime.ParseExact(horariosOcupados.Hour, "HH:mm", null).AddMinutes(Convert.ToUInt16(horariosOcupados.Duration)));
            }
            if (horaComparacion <= horaFinalDia)
            {
                var minutosDisponibles = (horaFinalDia - horaComparacion).TotalMinutes;
                cuposDisponibles = minutosDisponibles / 30 > 1 ? cuposDisponibles + (int)Math.Floor(minutosDisponibles / 30) : cuposDisponibles;
            }
            return cuposDisponibles;
        }
    }
}
