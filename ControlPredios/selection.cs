using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using System.Text;
using ESRI.ArcGIS.Carto;
using ESRI.ArcGIS.Geometry;
using ESRI.ArcGIS.Display;
using ESRI.ArcGIS.Framework;
using ESRI.ArcGIS.ArcMapUI;

namespace ControlPredios
{
    [Guid("e5a7b1f5-8ec6-4bb2-a8f0-5a7f153ed01c")]
    [ClassInterface(ClassInterfaceType.None)]
    [ProgId("ControlPredios.selection")]
    public class selection : ESRI.ArcGIS.Desktop.AddIns.Tool
    {
        IActiveView vista;
        IPoint punto;
        public selection()
        {
            System.Windows.Forms.Cursor.Current = System.Windows.Forms.Cursors.Cross;
        }

        protected override void OnMouseDown(MouseEventArgs arg)
        {

            // Asegúrate de que el clic sea con el botón izquierdo del mouse
            if (arg.Clicks != 1) return;

            vista = ArcMap.Document.ActivatedView;
            // Convertir las coordenadas de pantalla a coordenadas del mapa
            punto = vista.ScreenDisplay.DisplayTransformation.ToMapPoint(arg.X, arg.Y);

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
