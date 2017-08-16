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
        Halley_without_derivative,

        Quadruple,
        Quadratic,
        Quadratic_without_derivative,

        Newton,
        Newton_without_derivative,
        Secant_Newton_combination,
        Secant,

        Inverse,
        Muller,
        Moler_real,
        Steffensen,

        Custom,
    }

    public enum DragEffect
    {
        Move,

        SingleRoot,
        DoubleRoot,
        Singularity,

        Reset,
    }

    public enum Colorer
    {
        Root_phase,
        Iterations,
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
        private decimal _radius = 1;
        private decimal _centerreal = 0;
        private decimal _centerimaginary = 0;
        private decimal _parameters = 1;
        private ushort _iterations = 100;
        private ushort _drageffect = 0;
        private ushort _algorithm = 0;
        private decimal _delta = -15;
        private decimal _eps = -10;
        private decimal _epseps = -15;
        private ushort _colorer = 0;

        public decimal radiusD
        {
            get => _radius;
            set
            {
                if (value == _radius)
                    return;

                _Radius = (double)value;
                _radius = value;
                if (!ForbidRefresh)
                {
                    PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(radiusD)));
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
                    PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(Radius)));
                    Recompute?.Invoke();
                    Save(this);
                }
            }
        }

        public decimal centerreal
        {
            get => _centerreal;
            set
            {
                if (value == _centerreal)
                    return;

                _centerreal = value;
                CenterSaved = new Complex((double)_centerreal, (double)_centerimaginary);

                if (!ForbidRefresh)
                {
                    PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(centerreal)));
                    Recompute?.Invoke();
                    Save(this);
                }
            }
        }

        public decimal centerimaginary
        {
            get => _centerimaginary;
            set
            {
                if (value == _centerimaginary)
                    return;

                _centerimaginary = value;
                CenterSaved = new Complex((double)_centerreal, (double)_centerimaginary);

                if (!ForbidRefresh)
                {
                    PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(centerimaginary)));
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

                if (decimalizedReal == Instance._centerreal && decimalizedImaginary == Instance._centerimaginary)
                    return;

                Instance._centerreal = decimalizedReal;
                Instance.centerimaginary = decimalizedImaginary;
                CenterSaved = value;

                if (!ForbidRefresh)
                {
                    PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(centerreal)));
                    PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(centerimaginary)));
                    Recompute?.Invoke();
                    Save();
                }
            }
        }

        public decimal parameter
        {
            get => _parameters;
            set
            {
                if (value == _parameters)
                    return;

                _parameters = value;

                if (!ForbidRefresh)
                {
                    PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(parameter)));
                    Recompute?.Invoke();
                    Save(this);
                }
            }
        }

        public decimal eps
        {
            get => _eps;
            set
            {
                if (value == _eps)
                    return;

                _eps = value;

                if (!ForbidRefresh)
                {
                    PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(eps)));
                    Recompute?.Invoke();
                    Save(this);
                }
            }
        }

        public decimal delta
        {
            get => _delta;
            set
            {
                if (value == _delta)
                    return;

                _delta = value;

                if (!ForbidRefresh)
                {
                    PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(delta)));
                    Recompute?.Invoke();
                    Save(this);
                }
            }
        }

        public decimal epseps
        {
            get => _epseps;
            set
            {
                if (value == _epseps)
                    return;

                _epseps = value;

                if (!ForbidRefresh)
                {
                    PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(epseps)));
                    Recompute?.Invoke();
                    Save(this);
                }
            }
        }

        public ushort iterations
        {
            get => _iterations;
            set
            {
                if (value == _iterations)
                    return;

                _iterations = value;

                if (!ForbidRefresh)
                {
                    PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(iterations)));
                    Recompute?.Invoke();
                    Save(this);
                }
            }
        }

        public ushort drageffect
        {
            get => _drageffect;
            set
            {
                if (value == _drageffect)
                    return;

                _drageffect = value;

                if (!ForbidRefresh)
                {
                    PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(drageffect)));
                    //Recompute?.Invoke();
                    Save(this);
                }
            }
        }

        public ushort algorithm
        {
            get => _algorithm;
            set
            {
                if (value == _algorithm)
                    return;

                _algorithm = value;

                if (!ForbidRefresh)
                {
                    PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(algorithm)));
                    Recompute?.Invoke();
                    Save(this);
                }
            }
        }

        public ushort colorer
        {
            get => _colorer;
            set
            {
                if (value == _colorer)
                    return;

                _colorer = value;

                if (!ForbidRefresh)
                {
                    PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(colorer)));
                    Recompute?.Invoke();
                    Save(this);
                }
            }
        }

    }

}
