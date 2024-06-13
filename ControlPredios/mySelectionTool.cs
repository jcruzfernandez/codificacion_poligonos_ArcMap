using ESRI.ArcGIS.esriSystem;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using ESRI.ArcGIS.Carto;
using ESRI.ArcGIS.Geometry;
using ESRI.ArcGIS.Display;
using ESRI.ArcGIS.Framework;
using ESRI.ArcGIS.ArcMapUI;
using System.Runtime.InteropServices;

namespace ControlPredios
{
    //[ProgId("ControlPredios.mySelectionTool")]
    public class mySelectionTool: ESRI.ArcGIS.Desktop.AddIns.Tool
    {
        
        public mySelectionTool()
        {
            this.Cursor = System.Windows.Forms.Cursors.Cross;
        }

        public void OnMouseDown(int Button, int Shift, int X, int Y)
        {
            // Asegúrate de que el clic sea con el botón izquierdo del mouse
            if (Button != 1) return;

            IMxDocument mxDocument = ArcMap.Application.Document as IMxDocument;
            IMap map = mxDocument.FocusMap;

            // Convertir las coordenadas de pantalla a coordenadas del mapa
            IPoint clickPoint = mxDocument.ActiveView.ScreenDisplay.DisplayTransformation.ToMapPoint(X, Y);

            // Aquí puedes implementar la lógica para seleccionar el elemento vectorial bajo el punto de clic
            // Por ejemplo, utilizando IFeatureLayer y IQueryFilter para seleccionar features basados en una ubicación
        }

        public new bool Enabled
        {
            get
            {
                // La herramienta estará habilitada si hay un documento de mapa abierto
                return ArcMap.Application != null;
            }
        }
    }
}
