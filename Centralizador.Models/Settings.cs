using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Xml.Linq;

namespace Centralizador.Models.Properties
{
    // Esta clase le permite controlar eventos específicos en la clase de configuración:
    //  El evento SettingChanging se desencadena antes de cambiar un valor de configuración.
    //  El evento PropertyChanged se desencadena después de cambiar el valor de configuración.
    //  El evento SettingsLoaded se desencadena después de cargar los valores de configuración.
    //  El evento SettingsSaving se desencadena antes de guardar los valores de configuración.
    public sealed partial class Settings
    {
        public string DBServer { get; private set; }
        public string UserCen { get; private set; }
        public string PasswordCeN { get; private set; }
        public Uri UrlCen { get; private set; }
        public string User365 { get; private set; }
        public string Password365 { get; private set; }
        public string UserCC365 { get; private set; }
        public string SerialNumber { get; private set; }
        public Dictionary<string, int> DicReem { get; private set; }
        public string DBUser { get; private set; }
        public string DBPassword { get; private set; }
        public Dictionary<string, string> DicCompanies { get; private set; }
        public string UIDRange { get; set; }

        public string IPServer { get; set; }

        public DateTime DateTimeEmail { get; set; }


        public Settings()
        {
            SettingsLoaded += Settings_SettingsLoaded;
            SettingsSaving += Settings_SettingsSaving;
        }

        private void Settings_SettingsSaving(object sender, System.ComponentModel.CancelEventArgs e)
        {
            // Guardar en el Xml el nuevo dato
            // LOAD THE XML CONFIG
            XDocument doc = XDocument.Load(IPServer + @"Centralizador_config.xml");
            doc.Root.Element("ServidorCorreos").Element("DateTimeEmail").Value = DateTimeEmail.ToString("dd-MM-yyyy HH:mm:ss");
            doc.Root.Element("ServidorCorreos").Element("UIDRange").Value = UIDRange;
            doc.Save(IPServer + @"Centralizador_config.xml");

        }

        private void Settings_SettingsLoaded(object sender, System.Configuration.SettingsLoadedEventArgs e)
        {
            //   \\DESKTOP-7LUU0MK\Centralizador

            //   SRV-SOFTLAND
            var a = System.IO.Directory.GetCurrentDirectory() + @"\Centralizador.exe.config";
            XDocument docconfigApp = XDocument.Load(a);
            IPServer = @"\\" + docconfigApp
                .Root
                .Element("applicationSettings")
                .Element("Centralizador.WinApp.Properties.Settings")
                .Element("setting")
                .Value + @"\Centralizador\";

            //if (Environment.MachineName == "DEVELOPER")
            //{
            //    IPServer = @"\\DESKTOP-7LUU0MK\Centralizador\";
            //}
            //else
            //{
            //    IPServer = @"\\SRV-SOFTLAND\Centralizador\";
            //}
            if (string.IsNullOrEmpty(IPServer))
            {
                throw new Exception("Error en la IP de Centralizador.exe.config.");
            }

            try
            {
                // LOAD THE XML CONFIG
                var pp = IPServer + @"Centralizador_config.xml";
                XDocument doc = XDocument.Load(pp);

                // SOFTLAND DB
                DicCompanies = doc.Root.Element("SoftlandDB")
                    .Descendants("Empresa").ToDictionary(d => (string)d.Attribute("id"), d => (string)d);
                DBServer = doc.Root.Element("DataBase").Element("DBServer").Value;
                DBUser = doc.Root.Element("DataBase").Element("DBUser").Value;
                DBPassword = doc.Root.Element("DataBase").Element("DBPassword").Value;
                //CEN
                UserCen = doc.Root.Element("CEN").Element("UserCen").Value;
                PasswordCeN = doc.Root.Element("CEN").Element("PasswordCen").Value;
                UrlCen = new Uri(doc.Root.Element("CEN").Element("UrlCen").Value);

                //OUTLOOK
                User365 = doc.Root.Element("Outlook365").Element("User365").Value;
                Password365 = doc.Root.Element("Outlook365").Element("Password365").Value;
                UserCC365 = doc.Root.Element("Outlook365").Element("UserCC365").Value;
                UIDRange = doc.Root.Element("ServidorCorreos").Element("UIDRange").Value;
                var fecha = doc.Root.Element("ServidorCorreos").Element("DateTimeEmail").Value;
                DateTimeEmail = DateTime.ParseExact(fecha, "dd-MM-yyyy HH:mm:ss", CultureInfo.CreateSpecificCulture("es-CL"));
                //DateTimeEmail = Convert.ToDateTime(fecha, CultureInfo.InvariantCulture);

                //CERT
                SerialNumber = doc.Root.Element("CertificadoDigital").Element("SerialNumber").Value;

                //REEMPLAZOS
                DicReem = doc.Root.Element("Reemplazos").Descendants("Empresa").ToDictionary(d => (string)d.Attribute("id"), d => (int)d);
            }
            catch (Exception)
            {
                throw;
            }
        }
    }
}