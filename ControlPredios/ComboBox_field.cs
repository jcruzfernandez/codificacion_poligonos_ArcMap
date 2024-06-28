using ESRI.ArcGIS.Carto;
using ESRI.ArcGIS.Geodatabase;
using System;

namespace ControlPredios
{
    public class ComboBox_field : ESRI.ArcGIS.Desktop.AddIns.ComboBox
    {
        private static ComboBox_field _instance;
        public static string selectedFieldName;
        public static IField selectedField;
        public static ComboBox_field Instance
        {
            get
            {
                if (_instance == null)
                {
                    _instance = new ComboBox_field();
                }
                return _instance;
            }
        }
        public ComboBox_field()
        {
            _instance = this;
            UpdateComboBoxFields();
        }

        protected override void OnUpdate()
        {
            Enabled = ComboBox1.selectedLayer != null;
            // UpdateComboBox();
        }

        public void UpdateComboBoxFields()
        {
            // Limpiar los ítems existentes en el ComboBoxFields
            this.Clear();
            try
            {
                // Verificar si hay una capa seleccionada en ComboBox1
                IFeatureLayer selectedLayer = ComboBox1.selectedLayer;
                if (selectedLayer != null)
                {
                    IFeatureClass featureClass = selectedLayer.FeatureClass;
                    IFields fields = featureClass.Fields;

                    // Agregar todos los campos de la capa seleccionada al ComboBox
                    for (int i = 0; i < fields.FieldCount; i++)
                    {
                        IField field = fields.get_Field(i);
                        this.Add(field.Name, field);
                    }
                }
            }
            catch (Exception ex)
            {
                System.Windows.Forms.MessageBox.Show("Error: " + ex.Message);
            }
        }

        protected override void OnSelChange(int index)
        {
            if (index != -1)
            { 
            var getField = this.GetItem(index);
            selectedField = getField.Tag as IField;
            selectedFieldName = getField.Caption;
            }
        }

    }

}
