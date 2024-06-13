using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using ESRI.ArcGIS.ArcMapUI;
using ESRI.ArcGIS.Carto;
using ESRI.ArcGIS.Framework;
using ESRI.ArcGIS.esriSystem;
using ESRI.ArcGIS.Desktop.AddIns;
using System.Windows.Forms;
using ESRI.ArcGIS.Geodatabase;
using ESRI.ArcGIS.Geometry;

namespace ControlPredios
{
    public class ComboBox1 : ESRI.ArcGIS.Desktop.AddIns.ComboBox
    {
        private IActiveViewEvents_Event activeViewEvents;
        public string layerSelected;
        public static IFeatureLayer selectedLayer;
        public ComboBox1()
        {
            // Obtener el documento de ArcMap y el mapa enfocado
            IMxDocument mxDoc = (IMxDocument)ArcMap.Application.Document;
            IMap map = mxDoc.FocusMap;

            // Obtener el evento ActiveViewEvents del mapa
            activeViewEvents = (IActiveViewEvents_Event)map;

            // Suscribirse al evento ItemAdded
            activeViewEvents.ItemAdded += OnLayerAdded;

            // Inicializar el ComboBox con las capas existentes
            UpdateComboBox();
        }

        private void OnLayerAdded(object item)
        {
            // Actualizar el ComboBox cuando se agrega una capa
            UpdateComboBox();
        }

        private void UpdateComboBox()
        {
            // Limpiar los ítems existentes en el ComboBox
            this.Clear();

            // Obtener el documento de ArcMap y el mapa enfocado
            IMxDocument mxDoc = (IMxDocument)ArcMap.Application.Document;
            IMap map = mxDoc.FocusMap;

            // Agregar todas las capas del mapa al ComboBox
            for (int i = 0; i < map.LayerCount; i++)
            {
                ILayer layer = map.get_Layer(i);
                if (layer is IFeatureLayer)
                {
                    IFeatureLayer featureLayer = (IFeatureLayer)layer;
                    IFeatureClass featureClass = featureLayer.FeatureClass;
                    if (featureClass.ShapeType == esriGeometryType.esriGeometryPolygon)
                    {
                        this.Add(layer.Name, layer);
                    }
                }
            }
        }


        protected override void OnSelChange(int index)
        {
            //base.OnSelChange(cookie);
            // Actualizar la capa seleccionada
            //selectedLayer = this.GetItem(index);
            var selectedLyr = this.GetItem(index);
            selectedLayer = selectedLyr.Tag as IFeatureLayer;
            if (selectedLayer != null)
            {
                ComboBox_field.Instance.UpdateComboBoxFields();
                //    MessageBox.Show($"Capa seleccionada: {selectedLyr.Caption}", "Información de la capa", MessageBoxButtons.OK, MessageBoxIcon.Information);
            }

    }


        protected override void OnUpdate()
        {
            Enabled = ArcMap.Application != null;
        }

        ~ComboBox1()
        {
            // Remover el manejador de eventos para evitar memory leaks
            if (activeViewEvents != null)
            {
                activeViewEvents.ItemAdded -= OnLayerAdded;
            }
        }



    }

}
