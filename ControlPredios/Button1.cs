using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Windows.Forms;
using ESRI.ArcGIS.ArcMapUI;
using ESRI.ArcGIS.Carto;
using ESRI.ArcGIS.Desktop.AddIns;
using ESRI.ArcGIS.esriSystem;
using ESRI.ArcGIS.Framework;
using ESRI.ArcGIS.Geometry;
using ESRI.ArcGIS.Display;

namespace ControlPredios
{
    public class Button1 : ESRI.ArcGIS.Desktop.AddIns.Tool
    {
        IActiveView vista;
        IPoint punto;

        public Button1()
        {
        }

        protected override void OnMouseDown(MouseEventArgs arg)
        {
            vista = ArcMap.Document.ActivatedView;
            punto = vista.ScreenDisplay.DisplayTransformation.ToMapPoint(arg.X, arg.Y);
            ArcMap.Document.FocusMap.SelectByShape(punto, null, false);
            vista.PartialRefresh(esriViewDrawPhase.esriViewGeoSelection, null, null);
        }
        protected override void OnUpdate()
        {

        }
    }

}
