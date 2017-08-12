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
    public enum Algorithm
    {
        Halley,
        Halley_overnewtoned,
        Quadratic,
        Newton,
        Newton_iterative,
        Secant,
        Inverse,
        Muller,
        Steffensen,
    }

    public enum DragEffect
    {
        Move,
        SingleRoot,
        Singularity,
        DoubleRoot,
        Reset,
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

        static Uri XmlPath = new Uri("./Settings.xml", UriKind.Relative);

        private static bool ForbidRefresh = false;
        private static bool ForbidSaving = false;
        private static bool IsSaving = false;

        public static void Reset()
        {
            Instance = new Settings();

            Recompute?.Invoke();
            Save(true);
        }

        private static void Load() => Load(out _Instance);
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
                if (File.Exists(XmlPath.ToString()))
                {
                    var result = System.Windows.Forms.MessageBox.Show(
                                $"{e.Message}", $"Existing yet corrupt settings file. Try fixing?",
                                System.Windows.Forms.MessageBoxButtons.YesNo,
                                System.Windows.Forms.MessageBoxIcon.Error);

                    if (result != System.Windows.Forms.DialogResult.Yes)
                    {
                        workOffline = true;
                        programSettings = new Settings();
                    }
                }

                programSettings = new Settings();
                Save(programSettings, true);

                try
                {
                    var xmlData = ValidateSettings(XmlPath);
                    programSettings = serializer.Deserialize(new StringReader(xmlData)) as Settings;
                }
                catch (Exception ee)
                {
                    System.Windows.Forms.MessageBox.Show("Could not fix settings file.", $"Please try removing the old settings file. {ee.InnerException} Working without persistent settings.", System.Windows.Forms.MessageBoxButtons.OK, System.Windows.Forms.MessageBoxIcon.Error);

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

        private static void Save(bool saveImmediately = false) => Save(Instance, saveImmediately);
        private static void Save(Settings programSettings, bool saveImmediately = false)
        {
            if (ForbidSaving || (IsSaving && !saveImmediately))
                return;

            if (saveImmediately)
            {
                var serializer = new XmlSerializer(typeof(Settings));
                var writer = new StreamWriter(XmlPath.ToString());
                serializer.Serialize(writer, programSettings);
                writer.Close();
            }
            else
            {
                IsSaving = true;
                Task.Factory.StartNew(() =>
                {
                    System.Threading.Thread.Sleep(1000);
                    Save(programSettings, true);
                    IsSaving = false;
                });
            }
        }

        private static string ValidateSettings(Uri xmlFilename)
        {
            var xmlData = File.ReadAllText(xmlFilename.ToString());
            var xsdData = Properties.Resources.Settings;
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
        private ushort iterationsField = 100;
        private ushort drageffectField = 0;
        private ushort algorithmField = 0;

        public decimal radiusD
        {
            get => radiusField;
            set
            {
                if (value == radiusField)
                    return;

                _Radius = (double)value;
                radiusField = value;
                if (!ForbidRefresh)
                {
                    PropertyChanged?.Invoke(this, new PropertyChangedEventArgs("radius"));
                    Recompute?.Invoke();
                    Save(this);
                }
            }
        }

        private double _Radius = 1;
        [XmlIgnore]
        public double Radius
        {
            get => _Radius;
            set
            {
                if (value == _Radius)
                    return;

                _Radius = value;
                if ((double)decimal.MaxValue < value)
                {
                    radiusD = decimal.MaxValue;
                }
                else
                    radiusD = (decimal)value;
                if (!ForbidRefresh)
                {
                    PropertyChanged?.Invoke(this, new PropertyChangedEventArgs("Radius"));
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
                CenterSaved = new Complex((double)centerrealField, (double)centerimaginaryField);

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
                CenterSaved = new Complex((double)centerrealField, (double)centerimaginaryField);

                if (!ForbidRefresh)
                {
                    PropertyChanged?.Invoke(this, new PropertyChangedEventArgs("centerimaginary"));
                    Recompute?.Invoke();
                    Save(this);
                }
            }
        }

        private Complex CenterSaved = 0;
        [XmlIgnore]
        public Complex Center
        {
            get => CenterSaved;
            set
            {
                var decimalizedReal = decimal.MaxValue;
                var decimalizedImaginary = decimal.MaxValue;

                if (value.Real < (double)decimalizedReal)
                    decimalizedReal = (decimal)value.Real;

                if (value.Imaginary < (double)decimalizedImaginary)
                    decimalizedImaginary = (decimal)value.Real;

                if (decimalizedReal == Instance.centerrealField && decimalizedImaginary == Instance.centerimaginaryField)
                    return;

                Instance.centerrealField = decimalizedReal;
                Instance.centerimaginary = decimalizedImaginary;
                CenterSaved = value;

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
                    Recompute?.Invoke();
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

}
