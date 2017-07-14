using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Xml.Linq;
using System.Xml.Schema;
using System.Xml.Serialization;

namespace Fractal_Designer
{
    /// <remarks/>
    [System.SerializableAttribute()]
    [System.ComponentModel.DesignerCategoryAttribute("code")]
    [System.Xml.Serialization.XmlTypeAttribute(AnonymousType = true)]
    [System.Xml.Serialization.XmlRootAttribute(Namespace = "", IsNullable = false)]
    public partial class Settings
    {

        private decimal parameterField;

        private ushort iterationsField;

        /// <remarks/>
        public decimal parameter
        {
            get
            {
                return this.parameterField;
            }
            set
            {
                this.parameterField = value;
            }
        }
        /// <remarks/>
        public ushort iterations
        {
            get
            {
                return this.iterationsField;
            }
            set
            {
                this.iterationsField = value;
            }
        }

        const string XmlPath = "../../Settings/Settings.xml";
        const string XsdPath = "../../Settings/Settings.xsd";
        public static void Load(out Settings programSettings)
        {
            programSettings = null;
            var serializer = new XmlSerializer(typeof(Settings));

            try
            {
                var xmlData = ValidateSettings(XmlPath);
                programSettings = serializer.Deserialize(new StringReader(xmlData)) as Settings;
            }
            catch (Exception e)
            {
                var result = System.Windows.Forms.MessageBox.Show(
                    $"Corrupt settings file. Retry?", $"{e.Message}",
                    System.Windows.Forms.MessageBoxButtons.YesNo,
                    System.Windows.Forms.MessageBoxIcon.Error);

                if (result != System.Windows.Forms.DialogResult.Yes)
                    Application.Current.Shutdown();

                Save(default(Settings));
                try
                {
                    var xmlData = ValidateSettings(XmlPath);
                    programSettings = serializer.Deserialize(new StringReader(xmlData)) as Settings;
                }
                catch (Exception ee)
                {
                    System.Windows.Forms.MessageBox.Show("Could not fix the error.", "Please try removing the old settings file. The app could not self-fix. Exiting the application.", System.Windows.Forms.MessageBoxButtons.OK, System.Windows.Forms.MessageBoxIcon.Error);
                    Application.Current.Shutdown();
                }
            }
            // nicely communicate loading errors
        }
        public static void Save(Settings programSettings)
        {
            var serializer = new XmlSerializer(typeof(Settings), "settings");
            var writer = new StreamWriter(XmlPath);
            serializer.Serialize(writer, programSettings);
            writer.Close();
        }

        public static string ValidateSettings(string xmlFilename)
        {
            var xmlData = File.ReadAllText(xmlFilename);
            var xsdData = File.ReadAllText(XsdPath);
            var document = XDocument.Parse(xmlData);
            var schemaSet = new XmlSchemaSet();

            schemaSet.Add(XmlSchema.Read(new StringReader(xsdData), (o, e) =>
            {
                if (e.Exception != null)
                    throw e.Exception;
            }));

            document.Validate(schemaSet, (o, e) =>
            {
                if (e.Severity == XmlSeverityType.Error)
                    throw e.Exception;
            });

            return xmlData;
        }
    }


}
