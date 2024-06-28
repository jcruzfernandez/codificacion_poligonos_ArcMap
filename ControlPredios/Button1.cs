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
using ESRI.ArcGIS.Geoprocessing;
using ESRI.ArcGIS.DataSourcesGDB;
using System.Reflection;
using ESRI.ArcGIS.GeoprocessingUI;
using ESRI.ArcGIS.Geodatabase;

namespace ControlPredios
{
    public class Button1 : ESRI.ArcGIS.Desktop.AddIns.Tool
    {
        //IActiveView vista;
        //IPoint punto;
        private static string currentPath = System.IO.Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location);
        string pathToolbox = System.IO.Path.Combine(currentPath, "scripts", "ToolboxMain.tbx");
        public Button1()
        {
        }

        protected override void OnActivate()
        {
            // Mostrar cuadro de diálogo de advertencia
            DialogResult result = MessageBox.Show("¿Desea codificar todos los polígonos de la capa?", "Advertencia", MessageBoxButtons.YesNo, MessageBoxIcon.Warning);
            // Si el usuario selecciona 'No', salir del método
            if (result == DialogResult.No)
            {
                return;
            }
            try
            {
                // Obtener la ruta del directorio del ensamblado
                // Cargar la toolbox y la herramienta
                IWorkspaceFactory toolboxWorkspaceFactory = new ToolboxWorkspaceFactoryClass();
                IToolboxWorkspace toolboxWorkspace = toolboxWorkspaceFactory.OpenFromFile(System.IO.Path.Combine(currentPath, "scripts"), 0) as IToolboxWorkspace;
                IGPToolbox gpToolbox = toolboxWorkspace.OpenToolbox("ToolboxMain.tbx");
                IGPTool gpTool = gpToolbox.OpenTool("codificacionPrediosTotal");

                // Cargar la toolbox
                IGPToolCommandHelper2 gpToolCommandHelper = new GPToolCommandHelperClass() as IGPToolCommandHelper2; // GPToolCommandHelperClass();
                gpToolCommandHelper.SetTool(gpTool); // Reemplaza el nombre de tu herramienta de script
                bool pOK = true;
                IGPMessages pInvokeMessages = new GPMessagesClass();
                // Crea un objeto IVariantArray para almacenar los parámetros de la herramienta
                IArray parameters = gpTool.ParameterInfo;
                //parameters.RemoveAll();
                parameters.Add(ComboBox1.selectedLayerName);
                parameters.Add(ComboBox_field.selectedFieldName);
                //// Mostrar el diálogo de la herramienta
                gpToolCommandHelper.InvokeModal(0, parameters, out pOK, out pInvokeMessages);
            }
            catch (Exception ex)
            {
                System.Windows.Forms.MessageBox.Show("Error al ejecutar la herramienta: " + ex.Message);
            }
        }
        protected override void OnUpdate()
        {
            Enabled = ComboBox_field.selectedFieldName != null;
        }
    }
}
