using System;
using System.ComponentModel;
using CrystalTable.Models;

namespace CrystalTable.Logic
{
    public class MapDraft : INotifyPropertyChanged
    {
        private MapMetadata metadata;
        private WaferMapParameters parameters;
        private bool isDirty;
        private string templateName;

        public event PropertyChangedEventHandler PropertyChanged;

        public MapMetadata Metadata
        {
            get => metadata;
            set
            {
                if (metadata != value)
                {
                    metadata = value;
                    OnPropertyChanged(nameof(Metadata));
                    IsDirty = true;
                }
            }
        }

        public WaferMapParameters Parameters
        {
            get => parameters;
            set
            {
                if (parameters != value)
                {
                    parameters = value;
                    OnPropertyChanged(nameof(Parameters));
                    IsDirty = true;
                }
            }
        }

        public bool IsDirty
        {
            get => isDirty;
            private set
            {
                if (isDirty != value)
                {
                    isDirty = value;
                    OnPropertyChanged(nameof(IsDirty));
                }
            }
        }

        public string TemplateName
        {
            get => templateName;
            set
            {
                if (templateName != value)
                {
                    templateName = value;
                    OnPropertyChanged(nameof(TemplateName));
                }
            }
        }

        public MapDraft()
        {
            metadata = new MapMetadata(string.Empty, string.Empty, string.Empty);
            parameters = new WaferMapParameters();
            isDirty = false;
        }

        public void UpdateParameter(Action<WaferMapParameters> update)
        {
            update(Parameters);
            IsDirty = true;
        }

        public void Reset()
        {
            Metadata = new MapMetadata(string.Empty, string.Empty, string.Empty);
            Parameters = new WaferMapParameters();
            TemplateName = null;
            IsDirty = false;
        }

        public MapTemplate CreateTemplate()
        {
            return new MapTemplate
            {
                Name = Metadata.LotNumber,
                Metadata = new MapMetadata(Metadata.LotNumber, Metadata.WaferNumber, Metadata.Note),
                Parameters = new WaferMapParameters
                {
                    SizeX = Parameters.SizeX,
                    SizeY = Parameters.SizeY,
                    WaferDiameter = Parameters.WaferDiameter,
                    // Копируем остальные параметры
                }
            };
        }

        protected virtual void OnPropertyChanged(string propertyName)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}