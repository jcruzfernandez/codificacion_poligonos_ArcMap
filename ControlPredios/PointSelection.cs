using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using ESRI.ArcGIS.ArcMapUI;
using ESRI.ArcGIS.Carto;
using ESRI.ArcGIS.Desktop.AddIns;
using ESRI.ArcGIS.esriSystem;
using ESRI.ArcGIS.Framework;
using ESRI.ArcGIS.Geometry;
using ESRI.ArcGIS.Display;
using ESRI.ArcGIS.Geodatabase;
using System.Windows.Forms;
using ESRI.ArcGIS.Geoprocessing;

namespace ControlPredios
{
    public class PointSelection : ESRI.ArcGIS.Desktop.AddIns.Tool
    {
        IActiveView vista;
        IMap mapa;
        IPoint punto;
        string layerName;
        private static string currentPath = Directory.GetCurrentDirectory();
        string pathTool = System.IO.Path.Combine(currentPath, "scripts", "ToolboxMain.tbx");

        public PointSelection()
        {
            //System.Windows.Forms.Cursor.Current = System.Windows.Forms.Cursors.Cross;
        }

        protected override void OnActivate()
        {
            // Cambiar el cursor a una cruz cuando la herramienta se activa
            //System.Windows.Forms.Cursor.Current = System.Windows.Forms.Cursors.Cross;
            Cursor = System.Windows.Forms.Cursors.Cross;
        }

        protected override void OnMouseDown(MouseEventArgs arg)
        {
            System.Windows.Forms.Cursor.Current = System.Windows.Forms.Cursors.Cross;
            vista = ArcMap.Document.ActivatedView;
            punto = vista.ScreenDisplay.DisplayTransformation.ToMapPoint(arg.X, arg.Y);
            ArcMap.Document.FocusMap.SelectByShape(punto, null, false);
            mapa = ArcMap.Document.FocusMap;
            for (int i = 0; i < mapa.LayerCount; i++)
            {
                ILayer layer = mapa.get_Layer(i);
                if (layer is IFeatureLayer) // Asegúrate de que la capa sea una capa de entidades
                {
                    IFeatureLayer featureLayer = (IFeatureLayer)layer;
                    //IFeatureClass featureClass = featureLayer.FeatureClass;

                    // Crea un buffer alrededor del punto de clic si es necesario para mejorar la selección
                    // o ajusta la geometría de selección según tus necesidades
                    IGeometry selectGeom = punto as IGeometry;

                    // Verifica si el punto de clic intersecta alguna entidad de esta capa
                    if (IsFeatureIntersected(selectGeom, featureLayer))
                    {
                        layerName = layer.Name;
                        //MessageBox.Show("Capa seleccionada: " + layerName);

                        // Aquí puedes realizar el geoprocesamiento necesario con la capa seleccionada
                        // Por ejemplo, pasando el layerName o el featureLayer a otro método

                        break; // Sale del bucle si ya encontraste una capa que contiene el punto
                    }
                }
            }
            vista.PartialRefresh(esriViewDrawPhase.esriViewGeoSelection, null, null);
        }

        public static bool IsFeatureIntersected(IGeometry selectGeom, IFeatureLayer featureLayer)
        {
            // Prepara una consulta para encontrar entidades que intersecten con la geometría de selección
            ISpatialFilter spatialFilter = new SpatialFilter();
            spatialFilter.Geometry = selectGeom;
            spatialFilter.SpatialRel = esriSpatialRelEnum.esriSpatialRelIntersects;

            // Usa el FeatureClass del FeatureLayer para realizar la consulta
            IFeatureClass featureClass = featureLayer.FeatureClass;
            IFeatureCursor cursor = featureClass.Search(spatialFilter, false);

            // Verifica si se encontró alguna entidad
            IFeature feature = cursor.NextFeature();
            bool isIntersected = (feature != null);

            // Limpieza
            System.Runtime.InteropServices.Marshal.ReleaseComObject(cursor);

            return isIntersected;
        }

        protected override void OnDoubleClick()
        {
            GeoProcessor geoprocessor = new GeoProcessor();
            // Opcional: Configura el geoprocesador para sobrescribir la salida
            geoprocessor.OverwriteOutput = true;
            //MessageBox.Show("Capa seleccionada: " + layerName);
            try
            {
                geoprocessor.AddToolbox(pathTool);
                // Crea un objeto IVariantArray para almacenar los parámetros de la herramienta
                IVariantArray parameters = new VarArrayClass();
                parameters.Add(layerName);
                geoprocessor.Execute("codificacionPredios",parameters,null);
            }
            catch (Exception e)
            {
                Console.WriteLine("Error al ejecutar la herramienta: " + e.Message);
                // Manejo adicional de errores aquí
            }
        }
}

}
