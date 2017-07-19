using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Numerics;
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
    public partial class Settings : INotifyPropertyChanged
    {
        private decimal radiusField = 1;

        private decimal centerrealField = 0;

        private decimal centerimaginaryField = 0;

        private decimal parameterField = 1;

        private ushort iterationsField = 50;

        private ushort drageffectField = 0;

        private ushort algorithmField = 1;

        public decimal radius
        {
            get => radiusField;
            set
            {
                if (value == radiusField)
                    return;

                radiusField = value;
                if (!ForbidRefresh)
                {
                    PropertyChanged?.Invoke(this, new PropertyChangedEventArgs("radius"));
                    Recompute?.Invoke();
                    Save(this);
                }
            }
        }

        public decimal centerreal
        {
            get => centerrealField;
            set
            {
                if (value == centerrealField)
                    return;

                centerrealField = value;
                if (!ForbidRefresh)
                {
                    PropertyChanged?.Invoke(this, new PropertyChangedEventArgs("centerreal"));
                    Recompute?.Invoke();
                    Save(this);
                }
            }
        }

        public decimal centerimaginary
        {
            get => centerimaginaryField;
            set
            {
                if (value == centerimaginaryField)
                    return;

                centerimaginaryField = value;
                if (!ForbidRefresh)
                {
                    PropertyChanged?.Invoke(this, new PropertyChangedEventArgs("centerimaginary"));
                    Recompute?.Invoke();
                    Save(this);
                }
            }
        }

        [XmlIgnore]
        public Complex center
        {
            get => new Complex((double) Instance.centerreal, (double) Instance.centerimaginary);
            set
            {
                if ((decimal) value.Real == Instance.centerrealField && (decimal) value.Imaginary == Instance.centerimaginaryField)
                    return;

                Instance.centerrealField = (decimal) value.Real;
                Instance.centerimaginary = (decimal) value.Imaginary;


                if (!ForbidRefresh)
                {
                    PropertyChanged?.Invoke(this, new PropertyChangedEventArgs("centerreal"));
                    PropertyChanged?.Invoke(this, new PropertyChangedEventArgs("centerimaginary"));
                    Recompute?.Invoke();
                    Save();
                }
            }
        }

        public decimal parameter
        {
            get => parameterField;
            set
            {
                if (value == parameterField)
                    return;

                parameterField = value;

                if (!ForbidRefresh)
                {
                    PropertyChanged?.Invoke(this, new PropertyChangedEventArgs("parameter"));
                    Recompute?.Invoke();
                    Save(this);
                }
            }
        }

        public ushort iterations
        {
            get => iterationsField;
            set
            {
                if (value == iterationsField)
                    return;

                iterationsField = value;

                if (!ForbidRefresh)
                {
                    PropertyChanged?.Invoke(this, new PropertyChangedEventArgs("iterations"));
                    Recompute?.Invoke();
                    Save(this);
                }
            }
        }

        public ushort drageffect
        {
            get => drageffectField;
            set
            {
                if (value == drageffectField)
                    return;

                drageffectField = value;

                if (!ForbidRefresh)
                {
                    PropertyChanged?.Invoke(this, new PropertyChangedEventArgs("drageffect"));
                    Save(this);
                }
            }
        }

        public ushort algorithm
        {
            get => algorithmField;
            set
            {
                if (value == algorithmField)
                    return;

                algorithmField = value;

                if (!ForbidRefresh)
                {
                    PropertyChanged?.Invoke(this, new PropertyChangedEventArgs("algorithm"));
                    Recompute?.Invoke();
                    Save(this);
                }
            }
        }

    }

    public partial class Settings
    {
        public event PropertyChangedEventHandler PropertyChanged;
        public static event Action Recompute;

        private static Settings _Instance = null;

        public static Settings Instance
        {
            get
            {
                if (_Instance == null)
                {
                    Load();
                    return _Instance;
                }
                else
                    return _Instance;

            }
            private set => _Instance = value;
        }

        const string XmlPath = "../../Settings/Settings.xml";
        const string XsdPath = "../../Settings/Settings.xsd";

        public void Reset()
        {
            radius = 1;
            center = 0;
            parameter = 1;
            iterations = 50;
            drageffect = 0;
            algorithm = 1;
        }

        private static void Load() => Load(out _Instance);

        private static bool ForbidRefresh = false;
        private static bool ForbidSaving = false;

        private static void Load(out Settings programSettings)
        {
            programSettings = null;
            bool workOffline = false;
            var serializer = new XmlSerializer(typeof(Settings));

            ForbidRefresh = true;

            try
            {
                var xmlData = ValidateSettings(XmlPath);
                programSettings = serializer.Deserialize(new StringReader(xmlData)) as Settings;
            }
            catch (Exception e)
            {
                var result = System.Windows.Forms.MessageBox.Show(
                    $"{e.Message}", $"Missing or corrupt settings file. Try fixing?",
                    System.Windows.Forms.MessageBoxButtons.YesNo,
                    System.Windows.Forms.MessageBoxIcon.Error);

                if (result != System.Windows.Forms.DialogResult.Yes)
                {
                    workOffline = true;
                    programSettings = new Settings();
                }

                Save(new Settings());

                try
                {
                    var xmlData = ValidateSettings(XmlPath);
                    programSettings = serializer.Deserialize(new StringReader(xmlData)) as Settings;
                }
                catch (Exception ee)
                {
                    System.Windows.Forms.MessageBox.Show("Could not self-fix. Working offline.", $"Please try removing the old settings file. {ee.InnerException} Working without persistent settings.", System.Windows.Forms.MessageBoxButtons.OK, System.Windows.Forms.MessageBoxIcon.Error);

                    workOffline = true;
                    programSettings = new Settings();
                }
            }
            finally
            {
                ForbidRefresh = false;
                ForbidSaving = workOffline;
            }
        }

        private static void Save() => Save(Instance);

        private static void Save(Settings programSettings)
        {
            if (ForbidSaving)
                return;

            var serializer = new XmlSerializer(typeof(Settings), "settings");
            var writer = new StreamWriter(XmlPath);
            serializer.Serialize(writer, programSettings);
            writer.Close();
        }

        private static string ValidateSettings(string xmlFilename)
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
