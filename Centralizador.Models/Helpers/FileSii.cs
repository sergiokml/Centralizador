using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

using Centralizador.Models.ApiSII;

namespace Centralizador.Models.Helpers
{
    public class FileSii
    {
        public bool ExistsFile { get; set; }

        private static List<AuxCsv> AuxCsvsList { get; set; }

        public static string PathExcelFileSii
        {
            get
            {
                var dir = Environment.GetFolderPath(Environment.SpecialFolder.Desktop);
                return dir + @"\" + $"ce_empresas_dwnld_{DateTime.Now.Year}{string.Format("{0:00}", DateTime.Now.Month)}{string.Format("{0:00}", DateTime.Now.Day)}.csv";
            }
        }

        public FileSii()
        {
            ExistsFile = GetFile();
        }

        private bool GetFile()
        {
            try
            {
                return File.Exists(PathExcelFileSii);
            }
            catch (Exception)
            {
                return false;
            }
        }

        public static AuxCsv GetAuxCvsFromFile(Detalle detalle)
        {
            AuxCsv auxCsv = new AuxCsv();
            try
            {
                //await Task.Run(() =>
                //{
                auxCsv = AuxCsvsList.FirstOrDefault(x => x.Rut == detalle.Instruction.ParticipantDebtor.Rut + "-" + detalle.Instruction.ParticipantDebtor.VerificationCode);
                return auxCsv;
                //});
            }
            catch (Exception)
            {
                return null;
            }
            //return null;
        }

        public static async Task ReadFileSii()
        {
            await Task.Run(() =>
            {
                AuxCsvsList = File.ReadAllLines(PathExcelFileSii).Skip(1).Select(v => AuxCsv.GetFronCsv(v)).ToList();
            });
        }

        //public static void ReadFileSii()
        //{
        //    AuxCsvsList = File.ReadAllLines(PathExcelFileSii).Skip(1).Select(v => AuxCsv.GetFronCsv(v)).ToList();
        //}
    }

    public class AuxCsv
    {
        public string Rut { get; set; }
        public string Name { get; set; }
        public string Email { get; set; }

        public static AuxCsv GetFronCsv(string csvLine)
        {
            try
            {
                string[] values = csvLine.Split(';');
                if (values.Count() == 6)
                {
                    AuxCsv aux = new AuxCsv
                    {
                        Rut = values[0],
                        Name = values[1],
                        Email = values[4]
                    };
                    return aux;
                }
                else
                {
                    AuxCsv aux = new AuxCsv
                    {
                        Rut = "0",
                        Name = "0",
                        Email = "0"
                    };
                    return aux;
                }
            }
            catch (Exception)
            {
                throw;
            }
        }
    }
}